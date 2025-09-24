using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// 캐릭터 가챠 연출의 각 배경 요소를 설정하기 위한 클래스입니다.
/// </summary>
[System.Serializable]
public class GachaAnimElement
{
    public Image image;
    [Tooltip("좌우로 움직일 거리")]
    public float moveDistanceX = 50f;
    [Tooltip("위아래로 움직일 거리")]
    public float moveDistanceY = 0f;
    [Tooltip("움직임이 1회 완료되는 데 걸리는 시간")]
    public float moveDuration = 5f;
}

/// <summary>
/// 캐릭터 가챠 연출을 관리하고 재생하는 클래스입니다.
/// </summary>
public class CharacterGachaDirector : MonoBehaviour
{
    [Header("연출 배경 요소")]
    public GachaAnimElement sky;
    public GachaAnimElement waves;
    public GachaAnimElement sunLight;
    public GachaAnimElement ship;

    [Header("연출 핵심 요소")]
    [Tooltip("임시 로프 이미지. 나중에 애니메이션으로 교체될 수 있습니다.")]
    public Image ropeImage;
    [Tooltip("전체 연출을 담고 있는 부모 오브젝트. 줌 효과에 사용됩니다.")]
    public RectTransform container;

    [Header("애니메이션 설정")]
    public float startDelay = 2f;
    public float zoomDuration = 1f;
    public float zoomScale = 1.2f;
    public float ropeThrowDuration = 0.5f;

    private Sequence mainSequence;

    /// <summary>
    /// 가챠 연출을 시작합니다.
    /// </summary>
    /// <param name="onComplete">연출이 모두 끝났을 때 호출될 콜백 함수</param>
    public void Play(Action onComplete = null)
    {
        SetupInitialState();
        mainSequence = DOTween.Sequence();
        StartBackgroundAnimation();

        mainSequence.AppendInterval(startDelay);
        mainSequence.Append(container.DOScale(zoomScale, zoomDuration).SetEase(Ease.OutQuad));
        mainSequence.Append(
            ropeImage.transform.DOScale(1f, ropeThrowDuration)
                .SetEase(Ease.OutBack)
                .OnStart(() => ropeImage.gameObject.SetActive(true))
        );
        mainSequence.OnComplete(() => {
            Debug.Log("가챠 연출 완료!");
            onComplete?.Invoke();
        });
    }

    private void SetupInitialState()
    {
        // 모든 관련 오브젝트의 트윈을 중지합니다.
        KillTween(ship);

        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Kill();
        }

        container.localScale = Vector3.one;
        ropeImage.gameObject.SetActive(false);
        ropeImage.transform.localScale = Vector3.zero;
    }

    private void KillTween(GachaAnimElement element)
    {
        if (element != null && element.image != null) element.image.transform.DOKill();
    }

    private void StartBackgroundAnimation()
    {
        // 각 배경 요소에 개별적인 애니메이션을 적용합니다.
        ApplyElementAnimation(sky);
        ApplyElementAnimation(waves);
        ApplyElementAnimation(sunLight);
        ApplyElementAnimation(ship);
    }

    /// <summary>
    /// 단일 배경 요소에 반복적인 움직임 애니메이션을 적용합니다.
    /// </summary>
    private void ApplyElementAnimation(GachaAnimElement element)
    {
        if (element == null || element.image == null || element.moveDuration <= 0) return;

        // 시작 위치를 기록해 둡니다.
        Vector2 startPos = element.image.rectTransform.anchoredPosition;
        // 최종 목표 위치를 계산합니다.
        Vector2 targetPos = startPos + new Vector2(element.moveDistanceX, element.moveDistanceY);

        // DOTween을 사용하여 왕복 애니메이션을 설정합니다.
        element.image.rectTransform.DOAnchorPos(targetPos, element.moveDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(element.image.gameObject); // Kill을 위해 ID 부여
    }

    // 에디터 테스트용
    [ContextMenu("Test Play Animation")]
    private void TestPlay()
    {
        Play(() => Debug.Log("테스트 연출 콜백 호출됨"));
    }
}
