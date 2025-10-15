using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GooglePlayGames.BasicApi;
using JHT;
using System.Linq;
using System;


/// <summary>
/// 전투 전체 흐름을 관리하는 매니저
/// - 플레이어와 적 유닛의 스폰
/// - 전투 시작 및 종료 조건 판별
/// - 스킵 버튼 제어
/// - 전투 패배 처리 및 리셋
/// - BattleField 위치 관리 및 카메라 연동
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private Coroutine battleRoutine;

    private Button _skipBtn;
    private CameraFollow cameraFollow;

    protected PartyManager partymanager;

    [Header("스폰 프리팹")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("전투 필드")]
    private List<BattleField> battleFields = new();             // 동적으로 필드 구성 (씬 로드시 찾아서 등록)

    [Header("스폰 수 설정")]
    [SerializeField] private int growthPerStage = 1;

    private List<GameObject> currentPlayers = new();  // 기존 currentPlayer 대신 여러 명

    public List<JHT_BaseMonsterStat> spawnedEnemies = new(); // GameObject -> JHT_MOnsterDataSO

    public int currentRoundIndex = 0;

    private bool isbattleover = false;

    private bool isInitialized = false;

    private bool isHandlingDefeat = false;

    [SerializeField] private AllHealthBarsPanel allHealthBarsPanel;

    #region JHT
    private bool goNextIsland = false;

    private bool isLandEnd;
    public bool IslandEnd { get { return isLandEnd; } set { isLandEnd = value; OnStageEnd?.Invoke(isLandEnd); } }
    public Action<bool> OnStageEnd;

    private Coroutine enemySpawnDelay;
    #endregion
    
    #region CSJ
    public Action OnBattleStart;
    #endregion
    private void Awake()
    {
        Instance = this;    // 싱글톤 등록
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnStageEnd += SetBattleStop;

        PartyManager.OnPartyReady += OnPartyFormationSaved;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        OnStageEnd -= SetBattleStop;

        PartyManager.OnPartyReady -= OnPartyFormationSaved;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 지정된 씬에서만 초기화
        if (scene.name == "Game" && !isInitialized)
        {
            isInitialized = true;

            InitUI();
            InitBattleFields();
            ClearPlayers();

            if (_skipBtn != null)
            {
                _skipBtn.onClick.AddListener(Skip);
                _skipBtn.interactable = false;
            }
            else
            {
                Debug.LogWarning(" SkipButton을 찾을 수 없습니다.");
            }
        }
    }

    // UI 초기화 (버튼, 카메라 컴포넌트 등)
    private void InitUI()
    {
        GameObject skipBtnObj = GameObject.Find("SkipButton");
        if (skipBtnObj != null)
        {
            _skipBtn = skipBtnObj.GetComponent<Button>();
        }

        cameraFollow = Camera.main?.GetComponent<CameraFollow>();
    }

    // 전투 필드 리스트 초기화 (하이어라키에서 BattleFields 하위 오브젝트 검색)
    private void InitBattleFields()
    {
        battleFields.Clear();
        Transform fieldRoot = GameObject.Find("BattleFields")?.transform;

        if (fieldRoot != null)
        {
            foreach (Transform child in fieldRoot)
            {
                var bf = child.GetComponent<BattleField>();
                if (bf != null)
                    battleFields.Add(bf);
            }
        }
        else
        {
            Debug.LogError("BattleFields 오브젝트를 찾을 수 없습니다.");
        }
      //  battleFields[0].gameObject.SetActive(true);
      //  battleFields[0].FadeIn(1f); // 자연스럽게 등장
    }

    private void OnPartyFormationSaved(List<CombatCharacter> party)
    {
        RefreshCameraTargets();

        // 보완 조건: 스테이지 클리어 후에는 재시작 금지
        if (IslandEnd)
        {
            Debug.LogError($"BattleManager : isLandEnd - {isLandEnd} 111");
            return;
        }

        // 보완 조건: 패배 처리 중이면 전투 재시작 방지
        if (isHandlingDefeat)
        {
            Debug.LogWarning("[BattleManager] 패배 처리 중... 전투 재시작 차단됨");
            return;
        }

        // 전투 중이면 멈추고 다시 시작
        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
            JHT_MonsterSpawnManager.Instance.MonsterAllClear();
            ClearEnemies();
            ClearPlayers();
        }

        // 현재 라운드 기준으로 전투 재시작
        StartBattle(currentRoundIndex);
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

    // 스킵 버튼 클릭 시 호출
    private void Skip()
    {
        if (isbattleover) return;

        isbattleover = true;

        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
        }

        IslandStageManager.Instance.OnBattleComplete();
        JHT_MonsterSpawnManager.Instance.MonsterAllClear();
        ClearEnemies();
        ClearPlayers();
    }

    // 외부에서 전투 시작 요청
    public void StartBattle(int roundIndex)
    {
        isbattleover = false;
        currentRoundIndex = roundIndex;

        if (battleFields.Count == 0)
        {
            InitBattleFields(); // 재시도
            if (battleFields.Count == 0)
            {
                Debug.LogError("전투 필드가 없습니다. BattleManager가 초기화되지 않았습니다.");
                return;
            }
        }
        
        OnBattleStart?.Invoke();

        if (!MonsterDataManager.Instance.isTableLoadedFinish)
            return;

        if (GlobalStageManager.Instance.CurrentStageIndex == null)
        {
            // 딕셔너리가 세팅안되는 상황
            JHT_MonsterSpawnManager.Instance.ChangeIsland(battleFields[0], 0);
        }
        else
        {
            JHT_MonsterSpawnManager.Instance.ChangeIsland(battleFields[(GlobalStageManager.Instance.CurrentStageIndex.Value-1)],
                (GlobalStageManager.Instance.CurrentStageIndex.Value-1));
        }
        

        IslandEnd = false;
        Debug.LogError($"BattleManager : isLandEnd - {isLandEnd} 222");

        battleRoutine = StartCoroutine(BattleRoutine());
       // Debug.Log("battleRoutine 시작됨");

        if (_skipBtn != null)
            _skipBtn.interactable = true;

        // 카메라 전투 ON
        cameraFollow?.SetBattleActive(true);
    }

    //private IEnumerator StageStartCor()
    //{
    //    while (JHT_MonsterSpawnManager.Instance.stageDic.Count <= 0)
    //        yield return null;
    //
    //    
    //}

    // 전투 흐름 코루틴
    private IEnumerator BattleRoutine()
    {
        while(JHT_MonsterSpawnManager.Instance.roundTable == null)
            yield return null;

        // 수정
        int stageIdx = GlobalStageManager.Instance.CurrentStageIndex.Value - 1;
        if (stageIdx < 0 || stageIdx >= battleFields.Count)
        {
            Debug.LogError($"[BattleRoutine] stageIdx {stageIdx} 가 battleFields 범위를 벗어남 (count={battleFields.Count})");
            yield break;
        }
        var field = battleFields[stageIdx];

        SpawnEnemies(currentRoundIndex);
        SpawnPlayers(field);
        

        yield return new WaitForSeconds(0.5f);      // 등장 연출 대기

        // 몬스터 수가 0이 될떄까지 반복
        //yield return new WaitUntil(() => goNextIsland);

        yield return new WaitUntil(() => goNextIsland);
        
        Debug.LogError("BattleManager 대기끝");
        yield return new WaitForSeconds(1f);

        battleRoutine = null;

        if (isbattleover)
            yield break;

        if (_skipBtn != null)
            _skipBtn.interactable = false;


        JHT_MonsterSpawnManager.Instance.MonsterAllClear();
        ClearEnemies();
        ClearPlayers();
        currentRoundIndex = 0; // 이게 필요 없을수도 있음 -> 다음 island를 위해 설정하는부분
        cameraFollow?.StopFollowing(); // 전투 종료 시 추적 해제
        cameraFollow?.SetBattleActive(false);   // 전투 OFF
        IslandStageManager.Instance.OnBattleComplete();
    }

    // 플레이어 생성 및 카메라 타겟 설정
    private void SpawnPlayers(BattleField field)
    {
        ClearPlayers();
        var party = PartyManager.Instance.GetAllPartyMembers(); // 전체 반환

        if (allHealthBarsPanel != null)
        {
            allHealthBarsPanel.gameObject.SetActive(true);
        }  // 전체 반환

        for (int i = 0; i < party.Count; i++)
        {
            var character = party[i];
            character.Initialize(character.CharacterStats);
            character.transform.SetParent(null);
            character.transform.position = field.PlayerSpawnPoint.position + new Vector3(i * 0.3f, 0, 0);
            character.gameObject.SetActive(true);

            var fsm = character.GetComponent<BaseCharacterFSM>();
            if (fsm != null)
            {
                fsm.enabled = true;
                fsm.ChangeStateIdleForce();
            }
            currentPlayers.Add(character.gameObject);
        }

        var playerTransforms = currentPlayers.Select(p => p.transform).ToList();
        cameraFollow?.StartFollowing(playerTransforms);
    }

    // 적 생성 (스테이지 수에 비례해 증가)
    private void SpawnEnemies(int roundIndex)
    {
        if (roundIndex >= JHT_MonsterSpawnManager.Instance.roundTable.roundCount)
        {
            //GlobalStageManager.Instance.currentStageIndex++; // todo 임시 : 보스 나오기전
            currentRoundIndex = 0;
            IslandEnd = true;
            Debug.LogError($"BattleManager : isLandEnd - {isLandEnd} 333");
            return;
        }

        if (enemySpawnDelay == null)
            enemySpawnDelay = StartCoroutine(SpawnDelay(roundIndex));
    }

    IEnumerator SpawnDelay(int roundIndex)
    {
        yield return null;
        
        JHT_MonsterSpawnManager.Instance.ChangeRound(roundIndex);

        while (JHT_MonsterSpawnManager.Instance.curMonsterCountList.Count <= 0)
        {
            yield return null;
        }
        // 몬스터 데이터 넣기
        for (int i = 0; i < JHT_MonsterSpawnManager.Instance.curMonsterCountList.Count; i++)
        {
            spawnedEnemies.Add(JHT_MonsterSpawnManager.Instance.curMonsterCountList[i]);
        }


        if (enemySpawnDelay != null)
        {
            StopCoroutine(enemySpawnDelay);
            enemySpawnDelay = null;
        }
    }

    // 적 사망 시 호출
    public void OnEnemyDead(JHT_BaseMonsterStat enemy) //GameObject -> JHT_MonsterDataSO
    {
        if (spawnedEnemies.Contains(enemy))
            spawnedEnemies.Remove(enemy);

        QuestSignalManager.Instance.KillEnemy(MonsterId.All, 1);

        if (AllEnemiesDefeated())
        {
            currentRoundIndex++;
            SpawnEnemies(currentRoundIndex);
        }
    }

    // 적 전멸 여부 확인
    private bool AllEnemiesDefeated()
    {
        return spawnedEnemies.Count == 0;
    }

    private void SetBattleStop(bool value)
    {
        goNextIsland = value;
    }

    // 플레이어 사망 시 처리
    public void OnPlayerDead(GameObject deadPlayer)
    {
        if (currentPlayers.Contains(deadPlayer))
            currentPlayers.Remove(deadPlayer);

        Debug.Log($"{deadPlayer.name} 사망 → 남은 플레이어 수: {currentPlayers.Count}");

        if (currentPlayers.Count == 0)
        {
            Debug.Log("모든 플레이어 사망 → 패배 처리 시작");

            isbattleover = true;

            if (battleRoutine != null)
            {
                StopCoroutine(battleRoutine);
                battleRoutine = null;
            }

            if (_skipBtn != null)
                _skipBtn.interactable = false;

            StartCoroutine(HandleDefeat());
        }
    }

    // 패배 시 연출 및 리셋 처리
    private IEnumerator HandleDefeat()
    {
        isHandlingDefeat = true;

        JHT_MonsterSpawnManager.Instance.MonsterAllClear();
        ClearEnemies();
        ClearPlayers();

        // 패배 시 카메라 전투 OFF
        cameraFollow?.SetBattleActive(false);
        cameraFollow?.StopFollowing();
        

        ScreenScrollEffectManager.Instance.ShowScrollEffect("패배했습니다. \n 첫번째 섬부터 재도전합니다.", () => { });
        currentRoundIndex = 0;
        yield return new WaitForSeconds(1f);

        IslandStageManager.Instance.ResetStageAfterDefeat();

        isHandlingDefeat = false;
    }

    // 적 제거
    public void ClearEnemies()
    {
        /*foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }*/
        spawnedEnemies.Clear();

        Debug.Log("몬스터 제거됨");
    }

    // 플레이어 제거
    public void ClearPlayers()
    {
        foreach (var player in currentPlayers)
        {
            if (player != null)
            {
                player.SetActive(false);
                //player.transform.SetParent(PartyManager.Instance.transform); // 다시 파티매니저로 귀환
            }
        }
        currentPlayers.Clear();

        Debug.Log("플레이어 제거됨");
    }

}
