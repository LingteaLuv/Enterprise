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

        [SerializeField] private JHT_MonsterDataTable roundTable;

        //[SerializeField] List<JHT_MonsterDataTable> roundList;

        // int : 스테이지,  JHT_MonsterDataTable은 몬스터의 정보를 stirng(int로 변경해도 됨)을 가져와서 해당 데이터의 정보를 로드할거임
        // => 데이터 정보를 가져오는건 미리 addressable에서 로드한 모든 몬스터 데이터를 매 스테이지시 로드하는 걸로 하는게 좋아보임(fade in,out이 스테이지당 하는거같음)
        Dictionary<int, JHT_MonsterDataTable> stageDic;
        public List<JHT_MonsterSetManager> posList;

        // Demo
        [SerializeField] private JHT_MonsterDataTable sampleDataList1;
        [SerializeField] private JHT_MonsterDataTable sampleDataList2;
        [SerializeField] private JHT_MonsterDataTable sampleDataList3;
        [SerializeField] private JHT_MonsterDataTable sampleDataList4;
        [SerializeField] private JHT_MonsterDataTable sampleDataList5;

        public int stageIndex;
        public int roundIndex;
        public int curTotalCount;

        public Func<int,List<JHT_MonsterDataSO>> OnAddMonster;
        protected override void Awake()
        {
            base.Awake();

            GameObject projectilePoolParent = new GameObject($"{monsterProjectile.name} Pool_Parent");
            projectilePoolParent.transform.SetParent(transform);

            GameObject monsterPoolParent = new GameObject($"{monsterPrefab.name} Pool_Parent");
            monsterPoolParent.transform.SetParent(transform);

            GameObject damageTextPoolParent = new GameObject($"{damageTextPrefab.name} Pool_Parent");
            damageTextPoolParent.transform.SetParent(transform);

            monsterPool = new JHT_ObjectPool(monsterPrefab, 10, monsterPoolParent.transform);
            projectilePool = new JHT_ObjectPool(monsterProjectile, 20, projectilePoolParent.transform);
            damageTextPool = new JHT_ObjectPool(damageTextPrefab, 20, damageTextPoolParent.transform);
        }

        private void Start()
        {
            stageDic = new();
            posList = new();
            Init();
        }

        public void Init()
        {
            // MonsterDataLoad 스크립트에서 로드된 데이터를 가져와서 roundList, roundDic에 저장
            stageDic.Add(0, sampleDataList1);
            stageDic.Add(1, sampleDataList2);
            stageDic.Add(2, sampleDataList3);
            stageDic.Add(3, sampleDataList4);
            stageDic.Add(4, sampleDataList5);

            stageIndex = -1;

            OnAddMonster += SetSpawnRound;
        }

        protected override void OnDestroy()
        {
            OnAddMonster -= SetSpawnRound;
            base.OnDestroy();
        }

        public void ChangeStage()
        {
            stageIndex++;
            roundIndex = 0;
            if (stageIndex < 0)
               return;

            if(posList.Count > 0)
                posList.Clear();

            roundTable = stageDic[stageIndex];


            for (int i = 0; i < roundTable.monsterGroupPos.Count; i++)
            {
                roundTable.monsterPosData[i].transform.position = roundTable.monsterGroupPos[i];
            }

            for (int i = 0; i < roundTable.roundCount; i++)
            {
                int rand = UnityEngine.Random.Range(0, roundTable.monsterPosData.Count);
                posList.Add(roundTable.monsterPosData[rand]);

                if (i != 0 && posList[i] == posList[i - 1])
                {
                    posList.RemoveAt(i);
                    i--;
                }
            }

            curTotalCount = roundTable.totalCost;
            SpawnMonster(0);
        }

        // curStageIndex를 ++을 통해 round를 구별해줄거임
        public void SpawnMonster(int curStageIndex)
        {
            if (posList.Count < curStageIndex)
            {
                Debug.LogError("현재 라운드 최대를 넘었음");
                return;
            }

            List<JHT_MonsterDataSO> dataList = OnAddMonster?.Invoke(curStageIndex);

            posList[curStageIndex].checkList = new();


            for (int i = 0; i < dataList.Count; i++)
            {
                JHT_BaseMonsterFSM obj = monsterPool.GetPooled() as JHT_BaseMonsterFSM;
                obj.Init(dataList[i]);
                obj.transform.position = posList[curStageIndex].SetPos(dataList[i]).position;
            }
        }


        // 비동기식으로 다음 라운드의 적 미리 생성해두면 빠를듯
        // 현재 라운드의 totalCost를 통해 랜덤으로 몬스터 가져오기
        public List<JHT_MonsterDataSO> SetSpawnRound(int curRoundIndex)
        {
            int count = 0;
            List<JHT_MonsterDataSO> dataList = new();
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

                    dataList.Add(roundTable.monsterData[rand]);
                }

            }
            return dataList;
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
