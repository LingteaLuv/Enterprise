using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Dictionary<int, JHT_MonsterDataTable> stageDic;
        public List<JHT_MonsterSetManager> posList;
        public List<JHT_BaseMonsterStat> curMonsterCountList =new ();
        Dictionary<AtkRangeType, AnimatorOverrideController> aocDic;


        public int islandIndex;
        public int roundIndex;
        public int curTotalCount;

        private bool isMonsterDataSetReady;
        private bool isSkillSetReady;
        
        private GameObject projectilePoolParent;
        private GameObject monsterPoolParent;
        private GameObject damageTextPoolParent;
        public GameObject spawnPosParent;


        public Func<int,List<JHT_BaseMonsterStat>> OnAddMonster;

        MonsterDataManager monsterDataManager;

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
            aocDic = new();

            isMonsterDataSetReady = false;
            isSkillSetReady = false;

            OnAddMonster += SetSpawnRound;
            monsterDataManager.OnMonsterTableLoadFinish += DataStageLoad;
            monsterDataManager.OnMonsterSkillLoadFinish += SkillDataLoad;
        }

        protected override void OnDestroy()
        {
            OnAddMonster -= SetSpawnRound;
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


        public void ChangeIsland(BattleField field,int _islandIndex)
        {
            roundIndex = 0;
            if (_islandIndex < 0)
               return;

            if(posList.Count > 0)
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

            curTotalCount = roundTable.totalCost;
        }

        // curStageIndex를 ++을 통해 round를 구별해줄거임
        public void ChangeRound(int curRoundIndex)
        {
            if (posList.Count < curRoundIndex)
            {
                Debug.LogError("현재 라운드 최대를 넘었음");
                return;
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
                if (curRoundIndex / 2 != 0)
                {
                    obj.transform.localEulerAngles =
                        new Vector3(obj.transform.localEulerAngles.x, 180, obj.transform.localEulerAngles.x);
                }
            }
        }


        // 비동기식으로 다음 라운드의 적 미리 생성해두면 빠를듯
        // 현재 라운드의 totalCost를 통해 랜덤으로 몬스터 가져오기
        public List<JHT_BaseMonsterStat> SetSpawnRound(int curRoundIndex)
        {
            if (!(isMonsterDataSetReady && isSkillSetReady))
                return null;

            int count = 0;
            // 메모리 낭비아닌가?
            List<JHT_BaseMonsterStat> dataList = new();
            Dictionary<CrewRole, int> crewRoleCounter = new();

            float roundStart = Time.realtimeSinceStartup;

            // 다음 스테이지나 씬으로 연결
            if (curRoundIndex > roundTable.roundCount)
            {
                Debug.LogError("라운드 넘어감");
                return null;
            }

            while (count != curTotalCount)
            {
                int rand = UnityEngine.Random.Range(0, roundTable.monsterData.Count);
                count += roundTable.monsterData[rand].cost;

                if (Time.realtimeSinceStartup - roundStart > 2f)
                {
                    Debug.LogError($"[SetSpawnRound] 시간초과");
                    break;
                }


                if (count > curTotalCount)
                {
                    count -= roundTable.monsterData[rand].cost;
                    count -= dataList[dataList.Count - 1].cost;
                    dataList.RemoveAt(dataList.Count - 1);
                    continue;
                }
                else
                {
                    if (crewRoleCounter.TryGetValue(roundTable.monsterData[rand].monsterCrewRole, out int value))
                    {
                        if (value >= 2)
                        {
                            //여기서 중복된 CrewRole이 2개보다 많을 때 다른애를 뽑기위해 진행
                            count -= roundTable.monsterData[rand].cost;
                            continue;
                        }

                        crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = value + 1;
                    }
                    else
                    {
                        crewRoleCounter[roundTable.monsterData[rand].monsterCrewRole] = 1;
                    }
                    MonsterSkillSO normal = roundTable.monsterData[rand].normalSkill == "" ? 
                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].normalSkill];

                    MonsterSkillSO skill1 = roundTable.monsterData[rand].skill1 == "" ? 
                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill1];

                    MonsterSkillSO skill2 = roundTable.monsterData[rand].skill2 == "" ? 
                        null : monsterDataManager.monsterSkillDic[roundTable.monsterData[rand].skill2];

                    JHT_BaseMonsterStat stat = new(roundTable.monsterData[rand],normal,skill1,skill2);

                    if (roundIndex >= 5 && dataList.Find(a => a.monsterRarity == MonsterRarity.Elite) == null)
                    {
                        stat.monsterRarity = MonsterRarity.Elite;
                    }
                    else
                    {
                        stat.monsterRarity = MonsterRarity.Normal;
                    }

                    dataList.Add(stat);
                }

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
            curMonsterCountList.Clear();
        }

        private bool StageEndEvent(bool value)
        {
            return value;
        }

        #region 이전 버전
        // globalManager?에서 currentIndex++시 이벤트 - 데이터 넣기
        //public void ChangeStage(int stageIndex)
        //{
        //    if (stageIndex < 0)
        //        return;
        //
        //    roundList.Clear();
        //    for (int i = 0; i < stageDic[stageIndex].Count; i++)
        //    {
        //        roundList.Add(stageDic[stageIndex][i]);
        //    }
        //    ChangeRound(roundIndex); // demo
        //}
        //
        //// 몬스터 스폰하기
        //public void ChangeRound(int roundIndex)
        //{
        //    if (roundIndex < 0)
        //        return;
        //
        //    roundTable = roundList[roundIndex];
        //    curTotalCount = roundTable.totalCost;
        //    for (int i = 0; i < roundTable.monsterData.Count; i++)
        //    {
        //        JHT_BaseMonsterFSM obj = monsterPool.GetPooled() as JHT_BaseMonsterFSM;
        //        obj.Init(roundTable.monsterData[i]);
        //        obj.transform.position = roundTable.monsterPosData[i];
        //    }
        //}
        #endregion

    }
}
