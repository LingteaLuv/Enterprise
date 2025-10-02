using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class LogoDirector : MonoBehaviour
{
    [Header("애니메이션 대상 오브젝트")]
    public Transform object1;
    public Transform object2;
    public Transform object3; // RectTransform을 가진 UI 오브젝트여야 합니다.
    public Transform object4;

    [Header("1번 오브젝트: 낙하 & 흔들기 설정")]
    [SerializeField] private float obj1_startOffsetY = 10f;
    [SerializeField] private float obj1_dropDuration = 0.8f;
    [SerializeField] private Ease obj1_dropEase = Ease.OutBounce;
    [SerializeField] private float obj1_shakeDuration = 0.5f;
    [SerializeField] private float obj1_shakeStrength = 5f;

    [Header("2번 오브젝트: 스케일 업 설정")]
    [SerializeField] private float obj2_scaleDuration = 0.5f;
    [SerializeField] private Ease obj2_scaleEase = Ease.OutBack;

    [Header("3번 오브젝트: 급정거 등장 설정")]
    [SerializeField] private float obj3_startOffsetX = 20f;
    [SerializeField] private float obj3_moveDuration = 0.7f;
    [SerializeField] private Ease obj3_moveEase = Ease.OutQuint;

    [Header("3번 오브젝트: 대각선 늘리기 설정")]
    [Tooltip("최대로 늘어날 가로 길이 배율")]
    [SerializeField] private float obj3_stretchWidthFactor = 2.5f;
    [Tooltip("최대로 줄어들 세로 길이 배율")]
    [SerializeField] private float obj3_squashHeightFactor = 0.5f;
    [Tooltip("늘어나는 데 걸리는 시간")]
    [SerializeField] private float obj3_stretchDuration = 0.2f;
    [Tooltip("원래대로 돌아오는 데 걸리는 시간")]
    [SerializeField] private float obj3_returnDuration = 0.6f;
    [SerializeField] private Ease obj3_stretchEase = Ease.OutQuad;
    [SerializeField] private Ease obj3_returnEase = Ease.OutElastic;

    [Header("4번 오브젝트: 3단 회전 설정")]
    [Tooltip("첫 번째 회전 각도")]
    [SerializeField] private float obj4_angle1 = 45f;
    [Tooltip("첫 번째 회전에 걸리는 시간")]
    [SerializeField] private float obj4_duration1 = 0.4f;
    [SerializeField] private Ease obj4_ease1 = Ease.InOutSine;

    [Space(10)]
    [Tooltip("두 번째 회전 각도")]
    [SerializeField] private float obj4_angle2 = -45f;
    [Tooltip("두 번째 회전에 걸리는 시간")]
    [SerializeField] private float obj4_duration2 = 0.4f;
    [SerializeField] private Ease obj4_ease2 = Ease.InOutSine;

    [Space(10)]
    [Tooltip("마지막 회전 각도 (보통 0으로 설정하여 원위치)")]
    [SerializeField] private float obj4_angle3 = 0f;
    [Tooltip("원위치로 돌아오는 데 걸리는 시간")]
    [SerializeField] private float obj4_duration3 = 0.2f;
    [SerializeField] private Ease obj4_ease3 = Ease.InOutSine;

    private Vector3 obj1_startPos, obj1_endPos;
    private Vector3 obj2_startScale, obj2_endScale;
    private Vector3 obj3_startPos, obj3_endPos;
    private Vector3 obj4_originalRotation;

    private RectTransform obj3_rectTransform;
    private Vector2 obj3_originalSize;

    private Sequence mainSequence;
    private Sequence object4Sequence;

    private void Awake()
    {
        if (!ValidateObjects()) return;
        StoreInitialStates();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        mainSequence?.Kill();
        object4Sequence?.Kill();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayAllAnimations();
    }

    [ContextMenu("테스트 플레이 (Play Animation In Editor)")]
    public void PlayAllAnimations()
    {
        if (!ValidateObjects()) return;
        ResetToStart();

        mainSequence?.Kill();
        object4Sequence?.Kill();

        mainSequence = DOTween.Sequence();
        Vector2 stretchedSize = new Vector2(obj3_originalSize.x * obj3_stretchWidthFactor, obj3_originalSize.y * obj3_squashHeightFactor);

        mainSequence.AppendInterval(.5f)
                    .Append(object1.DOMove(obj1_endPos, obj1_dropDuration).SetEase(obj1_dropEase))
                    .Append(object1.DOShakePosition(obj1_shakeDuration, obj1_shakeStrength, 15))
                    .Append(object2.DOScale(obj2_endScale, obj2_scaleDuration).SetEase(obj2_scaleEase))
                    .Join(object3.DOMove(obj3_endPos, obj3_moveDuration).SetEase(obj3_moveEase))
                    // --- 3번 오브젝트의 대각선 늘리기 애니메이션 ---
                    .Append(obj3_rectTransform.DOSizeDelta(stretchedSize, obj3_stretchDuration).SetEase(obj3_stretchEase))
                    .Append(obj3_rectTransform.DOSizeDelta(obj3_originalSize, obj3_returnDuration).SetEase(obj3_returnEase));
        mainSequence.Play();

        object4Sequence = DOTween.Sequence();
        object4Sequence.AppendInterval(obj1_dropDuration)
                       .Append(object4.DORotate(new Vector3(0, 0, obj4_angle1), obj4_duration1).SetEase(obj4_ease1))
                       .Append(object4.DORotate(new Vector3(0, 0, obj4_angle2), obj4_duration2).SetEase(obj4_ease2))
                       .Append(object4.DORotate(obj4_originalRotation, obj4_duration3).SetEase(obj4_ease3));
        object4Sequence.Play();
    }

    private void StoreInitialStates()
    {
        obj1_endPos = object1.position;
        obj1_startPos = new Vector3(obj1_endPos.x, obj1_endPos.y + obj1_startOffsetY, obj1_endPos.z);
        obj2_endScale = object2.localScale;
        obj2_startScale = Vector3.zero;
        obj3_endPos = object3.position;
        obj3_startPos = new Vector3(obj3_endPos.x + obj3_startOffsetX, obj3_endPos.y, obj3_endPos.z);
        obj4_originalRotation = object4.eulerAngles;

        obj3_rectTransform = object3.GetComponent<RectTransform>();
        if (obj3_rectTransform != null)
        {
            obj3_originalSize = obj3_rectTransform.sizeDelta;
        }
    }

    private void ResetToStart()
    {
        DOTween.Kill(object1);
        DOTween.Kill(object2);
        DOTween.Kill(object3);
        DOTween.Kill(object4);

        object1.position = obj1_startPos;
        object2.localScale = obj2_startScale;
        object3.position = obj3_startPos;
        object4.eulerAngles = obj4_originalRotation;

        if (obj3_rectTransform != null)
        {
            obj3_rectTransform.sizeDelta = obj3_originalSize;
        }
    }

    private bool ValidateObjects()
    {
        if (object1 == null || object2 == null || object3 == null || object4 == null)
        {
            Debug.LogError("[LogoDirector] 인스펙터에서 모든 대상 오브젝트를 연결해주세요!");
            return false;
        }
        return true;
    }
}