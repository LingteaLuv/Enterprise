using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// UI(캔버스) 전용 '항해 연출' 컨트롤러.
/// - shipIcon: 배 아이콘(RectTransform)
/// - islandMarkers: UI상 섬(노드) 마커들(RectTransform)
/// - 배는 anchoredPosition을 보간해 이동한다.
/// - 필요 시 페이드/텍스트/사운드 등 UI 연출을 이곳에 추가.
/// </summary>
public class TravelUIController : MonoBehaviour
{
    [Header("UI 항해 구성")]
    [SerializeField] private RectTransform shipIcon;          // 배 아이콘(UI)
    [SerializeField] private List<RectTransform> islandMarkers; // 각 섬의 UI 마커(경로상 노드)

    [Header("연출 파라미터")]
    [SerializeField] private float moveDuration = 1.2f;       // 노드 간 이동 시간
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private CanvasGroup travelCanvasGroup;    // 항해용 캔버스 그룹(표시/숨김)
    [SerializeField] private TMP_Text stageLabel;                  // (선택) "Island 1" 같은 라벨

    /// <summary>
    /// UI를 보여주거나 숨김. (항해 중에만 보이게 하려면 사용)
    /// </summary>
    public void SetVisible(bool visible, float fadeTime = 0.2f)
    {
        if (travelCanvasGroup == null)
        {
            gameObject.SetActive(visible);
            return;
        }
        StartCoroutine(FadeCanvasGroup(travelCanvasGroup, visible ? 1f : 0f, fadeTime));
    }

    /// <summary>
    /// shipIcon을 index번째 마커로 이동 연출. 끝나면 onArrive 콜백.
    /// </summary>
    public IEnumerator MoveShipToMarker(int index, Action onArrive)
    {
        Debug.Log($"[TravelUI] {index}번 섬으로 이동 시작!");
        Debug.Log($"[TravelUI] gameObject.activeInHierarchy: {gameObject.activeInHierarchy}, enabled: {enabled}");
        // 안전 체크
        if (index < 0 || index >= islandMarkers.Count)
        {
            Debug.LogWarning($"[TravelUI] invalid index {index}");
            onArrive?.Invoke();
            yield break;
        }

        // 항해 UI 켜기(필요 시)
        SetVisible(true);

        // (선택) 라벨 갱신
        if (stageLabel != null)
            stageLabel.text = $"Island {index + 1}";

        // Lerp by anchoredPosition
        Vector2 start = shipIcon.anchoredPosition;
        Vector2 end = islandMarkers[index].anchoredPosition;

        float t = 0f;
        while (t < moveDuration)
        {
            float nt = ease.Evaluate(t / moveDuration);
            shipIcon.anchoredPosition = Vector2.LerpUnclamped(start, end, nt);
            t += Time.unscaledDeltaTime; // UI는 보통 unscaled 권장(연출이 TimeScale에 영향 안받게)
            yield return null;
        }
        shipIcon.anchoredPosition = end;

        // 항해 UI 끄기(원하면 남겨도 됨)
        SetVisible(false);

        Debug.Log(" 콜백 직전");
        onArrive?.Invoke();
        Debug.Log("콜백 직후");
        Debug.Log($"[TravelUI] {index}번 섬 도착 콜백 호출됨");
    }

    /// <summary>
    /// CanvasGroup의 투명도를 지정한 시간 동안 점진적으로 변경(페이드)합니다.
    /// - Time.unscaledDeltaTime을 사용하여 Time.timeScale의 영향을 받지 않도록 합니다(일시정지/슬로모션 중에도 일정한 UI 연출).
    /// - 페이드 완료 후, target 값이 1에 가까우면(> 0.99f) 입력을 받을 수 있게(interactable/blocksRaycasts) 설정합니다.
    /// </summary>
    /// <param name="cg">페이드 대상이 되는 CanvasGroup</param>
    /// <param name="target">목표 알파(0=완전 투명, 1=완전 불투명)</param>
    /// <param name="time">페이드에 걸리는 시간(초)</param>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float target, float time)
    {
        // 현재 알파 값을 시작점으로 기억
        float start = cg.alpha;

        // 경과 시간 누적용 변수
        float t = 0f;

        // 지정한 기간(time) 동안 선형 보간으로 알파를 변경
        while (t < time)
        {
            // 0~1 사이 진행도에 따라 start→target으로 알파를 보간
            cg.alpha = Mathf.Lerp(start, target, t / time);

            // TimeScale의 영향을 받지 않도록 unscaledDeltaTime 사용
            t += Time.unscaledDeltaTime;

            // 다음 프레임까지 대기
            yield return null;
        }

        // 루프 종료 시 보정: 목표 알파를 정확히 세팅
        cg.alpha = target;

        // 알파가 거의 1이라면(완전 보임) 상호작용 가능하게 전환
        // - interactable: UI 요소가 사용자 입력을 받을 수 있는지 여부
        // - blocksRaycasts: 이 CanvasGroup이 레이캐스트를 가로채는지 여부(클릭/터치 차단 여부)
        cg.interactable = target > 0.99f;
        cg.blocksRaycasts = cg.interactable;
    }
}
