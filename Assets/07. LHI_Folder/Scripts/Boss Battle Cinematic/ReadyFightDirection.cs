using DG.Tweening;
using TMPro;
using UnityEngine;

public class ReadyFightDirection : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TextMeshProUGUI readyText;
    private Vector3 readyTextPos;
    public TextMeshProUGUI fightText;
    public Transform plankTransform; // 판자 오브젝트
    private Vector3 plankPos; // 판자의 초기 위치 저장

    [Header("오디오")]
    public AudioClip readySound;
    public AudioClip fightSound;
    public AudioClip plankDropSound;
    private AudioSource audioSource;

    [Header("추가 효과")]
    // [SerializeField] private ParticleSystem sparksEffect; // 파티클 공부 필요
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraShakeIntensity = 0.12f;
    [SerializeField] private float cameraShakeDuration = 0.03f;

    // DOTween 시퀀스
    private Sequence readyFightSequence;

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        readyTextPos = readyText.rectTransform.localPosition;
        plankPos = plankTransform.localPosition;
    }

    private void Start()
    {
        InitializeUI();                 // UI 초기화    
        ReadyFightDirectionSequence();  // 시퀸스 저장
    }

    // 외부에서 호출
    [ContextMenu("PlayReadyFightDirection")]
    public void PlayReadyFightDirection() // 보스 전투를 시작할때 호출
    {
        readyFightSequence.Restart();
    }

    [ContextMenu("초기화")]
    public void InitializeUI() // UI 초기화 외부에서 호출
    {
        audioSource.Stop();
        readyFightSequence?.Pause(); // 시퀀스 일시정지

        readyText.rectTransform.localPosition = readyTextPos;
        plankTransform.localPosition = plankPos;
        readyText.alpha = 1f;

        fightText.alpha = 0f;
    }

    private void ReadyFightDirectionSequence()
    {
        // 새로운 DOTween 시퀀스 생성
        readyFightSequence = DOTween.Sequence();

        // 시퀀스 구성
        readyFightSequence.Append(readyText.DOFade(1f, 2f))
                        .Join(ReadyTextDrop())
                        .AppendCallback(() => PlankDrop())
                        .AppendInterval(0.57f) // 판자가 떨어지기 전에 약간의 딜레이 추가
                        .AppendCallback(() => readyText.alpha = 0f)
                        // 레디 텍스트 위치를 판자에 약간 위에 하면 판자가 지나가며 레디 텍스트가 사라지는 듯한 효과
                        // 진짜 지나갈때 사라지게 하려면 타이밍을 맞출 필요가 있음
                        .AppendInterval(0.8f)  // 잠시 대기
                        .Append(FightTextAppearance())
                        .AppendInterval(1f)
                        .AppendCallback(() => fightText.DOFade(0f, 0.6f))
                        .AppendCallback(() => Debug.Log("보스 전투 시작!")); // 보스 전투 시작 알림

        readyFightSequence.Pause(); // 자동 재생 비활성화
        readyFightSequence.SetAutoKill(false); // 시퀀스가 끝나도 자동으로 제거되지 않도록 설정
    }

    // 레디 텍스트 떨어지고 사운드 재생, 카메라 흔들림
    private Tween ReadyTextDrop()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(readyText.rectTransform.DOAnchorPosY(250f, 1.2f).SetEase(Ease.InCubic))
                .AppendCallback(() => PlaySound(readySound));
        return sequence;
    }

    // 판자 떨어지고 사운드 재생, 카메라 흔들림
    private Tween PlankDrop()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(plankTransform.DOLocalMoveY(0f, 0.7f).SetEase(Ease.InCubic))
                .AppendCallback(() => PlaySound(plankDropSound))
                .AppendCallback(() => CameraShake()); // 카메라 흔들림
        return sequence;
    }

    // 카메라 흔들기
    private Tween CameraShake()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(mainCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity))
                .Append(mainCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity))
                .Append(mainCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity))
                .Append(mainCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity))
                .Append(mainCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity));
        return sequence;
    }

    // 파이트 텍스트 등장
    private Tween FightTextAppearance()
    {
        var sequence = DOTween.Sequence();

        sequence.AppendCallback(() => fightText.alpha = 1f)
                .AppendCallback(() => PlaySound(fightSound))
                .Append(fightText.transform.DOScale(3f, 0.2f).From(0f).SetEase(Ease.OutBack))
                .Append(fightText.transform.DOScale(2f, 0.1f).SetEase(Ease.InCubic));
        return sequence;
    }

    private void PlaySound(AudioClip clip) // 오디오 클립 재생
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDisable()
    {
        readyFightSequence?.Kill();
        readyFightSequence = null;
    }
}
