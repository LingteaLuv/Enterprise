using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

/// <summary>
/// 장비 가챠(단조) 연출을 관리하고 재생하는 클래스입니다.
/// </summary>
public class EquipmentGachaDirector : MonoBehaviour
{
    [Header("연출 요소")]
    public RectTransform container;
    public Image hammerImage;
    public Image weaponImage;

    [Header("연출 흐름 설정")]
    public float startDelay = 1f;
    public float zoomScale = 1.5f;
    public int strikeCount = 3;
    public float strikeDuration = 1f;
    public float endDelay = 1f;

    [Header("세부 애니메이션 설정")]
    public float hammerWindUpAngle = -45f;
    public float hammerStrikeMoveDistanceY = 150f;
    public float weaponShakeDuration = 0.2f;
    public float weaponShakeStrength = 20f;

    [Header("스킵 버튼")]
    public Button skipBtn;

    private Sequence mainSequence;

    // --- [수정] 모든 애니메이션 요소의 초기 상태를 저장하기 위한 변수들 ---
    private Dictionary<Transform, Vector3> _initialPositions;
    private Dictionary<Transform, Vector3> _initialRotations;
    private Dictionary<Transform, Vector3> _initialScales;
    private bool _isInitialized = false;

    ImageMaterialChanger materialChanger;

    void Awake()
    {
        InitializeTransforms();
        materialChanger = weaponImage.gameObject.GetComponent<ImageMaterialChanger>();
    }
    private void OnEnable()
    {
        skipBtn.onClick.AddListener(() => { EffectPoolManager.Instance.ReturnToPool(gameObject); });
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        skipBtn.onClick.RemoveAllListeners();
    }
    private void InitializeTransforms()
    {
        if (_isInitialized) return;
        _initialPositions = new Dictionary<Transform, Vector3>();
        _initialRotations = new Dictionary<Transform, Vector3>();
        _initialScales = new Dictionary<Transform, Vector3>();

        StoreTransform(container);
        StoreTransform(hammerImage?.transform);
        StoreTransform(weaponImage?.transform);

        _isInitialized = true;
    }

    private void StoreTransform(Transform target)
    {
        if (target == null) return;
        _initialPositions[target] = target.localPosition;
        _initialRotations[target] = target.localEulerAngles;
        _initialScales[target] = target.localScale;
    }

    private void SetupInitialState()
    {
        if (!_isInitialized) InitializeTransforms();

        if (mainSequence != null && mainSequence.IsActive()) mainSequence.Kill();

        // 모든 자식 트윈을 중지하고, 저장된 초기 상태로 되돌립니다.
        foreach (var pair in _initialPositions)
        {
            Transform target = pair.Key;
            if (target != null)
            {
                target.DOKill();
                target.localPosition = _initialPositions[target];
                target.localEulerAngles = _initialRotations[target];
                target.localScale = _initialScales[target];
            }
        }
    }

    public void Play(Action onComplete = null)
    {
        SetupInitialState();
        mainSequence = DOTween.Sequence();

        mainSequence.AppendInterval(startDelay);

        // 타이밍 세분화
        float windUpTime = strikeDuration * 0.4f;   // 준비
        float strikeTime = strikeDuration * 0.2f;   // 내려치기
        float recoverTime = strikeDuration * 0.4f;  // 복귀

        Vector3 hammerRotOrigin = _initialRotations[hammerImage.transform];
        float hammerInitialY = _initialPositions[hammerImage.transform].y;

        for (int i = 0; i < strikeCount; i++)
        {
            // 준비동작 (위로 올리기)
            mainSequence.Append(
                hammerImage.rectTransform.DORotate(
                    new Vector3(0, 0, hammerWindUpAngle),
                    windUpTime
                ).SetEase(Ease.OutQuad)
            ); 
            mainSequence.Join(
                hammerImage.rectTransform.DOAnchorPosY(
                    hammerInitialY + hammerStrikeMoveDistanceY,
                    strikeTime
                ).SetEase(Ease.InQuad)
            );

            // 내려치기 시작(이펙트도 이 시점에)

            mainSequence.Append(
                hammerImage.rectTransform.DORotate(
                    new Vector3(0, 0, -hammerWindUpAngle * 0.5f),
                    strikeTime
                ).SetEase(Ease.InQuad)
            );
            mainSequence.Join(
                hammerImage.rectTransform.DOAnchorPosY(
                    hammerInitialY,
                    strikeTime
                ).SetEase(Ease.InQuad)
            );

            // 복귀(튕김 느낌)
            mainSequence.AppendCallback(() =>
            {
                if (materialChanger != null)
                    materialChanger.AddEffect(EffectType.ForgeFlare, .2f);
            });
            mainSequence.Append(
                hammerImage.rectTransform.DORotate(
                    hammerRotOrigin,
                    recoverTime
                ).SetEase(Ease.OutBounce)
            );

            // 무기 흔들기 (같이 조인)
            mainSequence.Join(
                weaponImage.transform.DOShakePosition(
                    weaponShakeDuration,
                    new Vector3(weaponShakeStrength, 0, 0),
                    10, 90, false, true
                )
            );
        }

        // 전체 컨테이너 확대 연출
        float totalStrikeDuration = strikeDuration * strikeCount;
        container.DOScale(zoomScale, totalStrikeDuration).SetEase(Ease.InOutSine);

        mainSequence.AppendInterval(endDelay);
        mainSequence.OnComplete(() =>
        {
            Debug.Log("장비 가챠 연출 완료!");
            onComplete?.Invoke();
        });
    }

    [ContextMenu("Test Play Equipment Animation")]
    private void TestPlay()
    {
        Play(() => Debug.Log("장비 테스트 연출 콜백 호출됨"));
    }
}
