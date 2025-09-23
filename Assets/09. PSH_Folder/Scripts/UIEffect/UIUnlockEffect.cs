using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic; // Dictionary를 사용하기 위해 필요해요!
using TMPro;

// 밖에서도 이 이넘을 사용할 수 있도록 클래스 바깥에 정의해요!
// 여기에 해금할 기능들을 쭉 추가해서 관리할 수 있어요.
public enum UnlockableFeature
{
    None,
    TenGacha,
    EquipGacha,
    RelicGacha,
}

// [System.Serializable]을 붙여야 인스펙터에 예쁘게 보여요!
[System.Serializable]
public struct FeatureUIData
{
    public UnlockableFeature feature; // 어떤 기능인지
    public string featureName;        // 표시될 기능의 이름
    public Sprite featureIcon;        // 표시될 기능의 아이콘
}

/// <summary>
/// [최종판] 기능 해금 시 나타나는 UI 효과를 담당하는 스크립트입니다.
/// 해금할 기능의 데이터를 미리 저장해두고, enum 값으로 간단히 호출하여 사용합니다.
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
    [Tooltip("여기에 해금될 기능들의 이름과 아이콘을 미리 설정해주세요.")]
    public List<FeatureUIData> featureDataList;

    [Header("자동 재생 설정")]
    [Tooltip("오브젝트가 활성화될 때 자동으로 재생할 기능입니다.")]
    public UnlockableFeature featureToPlayOnEnable;

    // 빠른 조회를 위해 List를 Dictionary로 변환해서 저장할 거예요!
    private Dictionary<UnlockableFeature, FeatureUIData> featureDataDict;

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
        // --- 위치 계산 --- 
        if (panelRect != null)
        {
            panelOnScreenPosition = panelRect.anchoredPosition;
            float offscreenDelta = Screen.width / 2f + panelRect.rect.width / 2f;
            panelOffScreenRight = new Vector2(panelOnScreenPosition.x + offscreenDelta, panelOnScreenPosition.y);
            panelOffScreenLeft = new Vector2(panelOnScreenPosition.x - offscreenDelta, panelOnScreenPosition.y);
        }
        if (lockShackleRect != null)
        {
            shackleStartPosition = lockShackleRect.anchoredPosition;
        }

        // --- 데이터 최적화 --- 
        InitializeDictionary();
    }

    /// <summary>
    /// 기능 해금 애니메이션을 재생합니다. 해금할 기능의 enum 값만 넘겨주세요!
    /// </summary>
    public void PlayUnlockEffect(UnlockableFeature featureToUnlock)
    {
        // 딕셔너리가 준비되었는지 다시 한번 확인해요! (에디터에서 테스트 시 안전장치)
        if (featureDataDict == null || featureDataDict.Count == 0) { InitializeDictionary(); }

        if (!featureDataDict.TryGetValue(featureToUnlock, out FeatureUIData data))
        {
            Debug.LogError($"앗! '{featureToUnlock}'에 대한 기능 데이터가 리스트에 없어요! 인스펙터에서 추가했는지 확인해주세요!");
            return;
        }

        featureNameText.text = data.featureName;
        featureIconImage.sprite = data.featureIcon;

        PlayAnimation(data.featureName);
    }

    private void PlayAnimation(string featureName)
    {
        // 이전 재생되던 트윈이 있다면 확실하게 종료시켜요.
        DOTween.Kill(transform, true);

        Sequence unlockSequence = DOTween.Sequence().SetTarget(transform);
        unlockSequence.AppendCallback(() =>
        {
            panelRect.anchoredPosition = panelOffScreenRight;
            lockCanvasGroup.alpha = 0f;
            lockShackleRect.anchoredPosition = shackleStartPosition;
            Debug.Log($"'{featureName}' 기능 해금! 애니메이션을 시작합니다! ★_★");
        });
        unlockSequence.Append(lockCanvasGroup.DOFade(1f, fadeDuration));
        unlockSequence.AppendInterval(0.1f);
        unlockSequence.Append(panelRect.DOAnchorPos(panelOnScreenPosition, slideDuration).SetEase(Ease.OutQuad));
        unlockSequence.Join(lockShackleRect.DOAnchorPosY(shackleStartPosition.y + shackleMoveDistance, shackleUnlockDuration).SetEase(Ease.OutBack));
        unlockSequence.AppendInterval(stayDuration);
        unlockSequence.Append(panelRect.DOAnchorPos(panelOffScreenLeft, slideDuration).SetEase(Ease.InQuad));
        unlockSequence.Join(lockCanvasGroup.DOFade(0f, fadeDuration));
        unlockSequence.OnComplete(() =>
        {
            Debug.Log("애니메이션 끝! 오브젝트를 비활성화할게요!");
            gameObject.SetActive(false);
        });
        unlockSequence.Play();
    }

    private void InitializeDictionary()
    {
        featureDataDict = new Dictionary<UnlockableFeature, FeatureUIData>();
        foreach (var data in featureDataList)
        {
            if (!featureDataDict.ContainsKey(data.feature))
            {
                featureDataDict.Add(data.feature, data);
            }
        }
    }

    [ContextMenu("테스트: 설정된 기능으로 애니메이션 재생")]
    private void PlayTestEffectFromInspector()
    {
        PlayUnlockEffect(featureToPlayOnEnable);
    }
}
