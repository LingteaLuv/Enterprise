using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    public class JHT_MonsterSpawnManager : Singleton<JHT_MonsterSpawnManager>
    {
        public JHT_ObjectPool projectilePool;
        private JHT_ObjectPool monsterPool;
        public JHT_ObjectPool damageTextPool;

        [SerializeField] private JHT_BaseMonsterFSM monsterPrefab;
        [SerializeField] private JHT_MonsterProjectile monsterProjectile;
        [SerializeField] private JHT_DamageBox damageTextPrefab;
        public List<JHT_MonsterSetManager> monsterPosPrefab;
        private List<JHT_MonsterSetManager> monsterSamplePos;

        public JHT_MonsterDataTable roundTable;
        //[SerializeField] List<JHT_MonsterDataTable> roundList;

        // int : 스테이지,  JHT_MonsterDataTable은 몬스터의 정보를 stirng(int로 변경해도 됨)을 가져와서 해당 데이터의 정보를 로드할거임
        // => 데이터 정보를 가져오는건 미리 addressable에서 로드한 모든 몬스터 데이터를 매 스테이지시 로드하는 걸로 하는게 좋아보임(fade in,out이 스테이지당 하는거같음)
        public Dictionary<int, JHT_MonsterDataTable> stageDic;
        public List<JHT_MonsterSetManager> posList;
        public List<JHT_BaseMonsterStat> curMonsterCountList;

        public int islandIndex;
        public int roundIndex;

        private bool isMonsterDataSetReady;
        private bool isSkillSetReady;

        private GameObject projectilePoolParent;
        private GameObject monsterPoolParent;
        private GameObject damageTextPoolParent;
        private GameObject spawnPosParent;


        public event Func<int, List<JHT_BaseMonsterStat>> OnAddMonster;

        MonsterDataManager monsterDataManager;

        public Coroutine roundCor;

        protected override void Awake()
        {
            base.Awake();

            projectilePoolParent = new GameObject($"{monsterProjectile.name} Pool_Parent");
            projectilePoolParent.transform.SetParent(transform);

            monsterPoolParent = new GameObject($"{monsterPrefab.name} Pool_Parent");
            monsterPoolParent.transform.SetParent(transform);

            damageTextPoolParent = new GameObject($"{damageTextPrefab.name} Pool_Parent");
            damageTextPoolParent.transform.SetParent(transform);

            spawnPosParent = new GameObject("SpawnPos_Parent");
            spawnPosParent.transform.SetParent(transform);
            monsterSamplePos = new(monsterPosPrefab.Count);
            for (int i = 0; i < monsterPosPrefab.Count; i++)
            {
                JHT_MonsterSetManager obj = Instantiate(monsterPosPrefab[i], spawnPosParent.transform);
                monsterSamplePos.Add(obj);
            }

            monsterPool = new JHT_ObjectPool(monsterPrefab, 10, monsterPoolParent.transform);
            projectilePool = new JHT_ObjectPool(monsterProjectile, 20, projectilePoolParent.transform);
            damageTextPool = new JHT_ObjectPool(damageTextPrefab, 20, damageTextPoolParent.transform);

            monsterDataManager = MonsterDataManager.Instance;
        }

        private void OnEnable()
        {
            stageDic = new();
            posList = new();
            curMonsterCountList = new();

            isMonsterDataSetReady = false;
            isSkillSetReady = false;

            OnAddMonster += SetSpawnMonster;
            monsterDataManager.OnMonsterTableLoadFinish += DataStageLoad;
            monsterDataManager.OnMonsterSkillLoadFinish += SkillDataLoad;
        }

        protected override void OnDestroy()
        {

            OnAddMonster -= SetSpawnMonster;
            monsterDataManager.OnMonsterTableLoadFinish -= DataStageLoad;
            monsterDataManager.OnMonsterSkillLoadFinish -= SkillDataLoad;
            base.OnDestroy();
        }

        public void SkillDataLoad()
        {
            isSkillSetReady = true;
        }

        public void DataStageLoad()
        {
            isMonsterDataSetReady = true;
            // 현재 스테이지만 로드할지 아니면 전체 다 로드할지 - 현재는 전체 다 로드함
            for (int i = 0; i < monsterDataManager.monsterTableList.Count; i++)
            {
                stageDic.Add(i, monsterDataManager.monsterTableList[i]);
            }
        }


        public void ChangeIsland(BattleField field, int _islandIndex)
        {
            roundIndex = 0;
            if (_islandIndex < 0)
                return;

            if (posList.Count > 0)
                posList.Clear();

            islandIndex = _islandIndex;
            roundTable = stageDic[_islandIndex];


            for (int i = 0; i < monsterSamplePos.Count; i++)
            {
                monsterSamplePos[i].transform.position = field.EnemySpawnPoints[i].position;

            }

            for (int i = 0; i < roundTable.roundCount; i++)
            {
                int rand = UnityEngine.Random.Range(0, monsterSamplePos.Count);
                posList.Add(monsterSamplePos[rand]);

                if (i != 0 && posList[i] == posList[i - 1])
                {
                    posList.RemoveAt(i);
                    i--;
                }
            }

        }

        public void ChangeRound(int curRoundIndex)
        {

            if (roundCor == null)
            {
                roundCor = StartCoroutine(ChangeRoundCor(curRoundIndex));
            }
        }

        public void EndChangeRound()
        {
            if (roundCor != null)
            {
                StopCoroutine(roundCor);
                roundCor = null;
            }
        }

        // curStageIndex를 ++을 통해 round를 구별해줄거임
        private IEnumerator ChangeRoundCor(int curRoundIndex)
        {
            while (!isMonsterDataSetReady || !isSkillSetReady || roundTable == null)
            {
                yield return null;
            }

            if (curRoundIndex > posList.Count)
            {
                yield return null;
            }

            roundIndex = curRoundIndex;
            if (curMonsterCountList.Count > 0)
                curMonsterCountList.Clear();

            curMonsterCountList = OnAddMonster?.Invoke(curRoundIndex);

            posList[curRoundIndex].checkList = new();


            for (int i = 0; i < curMonsterCountList.Count; i++)
            {
                JHT_BaseMonsterFSM obj = monsterPool.GetPooled() as JHT_BaseMonsterFSM;
                obj.Init(curMonsterCountList[i]);
                obj.transform.position = posList[curRoundIndex].SetPos(curMonsterCountList[i]).position;
                
            }

        }


        public List<JHT_BaseMonsterStat> SetSpawnMonster(int curRoundIndex)
        {
            int count = 0;
            List<JHT_BaseMonsterStat> dataList = new();
            Dictionary<CrewRole, int> crewRoleCounter = new();

            float roundStart = Time.realtimeSinceStartup;

            // 다음 스테이지나 씬으로 연결 **************************************************** 제발
            if (curRoundIndex > roundTable.roundCount)
            {
                return null;
            }

            while (count <= 4)
            {
                int rand = UnityEngine.Random.Range(0, roundTable.monsterData.Count);

                //시간 경과가 마쳤음에도 랜덤값을 못뻈을경우
                if (Time.realtimeSinceStartup - roundStart > 1f)
                {
                    while (count <= 4)
                    {
                        for (int i = 0; i < roundTable.monsterData.Count; i++)
                        {
                            if (crewRoleCounter.TryGetValue(roundTable.monsterData[i].monsterCrewRole, out int data))
                            {
                                if (data < 2)
                                {
                                    MonsterSkillSO n = roundTable.monsterData[i].normalSkill == -1 ?
                                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].normalSkill];

                                    MonsterSkillSO s1 = roundTable.monsterData[i].skill1 == -1 ?
                                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].skill1];

                                    MonsterSkillSO s2 = roundTable.monsterData[i].skill2 == -1 ?
                                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].skill2];

                                    JHT_BaseMonsterStat s = new(roundTable.monsterData[i], n, s1, s2, roundTable.addStat);

                                    dataList.Add(s);

                                    if (count == 4)
                                        break;
                                    else
                                        count++;
                                }
                            }
                        }
                    }

                    break;
                }



                if (crewRoleCounter.TryGetValue(roundTable.monsterData[rand].monsterCrewRole, out int value))
                {
                    if (value >= 2)
                    {
                        continue;
                    }

                    crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = value + 1;
                }
                else
                {
                    crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = 1;
                }
                MonsterSkillSO normal = roundTable.monsterData[rand].normalSkill == -1 ?
                    null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].normalSkill];

                MonsterSkillSO skill1 = roundTable.monsterData[rand].skill1 == -1 ?
                    null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill1];

                MonsterSkillSO skill2 = roundTable.monsterData[rand].skill2 == -1 ?
                    null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill2];

                JHT_BaseMonsterStat stat = new(roundTable.monsterData[rand], normal, skill1, skill2, roundTable.addStat);
                count++;

                // 조건에 따라 Elite 나오게하기
                //if (roundIndex >= 5 && dataList.Find(a => a.monsterRarity == MonsterRarity.Elite) == null)
                //{
                //    stat.monsterRarity = MonsterRarity.Elite;
                //}
                //else
                //{
                //    stat.monsterRarity = MonsterRarity.Normal;
                //}

                dataList.Add(stat);
            }
            return dataList;
        }


        public void MonsterAllClear()
        {
            for (int i = 0; i < monsterPoolParent.transform.childCount; i++)
            {
                if (monsterPoolParent.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    monsterPoolParent.transform.GetChild(i).GetComponent<JHT_BaseMonsterFSM>().Outit();
                }
            }

            for (int i = 0; i < projectilePoolParent.transform.childCount; i++)
            {
                if (projectilePoolParent.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    projectilePoolParent.transform.GetChild(i).GetComponent<JHT_MonsterProjectile>().Release();
                }
            }

            for (int i = 0; i < damageTextPoolParent.transform.childCount; i++)
            {
                if (damageTextPoolParent.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    damageTextPoolParent.transform.GetChild(i).GetComponent<JHT_DamageBox>().Release();
                }
            }
            EndChangeRound();
            curMonsterCountList.Clear();
        }



        #region cost로 몬스터 생성
        // 비동기식으로 다음 라운드의 적 미리 생성해두면 빠를듯
        // 현재 라운드의 totalCost를 통해 랜덤으로 몬스터 가져오기
        //public List<JHT_BaseMonsterStat> SetSpawnRound(int curRoundIndex)
        //{
        //    int count = 0;
        //    // 메모리 낭비아닌가?
        //    List<JHT_BaseMonsterStat> dataList = new();
        //    Dictionary<CrewRole, int> crewRoleCounter = new();

        //    float roundStart = Time.realtimeSinceStartup;

        //    // 다음 스테이지나 씬으로 연결
        //    if (curRoundIndex > roundTable.roundCount)
        //    {
        //        return null;
        //    }

        //    while (count != 6)
        //    {
        //        int rand = UnityEngine.Random.Range(0, roundTable.monsterData.Count);
        //        count += roundTable.monsterData[rand].cost;

        //        //시간 경과가 마쳤음에도 랜덤값을 못뻈을경우
        //        if (Time.realtimeSinceStartup - roundStart > 1f)
        //        {
        //            for (int i = 0; i < roundTable.monsterData.Count; i++)
        //            {
        //                if (crewRoleCounter.TryGetValue(roundTable.monsterData[i].monsterCrewRole, out int value))
        //                {
        //                    if (value < 2 && roundTable.monsterData[i].cost == 6 - count)
        //                    {
        //                        MonsterSkillSO normal = roundTable.monsterData[i].normalSkill == "" ?
        //                            null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].normalSkill];

        //                        MonsterSkillSO skill1 = roundTable.monsterData[i].skill1 == "" ?
        //                            null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].skill1];

        //                        MonsterSkillSO skill2 = roundTable.monsterData[i].skill2 == "" ?
        //                            null : monsterDataManager.monsterSkillDic[roundTable.monsterData[i].skill2];

        //                        JHT_BaseMonsterStat stat = new(roundTable.monsterData[i], normal, skill1, skill2, roundTable.addStat);

        //                        dataList.Add(stat);
        //                        break;
        //                    }
        //                }

        //            }

        //            break;
        //        }


        //        if (count > 6)
        //        {
        //            count -= roundTable.monsterData[rand].cost;
        //            count -= dataList[dataList.Count - 1].cost;
        //            dataList.RemoveAt(dataList.Count - 1);
        //            continue;
        //        }
        //        else
        //        {
        //            if (crewRoleCounter.TryGetValue(roundTable.monsterData[rand].monsterCrewRole, out int value))
        //            {
        //                if (value >= 2)
        //                {
        //                    //여기서 중복된 CrewRole이 2개보다 많을 때 다른애를 뽑기위해 진행
        //                    count -= roundTable.monsterData[rand].cost;
        //                    continue;
        //                }

        //                crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = value + 1;
        //            }
        //            else
        //            {
        //                crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = 1;
        //            }
        //            MonsterSkillSO normal = roundTable.monsterData[rand].normalSkill == "" ?
        //                null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].normalSkill];

        //            MonsterSkillSO skill1 = roundTable.monsterData[rand].skill1 == "" ?
        //                null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill1];

        //            MonsterSkillSO skill2 = roundTable.monsterData[rand].skill2 == "" ?
        //                null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill2];

        //            JHT_BaseMonsterStat stat = new(roundTable.monsterData[rand], normal, skill1, skill2, roundTable.addStat);

        //            // 조건에 따라 Elite 나오게하기
        //            //if (roundIndex >= 5 && dataList.Find(a => a.monsterRarity == MonsterRarity.Elite) == null)
        //            //{
        //            //    stat.monsterRarity = MonsterRarity.Elite;
        //            //}
        //            //else
        //            //{
        //            //    stat.monsterRarity = MonsterRarity.Normal;
        //            //}

        //            dataList.Add(stat);
        //        }

        //    }
        //    return dataList;
        //}

        #endregion
    }
}

