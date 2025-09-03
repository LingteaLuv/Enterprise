using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// 보스 도전 버튼을 누른 즉시 나오는 연출
/// </summary>
public class BossBattleButtonEffect : MonoBehaviour
{
    // .Append() (연결): 이전 트윈이 끝난 후 다음 트윈을 이어서 실행합니다.
    // .Join() (동시 실행): 이전 트윈과 동시에 다음 트윈을 실행합니다.
    // .AppendInterval() (간격 추가): 지정한 시간만큼 대기한 후 다음 트윈을 실행합니다.
    // .AppendCallback() (콜백 추가): 지정한 콜백 함수를 실행한 후 다음 트윈을 실행합니다.
    // 콜백함수 : 특정 이벤트가 발생했을 때 호출되는 함수입니다. 예를 들어, 버튼 클릭, 애니메이션 완료, 타이머 종료 등이 있습니다.
    // DOTween.Sequence() : 여러 개의 트윈을 순차적으로 또는 동시에 실행할 수 있는 시퀀스를 생성합니다.
    // DOTween.Kill() : 현재 실행 중인 트윈이나 시퀀스를 즉시 종료합니다. 
    // DOTween.Restart() : 일시정지된 트윈이나 시퀀스를 처음부터 다시 시작합니다.
    // DOTween.Pause() : 현재 실행 중인 트윈이나 시퀀스를 일시정지합니다.
    // DOTween.Play() : 일시정지된 트윈이나 시퀀스를 다시 재생합니다.
    // DOTween.Rewind() : 트윈이나 시퀀스를 처음 상태로 되돌립니다.
    // DOTween.Flip() : 트윈이나 시퀀스의 진행 방향을 반대로 전환합니다.
    // DOFade () : CanvasGroup의 알파 값을 트윈합니다. ex) fadeCanvasGroup.DOFade(1f, 0.6f) // 0.6초 동안 알파 값을 1로 변경


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip alertSiren;    // 경고 사이렌
    public AudioClip b; // 임시
    public AudioClip c; // 임시

    [Header("경고 벨트")]
    public CanvasGroup WarningBeltCanvasGroup;
    public TextMeshProUGUI WarningBeltTMP;
    private Vector3 WarningBeltTextPos;

    // DOTween 시퀀스
    private Sequence bossBattleButtonIntro;

    // SeatBelt 뜻은 :

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

    // 플레이 메서드 만들어서 외부에서 호출
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
                .Join(WarningBeltTMP.rectTransform.DORotate(new Vector3(0f, 0f, 10f), 0.1f).SetLoops(2, LoopType.Yoyo));

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