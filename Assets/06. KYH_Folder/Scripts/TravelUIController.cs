using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// UI 상에서 배 아이콘을 섬 위치에 맞춰 이동시키는 연출을 담당하는 컨트롤러
/// - 섬 Marker들과 ShipIcon을 RectTransform 기준으로 UI 이동
/// - Scene 로드 시 InitReferences로 요소들을 런타임에서 연결
/// </summary>
public class TravelUIController : MonoBehaviour
{
    // 🚢 배 아이콘 UI (RectTransform 기준으로 이동함)
    private RectTransform shipIcon;

    // 🏝 섬 Marker들의 위치 (MapArea 하위에 있는 RectTransform들)
    private List<RectTransform> islandMarkers = new();

    // 전체 배 UI를 컨트롤하는 CanvasGroup (보이기/숨기기 Fade 연출용)
    private CanvasGroup travelCanvasGroup;

    // 스테이지 번호를 표시하는 텍스트
    private TMP_Text stageLabel;

    [Header("연출 파라미터")]
    [SerializeField] private float moveDuration = 1.2f;                                             // 배가 이동하는 총 시간
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);            // 배 이동 커브

    // 초기화 여부 체크
    private bool isInitialized = false;

    // 외부에서 사용할 수 있는 초기화 완료 상태 플래그
    public bool IsReady => isInitialized;

    private void OnEnable()
    {
        // 씬 로드 이벤트에 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 완료 시 참조 초기화 수행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && !isInitialized)
        {
            Debug.Log("🚢 TravelUIController: Scene Loaded → InitReferences");
            isInitialized = true;
            InitReferences();
        }
    }

    /// <summary>
    /// 런타임에 필요한 오브젝트 참조들을 연결
    /// </summary>
    private void InitReferences()
    {
        // 배 아이콘 찾기
        shipIcon = GameObject.Find("ShipIcon")?.GetComponent<RectTransform>();
        if (shipIcon == null)
            Debug.LogError(" ShipIcon을 찾을 수 없습니다.");

        // MapArea 하위의 섬 마커들 찾기
        islandMarkers.Clear();
        Transform mapArea = GameObject.Find("MapArea")?.transform;
        if (mapArea != null)
        {
            foreach (Transform child in mapArea)
            {
                var marker = child.GetComponent<RectTransform>();
                if (marker != null)
                    islandMarkers.Add(marker);
            }
        }
        else
        {
            Debug.LogError(" MapArea 오브젝트를 찾을 수 없습니다.");
        }

        // TravelCanvasGroup 찾기
        travelCanvasGroup = GameObject.Find("TravelCanvasGroup")?.GetComponent<CanvasGroup>();
        if (travelCanvasGroup == null)
            Debug.LogWarning(" TravelCanvasGroup을 찾을 수 없습니다.");

        // StageNumber 텍스트 찾기
        stageLabel = GameObject.Find("StageNumber")?.GetComponent<TMP_Text>();
        if (stageLabel == null)
            Debug.LogWarning(" StageNumber을 찾을 수 없습니다.");
    }

    /// <summary>
    /// Travel UI 전체를 보이거나 숨긴다 (CanvasGroup 페이드)
    /// </summary>
    public void SetVisible(bool visible, float fadeTime = 0.2f)
    {
        if (travelCanvasGroup == null)
        {
            // CanvasGroup이 없다면 활성화만 간단히 처리
            gameObject.SetActive(visible);
            return;
        }
        // 페이드 연출
        StartCoroutine(FadeCanvasGroup(travelCanvasGroup, visible ? 1f : 0f, fadeTime));
    }

    /// <summary>
    /// 배 아이콘을 islandMarkers[index] 위치로 이동시키는 코루틴
    /// </summary>
    /// <param name="stageIndex">현재 스테이지 번호 (라벨용)</param>
    /// <param name="index">도달할 섬의 인덱스</param>
    /// <param name="onArrive">이동 완료 후 호출될 콜백</param>
    public IEnumerator MoveShipToMarker(int stageIndex, int index, Action onArrive)
    {
        Debug.Log($"[TravelUI] {index}번 섬으로 이동 시작!");

        // 인덱스 유효성 확인
        if (index < 0 || index >= islandMarkers.Count)
        {
            Debug.LogWarning($"[TravelUI] invalid index {index}");
            onArrive?.Invoke();
            yield break;
        }

        // UI 켜기
        SetVisible(true);

        // 스테이지 라벨 갱신
        if (stageLabel != null)
            stageLabel.text = $"STAGE {stageIndex} - ISLAND {index + 1}";

        // 배 위치 이동 연출
        Vector2 start = shipIcon.anchoredPosition;
        Vector2 end = islandMarkers[index].anchoredPosition;

        float t = 0f;
        while (t < moveDuration)
        {
            float nt = ease.Evaluate(t / moveDuration);
            shipIcon.anchoredPosition = Vector2.LerpUnclamped(start, end, nt);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 위치 고정
        shipIcon.anchoredPosition = end;

        // UI 끄기
        SetVisible(false);

        Debug.Log("콜백 직전");
        onArrive?.Invoke();
        Debug.Log("콜백 직후");
    }


    /// <summary>
    /// CanvasGroup을 부드럽게 페이드 인/아웃시키는 유틸리티
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float target, float time)
    {
        float start = cg.alpha;
        float t = 0f;

        while (t < time)
        {
            cg.alpha = Mathf.Lerp(start, target, t / time);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cg.alpha = target;

        // 페이드 후 인터랙션 여부 설정
        cg.interactable = target > 0.99f;
        cg.blocksRaycasts = cg.interactable;
    }
}
