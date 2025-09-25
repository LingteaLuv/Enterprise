using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public enum UnlockableFeature
{
    None,
    TenGacha,
    EquipGacha,
    RelicGacha,
}

[System.Serializable]
public struct FeatureUIData
{
    public UnlockableFeature feature;
    public string featureName;
    public Sprite featureIcon;
}

/// <summary>
/// [풀링 버전] 기능 해금 UI 효과를 담당합니다.
/// </summary>
public class UIUnlockEffect : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public RectTransform panelRect;
    public CanvasGroup lockCanvasGroup;
    public RectTransform lockShackleRect;
    public TextMeshProUGUI featureNameText;
    public Image featureIconImage;

    [Header("해금 기능 데이터 목록")]
    public List<FeatureUIData> featureDataList;

    [Header("에디터 테스트 설정")]
    [Tooltip("true로 설정하면, 프리팹을 켰을 때 아래 기능으로 자동 재생되어 테스트하기 편해요.")]
    public bool playOnEnableForTest = false;
    public UnlockableFeature featureToPlayOnEnable;

    private Dictionary<UnlockableFeature, FeatureUIData> featureDataDict;
    private bool isInitialized = false;

    [Header("애니메이션 설정")]
    public float slideDuration = 0.5f;
    public float fadeDuration = 0.4f;
    public float shackleUnlockDuration = 0.3f;
    public float shackleMoveDistance = 25f;
    public float stayDuration = 1.5f;

    private Vector2 panelOnScreenPosition;
    private Vector2 panelOffScreenRight;
    private Vector2 panelOffScreenLeft;
    private Vector2 shackleStartPosition;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        if (playOnEnableForTest)
        {
            PlayUnlockEffect(featureToPlayOnEnable);
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (panelRect != null)
        {
            panelOnScreenPosition = panelRect.anchoredPosition;
            float offscreenDelta = Screen.width / 2f + panelRect.rect.width / 2f;
            panelOffScreenRight = new Vector2(panelOnScreenPosition.x + offscreenDelta, panelOnScreenPosition.y);
            panelOffScreenLeft = new Vector2(panelOnScreenPosition.x - offscreenDelta, panelOnScreenPosition.y);
        }
        if (lockShackleRect != null) { shackleStartPosition = lockShackleRect.anchoredPosition; }

        featureDataDict = new Dictionary<UnlockableFeature, FeatureUIData>();
        foreach (var data in featureDataList)
        {
            if (!featureDataDict.ContainsKey(data.feature))
            {
                featureDataDict.Add(data.feature, data);
            }
        }
        isInitialized = true;
    }

    public void PlayUnlockEffect(UnlockableFeature featureToUnlock)
    {
        if (!isInitialized) Initialize();

        if (!featureDataDict.TryGetValue(featureToUnlock, out FeatureUIData data))
        {
            Debug.LogError($"앗! '{featureToUnlock}'에 대한 기능 데이터가 없어요!");
            EffectPoolManager.Instance.ReturnToPool(gameObject);
            return;
        }

        featureNameText.text = data.featureName;
        featureIconImage.sprite = data.featureIcon;

        PlayAnimation(data.featureName);
    }

    private void PlayAnimation(string featureName)
    {
        DOTween.Kill(transform, true);

        Sequence unlockSequence = DOTween.Sequence().SetTarget(transform);

        // 1. 시작 상태 설정
        unlockSequence.AppendCallback(() =>
        {
            panelRect.anchoredPosition = panelOffScreenRight;
            lockCanvasGroup.alpha = 0f;
            lockShackleRect.anchoredPosition = shackleStartPosition;
            Debug.Log($"'{featureName}' 기능 해금! 애니메이션을 시작합니다! ★_★");
        });

        // 2. 자물쇠 페이드 인
        unlockSequence.Append(lockCanvasGroup.DOFade(1f, fadeDuration));
        unlockSequence.AppendInterval(0.1f);

        // 3. 패널 슬라이드 인 & 자물쇠 잠금 해제
        unlockSequence.Append(panelRect.DOAnchorPos(panelOnScreenPosition, slideDuration).SetEase(Ease.OutQuad));
        unlockSequence.Join(lockShackleRect.DOAnchorPosY(shackleStartPosition.y + shackleMoveDistance, shackleUnlockDuration).SetEase(Ease.OutBack));

        // 4. 잠시 대기
        unlockSequence.AppendInterval(stayDuration);

        // 5. 패널 슬라이드 아웃 & 자물쇠 페이드 아웃
        unlockSequence.Append(panelRect.DOAnchorPos(panelOffScreenLeft, slideDuration).SetEase(Ease.InQuad));
        unlockSequence.Join(lockCanvasGroup.DOFade(0f, fadeDuration));

        // 6. 종료 처리
        unlockSequence.OnComplete(() =>
        {
            Debug.Log("애니메이션 끝! 오브젝트를 풀에 반납합니다.");
            EffectPoolManager.Instance.ReturnToPool(gameObject);
        });

        unlockSequence.Play();
    }

    [ContextMenu("테스트: 설정된 기능으로 애니메이션 재생")]
    private void PlayTestEffectFromInspector()
    {
        if (!isInitialized) Initialize();
        PlayUnlockEffect(featureToPlayOnEnable);
    }
}
