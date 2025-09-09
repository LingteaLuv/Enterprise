using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 섬 이동, 배 이동, 전투 흐름을 통합 관리하는 매니저
/// - 각 섬은 islandSets에 GameObject로 등록
/// - 각 섬 앞의 배 도착 위치는 shipPositions에 Transform으로 지정
/// - 전투가 끝나면 자동으로 다음 섬으로 이동
/// </summary>
public class IslandStageManager : MonoBehaviour
{
    [Header("배 이동 설정")]
    [SerializeField] private ShipController shipController;
    [SerializeField] private Transform shipTransform;
    [SerializeField] private float shipMoveDuration = 2f;

    [SerializeField] private TravelUIController travelUI;               // UI상에서 배 위치 이동 처리
    [SerializeField] private GameObject chestPrefab;                    // (예정) 보물상자 프리팹
    [SerializeField] private GameObject flagPrefab;                     // 섬 클리어 시 표시할 깃발 프리팹

    private List<GameObject> islandSets = new();                        // 하이어라키에 있는 IslandMaker 세트
    private List<Transform> shipPositions = new();                      // 각 섬 앞에 배가 도달 할 위치
    private List<GameObject> battleFields = new();                      // 각 섬의 전투 필드
    private List<IslandMaker> islandMakers = new();                     // IslandMaker 스크립트가 붙은 섬

    private int currentIndex = 0;                                       // 현재 진행 중인 섬 인덱스

    private Coroutine handleReturnAndNext;                              // 섬 이동 처리 코루틴
    private Coroutine moveToAndEnter;                                   // 배 도착 이후 전투 시작 처리 코루틴

    public static IslandStageManager Instance { get; private set; }     // 싱글 톤 인스턴스

    private void Awake()
    {
        Instance = this;
    }


    private void OnEnable()
    {
        // 씬 로드 완료 시점에 초기화 메서드 연결
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드되면 자동으로 섬 데이터 초기화 및 스테이지 시작
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "0909Demo")
        {
            Debug.Log("IslandStageManager: Scene Loaded → InitReferences + StartStage");
            InitReferences();
            StartCoroutine(WaitUntilTravelUIReady());
        }
    }


    private IEnumerator WaitUntilTravelUIReady()
    {
        float timeout = 2f;
        float timer = 0f;


        while (travelUI == null || !travelUI.IsReady)
        {
            timer += Time.deltaTime;
            if (timer >= timeout)
            {
                Debug.LogWarning("TravelUIController 초기화 시간 초과");
                yield break;
            }
            yield return null;
        }


        StartStage();
    }

    /// <summary>
    /// 하이어라키에서 오브젝트를 찾아 런타임에 참조 연결
    /// </summary>
    private void InitReferences()
    {
        // Island Sets
        islandSets.Clear();
        Transform islandRoot = GameObject.Find("MapArea")?.transform;
        if (islandRoot != null)
        {
            foreach (Transform child in islandRoot)
            {
                if (child.name.StartsWith("IslandMaker"))
                    islandSets.Add(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("[InitReferences] MapArea 오브젝트를 찾을 수 없습니다.");
        }

        // Battle Fields
        battleFields.Clear();
        Transform fieldRoot = GameObject.Find("BattleFields")?.transform;
        if (fieldRoot != null)
        {
            foreach (Transform child in fieldRoot)
                battleFields.Add(child.gameObject);
        }
        else
        {
            Debug.LogError("[InitReferences] BattleFields 오브젝트를 찾을 수 없습니다.");
        }

        // Island Makers
        islandMakers.Clear();
        foreach (var island in islandSets)
        {
            var maker = island.GetComponent<IslandMaker>();
            if (maker != null)
                islandMakers.Add(maker);
        }

        // Ship Positions (각 IslandMaker 하위에 ShipPosition)
        shipPositions.Clear();
        foreach (var maker in islandMakers)
        {
            var pos = maker.transform.Find("ShipPosition");
            if (pos != null)
                shipPositions.Add(pos);
            else
                Debug.LogWarning($"[InitReferences] ShipPosition 누락: {maker.name}");
        }
    }

    /// <summary>
    /// 스테이지의 첫 섬부터 시작
    /// </summary>
    public void StartStage()
    {
        Debug.Log("시작");
        currentIndex = 0;
        moveToAndEnter = StartCoroutine(MoveToAndEnter(currentIndex));
    }

    /// <summary>
    /// 전투가 끝나면 호출됨 → 다음 섬으로 이동
    /// </summary>
    public void OnBattleComplete()
    {
        Debug.Log($"[OnBattleComplete 호출됨] currentIndex: {currentIndex}");

        if (!GlobalStageManager.Instance.bossBattleTriggered)
        {
            SpawnClearMarker(currentIndex);
        }
        else
        {
            Debug.Log($" 깃발 생략됨 (보스전 패배 이후 루프): index={currentIndex}");
        }

        QuestSignalManager.Instance.ETCAchieve("IslandClear");

        // 디버깅용 상태 확인
        if (!this.enabled)
            Debug.LogError(" IslandStageManager.enabled == false");
        if (!this.gameObject.activeInHierarchy)
            Debug.LogError(" IslandStageManager GameObject가 비활성화됨");

        handleReturnAndNext = StartCoroutine(HandleReturnAndNext());
    }

    /// <summary>
    /// 깃발 생성 후 다음 섬 이동까지의 흐름 제어
    /// </summary>
    private IEnumerator HandleReturnAndNext()
    {
        DeactivateAllBattleFields();

        bool done = false;
        ScreenScrollEffectManager.Instance.ShowScrollEffect("전투 완료! 다음 섬으로 이동합니다...", () =>
        {
            done = true;
        });
        yield return new WaitUntil(() => done);

        Debug.Log(" [HandleReturnAndNext] 진입");

        yield return new WaitForSeconds(1f);

        currentIndex++;
        Debug.Log($" [HandleReturnAndNext] currentIndex 증가됨: {currentIndex}");

        // 모든 섬을 클리어했다면 → 보스전으로 이동
        if (currentIndex >= islandSets.Count)
        {
            Debug.Log(" [HandleReturnAndNext] 모든 섬 완료");

            if (!GlobalStageManager.Instance.bossBattleTriggered)
            {
                Debug.Log("→ 보스전 자동 진입 조건 만족 (처음 클리어)");
                GlobalStageManager.Instance.bossBattleTriggered = true;

                SceneManager.LoadScene("BossBattleScene");
                yield break;
            }
            else
            {
                Debug.Log("→ 이미 보스전 진입한 상태, 일반 반복 전투 재시작");
                ResetClearMarkers();
                StartStage();
                yield break;
            }
        }

        // 다음 섬으로 이동
        Debug.Log($" [HandleReturnAndNext] MoveToAndEnter({currentIndex}) 호출 시도");
        moveToAndEnter = StartCoroutine(MoveToAndEnter(currentIndex));
        Debug.Log(" [HandleReturnAndNext] MoveToAndEnter 종료");
    }

    /// <summary>
    /// 지정된 섬으로 배를 이동시키고, 전투를 시작함
    /// </summary>
    private IEnumerator MoveToAndEnter(int index)
    {
        Debug.Log($"[MoveToAndEnter] index={index} 시작");

        bool arrive = false;
        yield return travelUI.MoveShipToMarker(GlobalStageManager.Instance.currentStageIndex, index, () =>
        {
            arrive = true;
            Debug.Log($"[MoveToAndEnter] onArrive 수신 index={index}");
        });

        // 도착 후 스크롤 연출
        bool done = false;
        ScreenScrollEffectManager.Instance.ShowScrollEffect("섬에 도착했습니다!", () =>
        {
            done = true;
        });
        yield return new WaitUntil(() => done);

        SetBattleField(index);

        BattleManager.Instance?.StartBattle(currentIndex);
        Debug.Log($"[MoveToAndEnter] StartBattle 호출 이후 정상 종료 index={index}");

        StopCoroutine(moveToAndEnter);
    }

    /// <summary>
    /// 보스전 패배 등으로 인해 스테이지를 다시 시작할 때 사용
    /// </summary>
    public void ResetStageAfterDefeat()
    {
        Debug.Log("[ResetStageAfterDefeat] 패배로 인해 스테이지 재시작");

        foreach (var island in islandSets)
            island.SetActive(false);
        DeactivateAllBattleFields();
        ResetClearMarkers();
        StartStage();
    }

    /// <summary>
    /// 현재 인덱스에 해당하는 전투 필드만 활성화
    /// </summary>
    private void SetBattleField(int index)
    {
        Debug.Log($"[SetBattleField] index={index}");

        for (int i = 0; i < battleFields.Count; i++)
        {
            Debug.Log($"- battleField[{i}]: {battleFields[i].name} → {(i == index ? "활성화" : "비활성화")}");
            battleFields[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// 모든 전투 필드 비활성화
    /// </summary>
    private void DeactivateAllBattleFields()
    {
        foreach (var field in battleFields)
            field.SetActive(false);
    }

    /// <summary>
    /// 깃발 위치에 클리어 마커(깃발) 생성
    /// </summary>
    private void SpawnClearMarker(int islandIndex)
    {
        if (islandIndex < 0 || islandIndex >= islandMakers.Count)
        {
            Debug.LogWarning("유효하지 않은 인덱스의 섬입니다.");
            return;
        }

        var island = islandMakers[islandIndex];
        var anchor = island.transform.Find("FlagPosition");

        if (anchor != null && flagPrefab != null)
        {
            Instantiate(flagPrefab, anchor.position, Quaternion.identity, anchor);
            Debug.Log($" 깃발 생성 완료: 섬 {islandIndex}");
        }
    }

    /// <summary>
    /// 섬들의 FlagPosition에 붙은 깃발 제거 (루프 반복 시작 시 호출)
    /// </summary>
    private void ResetClearMarkers()
    {
        foreach (var island in islandMakers)
        {
            Transform anchor = island.transform.Find("FlagPosition");
            if (anchor == null) continue;

            for (int i = anchor.childCount - 1; i >= 0; i--)
                Destroy(anchor.GetChild(i).gameObject);

            Debug.Log($"플래그 제거 완료 - {island.name}");
        }
    }
}