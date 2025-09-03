using DG.Tweening;
using UnityEngine;
using TMPro;

/// <summary>
/// 보스 도전 버튼을 누른 즉시 나오는 연출
/// </summary>
public class BossBattleButtonDirection : MonoBehaviour
{
    [Header("오디오")]
    public AudioClip alertSiren;    // 경고 사이렌
    private AudioSource audioSource;

    [Header("경고 벨트")]
    public CanvasGroup WarningBeltCanvasGroup;
    public TextMeshProUGUI WarningBeltTMP; 
    private Vector3 WarningBeltTextPos;

    // DOTween 시퀀스
    private Sequence bossBattleButtonIntro;

    private void Awake()
    {
        if (!audioSource)
        audioSource = GetComponent<AudioSource>();
        WarningBeltTextPos = WarningBeltTMP.rectTransform.localPosition;
    }

    void Start()
    {
        InitializeUI();             // UI 초기화
        StartBossIntroSequence();   // 시퀸스 저장
    }

    // 외부에서 호출
    [ContextMenu("OnBossChallenge")]
    public void OnBossChallenge() // 보스 도전 버튼 눌렀을 때 호출
    {
        bossBattleButtonIntro.Restart();
    }

    [ContextMenu("초기화")]
    public void InitializeUI() // UI 초기화 외부에서 호출
    {
        audioSource.Stop();
        bossBattleButtonIntro?.Pause(); // 시퀀스 일시정지

        WarningBeltCanvasGroup.alpha = 0f;
        WarningBeltTMP.rectTransform.localPosition = WarningBeltTextPos;

    }

    private void StartBossIntroSequence()
    {
        Debug.Log("실행 테스트");
        // 새로운 DOTween 시퀀스 생성
        bossBattleButtonIntro = DOTween.Sequence();
        // 시퀀스 구성
        bossBattleButtonIntro.Append(SeatBeltAppearance())                      // 벨트 서서히 등장
                                .AppendCallback(() => PlaySound(alertSiren))    // 경고 사이렌
                                .Append(SeatBeltText());                        // 벨트 텍스트 날라옴
                                
                                // 초기화를 이쪽에서 할지 외부에서 해야할지 고민중
                                // 이쪽에서 초기화 한다면 여기 애니메이션이 재생하고 바로 혼자서 초기화됨
                                // 장면 전환과 타이밍이 맞지 않다면 어색할수 있음
                                // 초기화를 밖에서 한다면 장면 전환중에 초기화 할수도 있어 밖에선 타이밍에 잡기에 어려움이 있음

        bossBattleButtonIntro.Pause(); // 자동 재생 비활성화 설정에 따라 일시정지 상태로 시작
        bossBattleButtonIntro.SetAutoKill(false); // 시퀀스가 끝나도 자동으로 제거되지 않도록 설정
    }

    private Tween SeatBeltAppearance() // 경고 벨트 서서히 등장
    {
        return WarningBeltCanvasGroup.DOFade(1f, 1.5f);
    }

    private Tween SeatBeltText() // 경고 벨트 텍스트 날라옴
    {
        var sequence = DOTween.Sequence();

        sequence.Append(WarningBeltTMP.rectTransform.DOLocalMoveX(0f, 0.5f).SetEase(Ease.OutBack))
                .Append(WarningBeltTMP.rectTransform.DOScale(1.3f, 0.15f))
                .Append(WarningBeltTMP.rectTransform.DOScale(1f, 0.15f).SetEase(Ease.OutBack))
                .Join(WarningBeltTMP.rectTransform.DORotate(new Vector3(0f, 0f, 20f), 0.07f).SetLoops(4, LoopType.Yoyo));

        return sequence;
    }

    private void PlaySound(AudioClip clip) // 오디오 클립 재생
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}