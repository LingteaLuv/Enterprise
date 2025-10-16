using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JHT;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;
using UnityEngine.AddressableAssets;

public class BossBattleManager : Singleton<BossBattleManager>
{

    [Header("보스 스폰 위치 설정")]
    [SerializeField] private GameObject bossPos; // 팀원 방식 사용

    [SerializeField] private GameObject monsterPrefab;
    
    
    private Transform monsterParent;

    private List<GameObject> currentPlayers;
    public CameraFollow cameraFollow;

    public static bool IsBossBattle = false;
    private readonly string _returnSceneName = "Game";
    [SerializeField] private float startDelay = 3f; // 몇 초 뒤에 켜줄지

    private JHT_MonsterSpawnManager monsterSpawnManager;
    public List<JHT_BaseMonsterStat> spawnBossMonster;
    public BossBattleProduct product;
    public BossBattleDirection direction;
    
    [Header("튜토리얼 시그널 (CSJ)")]
    [SerializeField] private TutorialEventSO WinTutorial;
    [SerializeField] private TutorialEventSO LoseTutorial;
    [SerializeField] private TutorialEventSO EnterTutorial;

    public Action<JHT_BaseMonsterStat> OnDieMonster;

    CancellationTokenSource[] token = new CancellationTokenSource[2];

    Coroutine delay1;
    Coroutine delay2;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Battle()
    {
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        BattleAsync().Forget();
        //StartCoroutine(EnableMeleeAfterDelay());
    }

    private async UniTask BattleAsync()
    {
        try
        {
            RefreshCameraTargets();
            for (int i = 0; i < token.Length; i++)
            {
                if(token[i] == null)
                    token[i] = new();
            }


            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token[0].Token);

            product = FindFirstObjectByType<BossBattleProduct>();
            direction = FindFirstObjectByType<BossBattleDirection>();
            

            OnDieMonster += HandleBossDefeated;

            cameraFollow = Camera.main?.GetComponent<CameraFollow>();
            monsterSpawnManager = JHT_MonsterSpawnManager.Instance;


            await DataLoad();

            SpawnPlayers();
            SpawnBoss();
            product.Init();
            
            if (GlobalStageManager.Instance.isTutorialWin || GlobalStageManager.Instance.isTutorialLose)
            {
                EnterTutorial.Raise();
            }

            if (GlobalStageManager.Instance.isTutorialWin)
            {
                EndBattle(true);
            }

            if (GlobalStageManager.Instance.isTutorialLose)
            {
                EndBattle(false);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async UniTask DataLoad()
    {
        var monsterTransformHandle = await Addressables.LoadAssetAsync<GameObject>("MonsterTransform");
        var monsterSampleHandle = await Addressables.LoadAssetAsync<GameObject>("SampleMonster");
        var win = await Addressables.LoadAssetAsync<TutorialEventSO>("WinSignal");
        var lose = await Addressables.LoadAssetAsync<TutorialEventSO>("DefeatSignal");
        var enter = await Addressables.LoadAssetAsync<TutorialEventSO>("EnterBossSignal");

        bossPos = monsterTransformHandle;
        monsterPrefab = monsterSampleHandle;
        monsterParent = product.transform.Find("MonsterShip").transform;
        
        WinTutorial = win;
        LoseTutorial = lose;
        EnterTutorial = enter;
        //bossPos.transform.position += new Vector3(6, 0);
    }

    private void RefreshCameraTargets()
    {
        if (cameraFollow == null)
            cameraFollow = Camera.main?.GetComponent<CameraFollow>();

        if (cameraFollow != null)
        {
            var playerTransforms = PartyManager.Instance.GetAllPartyMembers()
                .Select(p => p.transform)
                .ToList();

            cameraFollow.StartFollowing(playerTransforms);
        }

    }

    #region 플레이어 스폰
    private void SpawnPlayers()
    {
        currentPlayers = new();

        IsBossBattle = true;
        var party = PartyManager.Instance.GetAllPartyMembers();

        foreach (var character in party.Select((ch, i) => new { ch, i }))
        {
            var c = character.ch;
            c.Initialize(c.CharacterStats);
            c.transform.position = new Vector3(character.i * 0.3f - 9.5f, -2f, 0);
            c.gameObject.SetActive(true);

            var fsm = c.GetComponent<BaseCharacterFSM>();
            if (fsm != null)
            {
                fsm.enabled = true;
                fsm.ChangeStateIdleForce();
            }

            // MeleeCharacter 스크립트 비활성화
            var melee = c.GetComponent<MeleeCharacter>();
            if (melee != null) melee.enabled = false;


            currentPlayers.Add(c.gameObject);
        }

        if (cameraFollow != null)
            cameraFollow.SetTargets(currentPlayers.Select(p => p.transform).ToList());
    }
    #endregion

    #region 보스 스폰
    private void SpawnBoss()
    {
        if (!monsterSpawnManager.isMonsterDataSetReady ||
            !monsterSpawnManager.isSkillSetReady ||
            monsterSpawnManager.roundTable == null)
        {
            return;
        }

        spawnBossMonster = new();
        if (monsterSpawnManager.curMonsterCountList.Count > 0)
            monsterSpawnManager.curMonsterCountList.Clear();

        // 랜덤 보스 선택
        int randBoss = UnityEngine.Random.Range(0, monsterSpawnManager.roundTable.captinMonsterData.Count);

        // 보스 포지션 프리팹 생성
        JHT_MonsterSetManager bossPosSpawn = Instantiate(bossPos).GetComponent<JHT_MonsterSetManager>();

        // 기본 몬스터 리스트 + 보스 추가
        monsterSpawnManager.curMonsterCountList = monsterSpawnManager.GetMonsterDataList(
            GlobalStageManager.Instance.CurrentStageIndex.Value - 1, true, 6);

        var captinData = monsterSpawnManager.DataSettiing(randBoss, true);
        monsterSpawnManager.curMonsterCountList.Add(captinData);

        spawnBossMonster = monsterSpawnManager.curMonsterCountList;

        // 몬스터 생성
        for (int i = 0; i < spawnBossMonster.Count; i++)
        {
            JHT_BaseMonsterFSM obj = Instantiate(monsterPrefab).GetComponent<JHT_BaseMonsterFSM>();
            obj.Init(spawnBossMonster[i], SpawnType.BossStage);
            obj.transform.position = bossPosSpawn.SetPos(spawnBossMonster[i]).position;
            obj.transform.SetParent(monsterParent);
            obj.GetComponent<JHT_NormalMonster>().enabled = false;
        }
        
    }
    #endregion

    #region 승패 처리


    private void HandleBossDefeated(JHT_BaseMonsterStat stat)
    {
        // 보스 몬스터 다 죽으면 전투 종료
        
        spawnBossMonster.Remove(stat);

        if (spawnBossMonster.Count <= 0)
        {
            EndBattle(true);
        }

    }

    public void OnPlayerDead(GameObject player)
    {
        if (currentPlayers.Contains(player))
            currentPlayers.Remove(player);

        if (currentPlayers.Count == 0)
            EndBattle(false);
    }

    private void EndBattle(bool isVictory)
    {
        cameraFollow?.SetBattleActive(false);
        direction.defeatText.gameObject.SetActive(isVictory);
        direction.victoryText.gameObject.SetActive(isVictory);

        IsBossBattle = false;
        monsterParent = null;
        spawnBossMonster.Clear();
        currentPlayers.Clear();

        if (isVictory)
            direction.PlayVictoryDirection();
        else
            direction.PlayDefeatDirection();

        product.LoseProduct(isVictory);
        // 카메라 추적 해제
        if (cameraFollow != null)
            cameraFollow.SetTargets(new List<Transform>());

        // FSM/입력 정지
        foreach (var player in currentPlayers)
            player?.GetComponent<BaseCharacterFSM>()?.ChangeStateIdleForce();

        for (int i = 0; i < token.Length; i++)
        {
            token[i].Cancel();
            token[i].Dispose();
            token[i] = null;
        }

        if (isVictory)
        {

            // 스테이지 클리어 기록
            QuestSignalManager.Instance.StageClear(GlobalStageManager.Instance.CurrentStageIndex.Value);

            if (GlobalStageManager.Instance.CurrentStageIndex.Value < 4)
                GlobalStageManager.Instance.CurrentStageIndex.Value++;
            else
                GlobalStageManager.Instance.CurrentStageIndex.Value = 4;

            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;
            GlobalStageManager.Instance.bossBattleTriggered = false;

            QuestSignalManager.Instance.KillEnemy(MonsterId.Boss);

            StartCoroutine(Delay1());
        }
        else
        {
            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;
            GlobalStageManager.Instance.bossBattleTriggered = false;
            StartCoroutine(Delay2());
        }

        OnDieMonster -= HandleBossDefeated;
    }

    private IEnumerator Delay1()
    {
        yield return new WaitForSeconds(8f);
        cameraFollow = null;
        SceneManager.LoadScene(_returnSceneName);
        
        if (GlobalStageManager.Instance.isTutorialWin)
        {
            WinTutorial.Raise();
        }

        if (delay1 != null)
        {
            StopCoroutine(delay1);
            delay1 = null;
        }

    }

    private IEnumerator Delay2()
    {
        yield return new WaitForSeconds(8f);
        cameraFollow = null;
        SceneManager.LoadScene(_returnSceneName);

        if (GlobalStageManager.Instance.isTutorialLose)
        {
            LoseTutorial.Raise();
        }
        
        if (delay2 != null)
        {
            StopCoroutine(delay2);
            delay2 = null;
        }
    }
    #endregion

    public void EnableMeleeAfterDelay()
    {

        foreach (var player in currentPlayers)
        {
            var melee = player.GetComponent<MeleeCharacter>();
            if (melee != null)
            {
                melee.enabled = true;
            }
        }

    }

    public void EnableMonsterAfterDelay()
    {
        for (int i = 0; i < monsterParent.childCount; i++)
        {
            var m = monsterParent.GetChild(i);
            if (m.TryGetComponent(out JHT_BaseMonsterFSM obj))
            {
                obj.enabled = true;
            }
            else
            {
                continue;
            }
        }

    }
}