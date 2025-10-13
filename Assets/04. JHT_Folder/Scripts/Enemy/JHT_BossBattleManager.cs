using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JHT_BossBattleManager : Singleton<JHT_BossBattleManager>
{
    [Header("Demo")]
    [SerializeField] private Button battleButton;


    JHT_MonsterSpawnManager monsterSpawnManager;
    public JHT_MonsterSetManager bossPos;

    private int monsterCount;
    public int MonsterCount { get { return monsterCount; } set { monsterCount = value; OnMonsterCountChanged?.Invoke(monsterCount); } }
    public Action<int> OnMonsterCountChanged;

    public List<JHT_BaseMonsterStat> spawnBossMonster;
    protected override void Awake()
    {
        base.Awake();
        
    }

    private void OnEnable()
    {
        battleButton.onClick.AddListener(delegate { SetBossStage(GlobalStageManager.Instance.CurrentStageIndex.Value - 1); });
        OnMonsterCountChanged += MonsterCountChanged;
        if(spawnBossMonster != null)
            spawnBossMonster.Clear();

        spawnBossMonster = new();

        
    }

    private void OnDisable()
    {
        battleButton.onClick.RemoveListener(delegate { SetBossStage(GlobalStageManager.Instance.CurrentStageIndex.Value - 1); });
        OnMonsterCountChanged -= MonsterCountChanged;
    }

    private void Start()
    {
        monsterSpawnManager = JHT_MonsterSpawnManager.Instance;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    public void SetBossStage(int _stageIndex)
    {
        if (!monsterSpawnManager.isMonsterDataSetReady || !monsterSpawnManager.isSkillSetReady || monsterSpawnManager.roundTable == null)
        {
            Debug.LogError("테이블, 스킬, 데이터 세팅 미완료");
            return;
        }

        if (monsterSpawnManager.curMonsterCountList.Count > 0)
            monsterSpawnManager.curMonsterCountList.Clear();

        int randBoss = UnityEngine.Random.Range(0, monsterSpawnManager.roundTable.captinMonsterData.Count);


        JHT_MonsterSetManager bossPosSpawn = Instantiate(bossPos);
        monsterSpawnManager.curMonsterCountList = monsterSpawnManager.GetMonsterDataList(_stageIndex, true, 6);

        var captinData = monsterSpawnManager.DataSettiing(randBoss, true);
        monsterSpawnManager.curMonsterCountList.Add(captinData);

        spawnBossMonster = monsterSpawnManager.curMonsterCountList;
        for (int i = 0; i < monsterSpawnManager.curMonsterCountList.Count; i++)
        {
            JHT_BaseMonsterFSM obj = monsterSpawnManager.monsterPool.GetPooled() as JHT_BaseMonsterFSM;
            obj.Init(spawnBossMonster[i], SpawnType.BossStage);
            obj.transform.position = bossPosSpawn.SetPos(spawnBossMonster[i]).position;
        }
    }



    private void MonsterCountChanged(int value)
    {
        if (value <= 0)
        {
            BossWin();
        }
    }

    private void BossWin()
    {
        Debug.LogError("보스전 이김");
    }
}
