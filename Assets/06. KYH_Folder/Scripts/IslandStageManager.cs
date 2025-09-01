using DG.Tweening.Core.Easing;
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
    [Header("섬 구성")]
    [SerializeField] private List<GameObject> islandSets;         // 각 섬에 해당하는 게임오브젝트 세트 (환경, 적, 타일맵 등 포함)
    [SerializeField] private List<Transform> shipPositions;       // 각 섬 앞에 배가 도달할 위치 (Transform으로 배치)
    [SerializeField] private List<GameObject> battleFields;

    [Header("배 이동 설정")]
    [SerializeField] private ShipController shipController;
    [SerializeField] private Transform shipTransform;             // 실제 배 GameObject (움직일 대상)
    [SerializeField] private float shipMoveDuration = 2f;         // 섬까지 배가 이동하는 시간

    
    [SerializeField] private TravelUIController travelUI;          // // 캔버스에서 배 이동을 연출하는 컨트롤러
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private List<IslandMaker> islandMakers;
    private int currentIndex = 0;                                  // 현재 진행 중인 섬의 인덱스 번호
                                      

    private Coroutine handleReturnAndNext;
    private Coroutine moveToAndEnter;
    public static IslandStageManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartStage();
    }
    /// <summary>
    /// 스테이지 시작: 첫 섬으로 배를 이동시키며 시작
    /// </summary>
    public void StartStage()
    {
        Debug.Log("시작");
        currentIndex = 0;
        moveToAndEnter = StartCoroutine(MoveToAndEnter(currentIndex));
    }

    /// <summary>
    /// 전투가 완료되었을 때 호출됨
    /// 다음 섬으로 이동하거나 스테이지 클리어 처리
    /// </summary>
    public void OnBattleComplete()
    {
        Debug.Log($"[OnBattleComplete 호출됨] currentIndex: {currentIndex}");

        // 보스전에 아직 진입하지 않은 상태에서만 깃발 생성
        if (GlobalStageManager.Instance.bossBattleTriggered == false)
        {
            SpawnClearMarker(currentIndex);
        }
        else
        {
            Debug.Log($" 깃발 생략됨 (보스전 패배 이후 루프): index={currentIndex}");
        }

        QuestSignalManager.Instance.ETCAchieve("IslandClear");

        if (!this.enabled)
            Debug.LogError(" IslandStageManager.enabled == false");
        if (!this.gameObject.activeInHierarchy)
            Debug.LogError(" IslandStageManager GameObject가 비활성화됨");

        handleReturnAndNext = StartCoroutine(HandleReturnAndNext());
    }

    /// <summary>
    /// 배가 복귀 후 다음 섬으로 이동하는 전체 흐름
    /// </summary>
    private IEnumerator HandleReturnAndNext()
    {
        DeactivateAllBattleFields();

        // 패널 연출 삽입
        bool done = false;

        ScreenScrollEffectManager.Instance.ShowScrollEffect("전투 완료! 다음 섬으로 이동합니다...", () => {
            done = true; 
        });
        yield return new WaitUntil(() => done);

        

        Debug.Log(" [HandleReturnAndNext] 진입");

        yield return new WaitForSeconds(1f);

        currentIndex++;
        Debug.Log($" [HandleReturnAndNext] currentIndex 증가됨: {currentIndex}");

        if (currentIndex >= islandSets.Count)
        {
            Debug.Log(" [HandleReturnAndNext] 모든 섬 완료");

            if (!GlobalStageManager.Instance.bossBattleTriggered)
            {
                Debug.Log("→ 보스전 자동 진입 조건 만족 (처음 클리어)");
                GlobalStageManager.Instance.bossBattleTriggered = true;

                // 보스 씬으로 이동
                SceneManager.LoadScene("BossBattleScene");
                yield break;
            }
            else
            {
                Debug.Log("→ 이미 보스전 진입한 상태, 일반 반복 전투 재시작");
                ResetClearMarkers();
                StartStage(); // 반복 전투 시작
                yield break;
            }
        }

        Debug.Log($"➡ [HandleReturnAndNext] MoveToAndEnter({currentIndex}) 호출 시도");
        moveToAndEnter = StartCoroutine(MoveToAndEnter(currentIndex));
        Debug.Log(" [HandleReturnAndNext] MoveToAndEnter 종료");
    }

    /// <summary>
    /// 배를 해당 섬 위치까지 이동시키고, 도착하면 섬 활성화 + 전투 시작
    /// </summary>
    private IEnumerator MoveShipToIsland(int index)
    {
        Vector3 start = shipTransform.position;
        Vector3 end = shipPositions[index].position;

        float t = 0f;
        while (t < shipMoveDuration)
        {
            shipTransform.position = Vector3.Lerp(start, end, t / shipMoveDuration);
            t += Time.deltaTime;
            yield return null;
        }

        shipTransform.position = end; // 정확히 위치 고정

        // 1. 섬 활성화: 현재 섬만 켜고 나머지는 끔
        for (int i = 0; i < islandSets.Count; i++)
            islandSets[i].SetActive(i == index);

        // 2. 전투 시작 (BattleManager는 기존 시스템을 그대로 사용)
        BattleManager.Instance.StartBattle(currentIndex);
    }


    /*
    // [확장용] ShipController를 따로 두고 싶을 때 사용하는 코드 예시
    // IslandStageManager는 흐름만 관리하고,
    // ShipController가 실제 배 이동만 담당하도록 분리 가능함

    [SerializeField] private ShipController shipController; // ShipController 참조 추가

    private IEnumerator MoveShipToIsland(int index)
    {
        // ShipController에게 이동 명령을 위임
        yield return shipController.MoveToPosition(shipPositions[index].position, shipMoveDuration, () =>
        {
            // 도착 후 섬 활성화
            for (int i = 0; i < islandSets.Count; i++)
                islandSets[i].SetActive(i == index);

            // 전투 시작
            BattleManager.Instance.StartBattle();
        });
    }
    */

    /// <summary>
    /// UI에서 배를 index번째 섬 마커로 이동시키고, 섬을 활성화한 뒤 전투 시작
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

        // 패널 연출 삽입
        bool done = false;

        ScreenScrollEffectManager.Instance.ShowScrollEffect("섬에 도착했습니다!", () => {
            done = true; 
        });
        yield return new WaitUntil(() => done);


        SetBattleField(index); // 전투 필드 활성화



        BattleManager.Instance?.StartBattle(currentIndex);

        Debug.Log($"[MoveToAndEnter] StartBattle 호출 이후 정상 종료 index={index}");

        StopCoroutine(moveToAndEnter);
    }

    public void ResetStageAfterDefeat()
    {
        Debug.Log("[ResetStageAfterDefeat] 패배로 인해 스테이지 재시작");

        // 모든 섬/전투 필드 초기화
        for (int i = 0; i < islandSets.Count; i++)
            islandSets[i].SetActive(false);
        DeactivateAllBattleFields();

        // 깃발/상자 제거
        ResetClearMarkers();

        // 다시 처음부터 시작
        StartStage();
    }

    private void SetBattleField(int index)
    {
        Debug.Log($"[SetBattleField] index={index}");

        for (int i = 0; i < battleFields.Count; i++)
        {
            Debug.Log($"- battleField[{i}]: {battleFields[i].name} → {(i == index ? "활성화" : "비활성화")}");
            battleFields[i].SetActive(i == index);
        }
    }

    private void DeactivateAllBattleFields()
    {
        for (int i = 0; i < battleFields.Count; i++)
            battleFields[i].SetActive(false);
    }

    private void SpawnClearMarker(int islandIndex)
    {
        if (islandIndex < 0 || islandIndex >= islandMakers.Count)
        {
            Debug.LogWarning("유효하지 않은 인덱스의 섬입니다.");
            return;
        }

        var island = islandMakers[islandIndex];
        var anchor = island.transform.Find("FlagPosition"); // 또는 ChestPosition
        
        if (anchor != null && flagPrefab != null)
        {
            Instantiate(flagPrefab, anchor.position, Quaternion.identity, anchor);
            Debug.Log($" 깃발, 상자 생성 완료: 섬 {islandIndex}");
        }
    }

    private void ResetClearMarkers()
    {
        foreach (var island in islandMakers)
        {
            Transform anchor = island.transform.Find("FlagPosition");
            if (anchor == null) continue;

            for (int i = anchor.childCount - 1; i >= 0; i--)
            {
                Destroy(anchor.GetChild(i).gameObject);
            }

            Debug.Log($"플래그 제거 완료 - {island.name}");
        }
    }
}