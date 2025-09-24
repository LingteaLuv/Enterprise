using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

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
    public Image ropeImage;
    public RectTransform container;

    [Header("애니메이션 설정")]
    public float startDelay = 2f;
    public float zoomDuration = 1f;
    public float zoomScale = 1.2f;
    public float ropeThrowDuration = 0.5f;
    public float endDelay = 1f; // 연출이 모두 끝난 후, 사라지기 전까지의 대기 시간

    private Sequence mainSequence;
    private Dictionary<RectTransform, Vector2> _initialPositions;
    private bool _isInitialized = false;

    void Awake()
    {
        InitializePositions();
    }

    /// <summary>
    /// 모든 연출 요소의 초기 위치를 저장합니다。
    /// </summary>
    private void InitializePositions()
    {
        if (_isInitialized) return;

        _initialPositions = new Dictionary<RectTransform, Vector2>();
        StoreInitialPosition(sky);
        StoreInitialPosition(waves);
        StoreInitialPosition(sunLight);
        StoreInitialPosition(ship);

        if (container != null) _initialPositions[container] = container.anchoredPosition;
        if (ropeImage != null) _initialPositions[ropeImage.rectTransform] = ropeImage.rectTransform.anchoredPosition;

        _isInitialized = true;
    }

    private void StoreInitialPosition(GachaAnimElement element)
    {
        if (element != null && element.image != null)
        {
            _initialPositions[element.image.rectTransform] = element.image.rectTransform.anchoredPosition;
        }
    }

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

        // 마지막에 여운을 남기기 위한 딜레이 추가
        mainSequence.AppendInterval(endDelay);

        mainSequence.OnComplete(() =>
        {
            Debug.Log("가챠 연출 완료!");
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 연출 시작 전, 모든 요소를 초기 상태 및 위치로 리셋합니다。
    /// </summary>
    private void SetupInitialState()
    {
        if (!_isInitialized) InitializePositions();

        // 모든 트윈 중지
        if (mainSequence != null && mainSequence.IsActive()) mainSequence.Kill();
        foreach (var pair in _initialPositions)
        {
            if (pair.Key != null) pair.Key.DOKill();
        }

        // 모든 요소의 위치를 저장된 초기 위치로 리셋
        foreach (var pair in _initialPositions)
        {
            if (pair.Key != null) pair.Key.anchoredPosition = pair.Value;
        }

        // 기타 상태 리셋
        container.localScale = Vector3.one;
        ropeImage.gameObject.SetActive(false);
        ropeImage.transform.localScale = Vector3.zero;
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
        if (!_initialPositions.TryGetValue(element.image.rectTransform, out Vector2 startPos)) return;

        Vector2 targetPos = startPos + new Vector2(element.moveDistanceX, element.moveDistanceY);

        // DOTween을 사용하여 왕복 애니메이션을 설정합니다.
        element.image.rectTransform.DOAnchorPos(targetPos, element.moveDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    [ContextMenu("Test Play Animation")]
    private void TestPlay()
    {
        Play(() => Debug.Log("테스트 연출 콜백 호출됨"));
    }
}
