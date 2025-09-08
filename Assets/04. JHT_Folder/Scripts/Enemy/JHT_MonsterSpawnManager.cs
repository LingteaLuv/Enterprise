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
        [SerializeField] private JHT_MonsterDataTable monsterTable;

        // List<JHT_MonsterDataTable> : 1-1, 1-2에서 -1 -2를 나눔
        [SerializeField] List<JHT_MonsterDataTable> roundList;
        
        // int : 스테이지,  JHT_MonsterDataTable은 몬스터의 정보를 stirng(int로 변경해도 됨)을 가져와서 해당 데이터의 정보를 로드할거임
        // => 데이터 정보를 가져오는건 미리 addressable에서 로드한 모든 몬스터 데이터를 매 스테이지시 로드하는 걸로 하는게 좋아보임(fade in,out이 스테이지당 하는거같음)
        Dictionary<int, List<JHT_MonsterDataTable>> stageDic;


        

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

        public void Init()
        {
            // MonsterDataLoad 스크립트에서 로드된 데이터를 가져와서 roundList, roundDic에 저장


        }

        // globalManager?에서 currentIndex++시 이벤트 - 데이터 넣기
        public void ChangeStage(int stageIndex)
        {
            roundList.Clear();
            for (int i = 0; i < stageDic[stageIndex].Count; i++)
            {
                roundList.Add(stageDic[stageIndex][i]);
            }
        }
        
        // 몬스터 스폰하기
        public void ChangeRound(int roundIndex)
        {
            JHT_MonsterDataTable inst = roundList[roundIndex];
            for (int i = 0; i < inst.monsterData.Count; i++)
            {
                JHT_BaseMonsterFSM obj = monsterPool.GetPooled() as JHT_BaseMonsterFSM;
                obj.Init(obj.monsterStat);

                //어디에 스폰될지 위치 정해줘야함
                obj.transform.position = new Vector2(0, 0);
            }
        }


    }
}
