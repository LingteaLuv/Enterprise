using System;
using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    public class JHT_MonsterSpawnManager : Singleton<JHT_MonsterSpawnManager>
    {
        public JHT_ObjectPool projectilePool;
        JHT_ObjectPool monsterPool;

        [SerializeField] private JHT_BaseMonsterFSM monsterPrefab;
        [SerializeField] private JHT_MonsterProjectile monsterProjectile;
        [SerializeField] private JHT_MonsterDataTable roundTable;
        [SerializeField] private JHT_MonsterSetManager monsterSetTransform;

        //[SerializeField] List<JHT_MonsterDataTable> roundList;

        // int : 스테이지,  JHT_MonsterDataTable은 몬스터의 정보를 stirng(int로 변경해도 됨)을 가져와서 해당 데이터의 정보를 로드할거임
        // => 데이터 정보를 가져오는건 미리 addressable에서 로드한 모든 몬스터 데이터를 매 스테이지시 로드하는 걸로 하는게 좋아보임(fade in,out이 스테이지당 하는거같음)
        Dictionary<int, JHT_MonsterDataTable> stageDic;

        // Demo
        [SerializeField] private JHT_MonsterDataTable sampleDataList1;
        [SerializeField] private JHT_MonsterDataTable sampleDataList2;
        [SerializeField] private JHT_MonsterDataTable sampleDataList3;
        [SerializeField] private JHT_MonsterDataTable sampleDataList4;
        [SerializeField] private JHT_MonsterDataTable sampleDataList5;

        public int stageIndex;
        public int roundIndex;
        public int curTotalCount;
        public int curSpawnIndex;

        public Func<int,List<JHT_MonsterDataSO>> OnAddMonster;
        protected override void Awake()
        {
            base.Awake();

            GameObject projectilePoolParent = new GameObject($"{monsterProjectile.name} Pool_Parent");
            projectilePoolParent.transform.SetParent(transform);

            GameObject monsterPoolParent = new GameObject($"{monsterPrefab.name} Pool_Parent");
            monsterPoolParent.transform.SetParent(transform);

            monsterPool = new JHT_ObjectPool(monsterPrefab, 10, monsterPoolParent.transform);
            projectilePool = new JHT_ObjectPool(monsterProjectile, 20, projectilePoolParent.transform);
        }

        private void Start()
        {
            stageDic = new();
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
            roundIndex = 0;
            curSpawnIndex = 0;

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

            if (stageIndex < 0)
               return;

            roundTable = stageDic[stageIndex];
            SpawnMonster(stageIndex);
        }

        public void SpawnMonster(int curStageIndex)
        {
            List<JHT_MonsterDataSO> dataList = OnAddMonster?.Invoke(curStageIndex);

            for (int i = 0; i < dataList.Count; i++)
            {
                JHT_BaseMonsterFSM obj = monsterPool.GetPooled() as JHT_BaseMonsterFSM;
                obj.Init(roundTable.monsterData[i]);
            }
        }


        // 현재 라운드의 totalCost를 통해 랜덤으로 몬스터 가져오기
        public List<JHT_MonsterDataSO> SetSpawnRound(int curRoundIndex)
        {
            curSpawnIndex = curRoundIndex;
            int count = 0;
            List<JHT_MonsterDataSO> dataList = new();

            while (count < curTotalCount)
            {
                int rand = UnityEngine.Random.Range(0, roundTable.monsterData.Count);
                count += roundTable.monsterData[rand].cost;
                if (count > curTotalCount)
                {
                    count -= roundTable.monsterData[rand].cost;
                    continue;
                }
                else if (count == curTotalCount)
                {
                    dataList.Add(roundTable.monsterData[rand]);
                    break;
                }
                else
                {
                    dataList.Add(roundTable.monsterData[rand]);
                }
            }

            return dataList;
        }

        

        //public JHT_MonsterDataSO SetSO()
        //{
        //    foreach (var d in roundList)
        //    {
        //
        //    }
        //
        //    return
        //}


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
