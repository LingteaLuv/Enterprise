using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossBattleIntroManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public CanvasGroup orangeTextCanvasGroup;   // 주황색 텍스트 캔버스 그룹
    public TextMeshProUGUI orangeText;          // 주황색 텍스트
    public RectTransform orangeTextRect;        // 주황색 텍스트 RectTransform
    public CanvasGroup fadeCanvasGroup;         // 페이드 캔버스 그룹
    public CanvasGroup readyFightCanvasGroup;   // READY/FIGHT 캔버스 그룹
    public TextMeshProUGUI readyText;           // READY 텍스트
    public TextMeshProUGUI fightText;           // FIGHT 텍스트
    public RectTransform plankTransform;        // 판자 RectTransform

    [Header("DOTween 애니메이션 설정")]
    [SerializeField] private float orangeTextDuration = 1.5f;
    [SerializeField] private float orangeTextStayDuration = 1.2f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float readyStayDuration = 1f;
    [SerializeField] private float plankDropDuration = 1.2f;
    [SerializeField] private float fightStayDuration = 1.5f;

    [Header("이징 설정")]
    [SerializeField] private Ease orangeTextEase = Ease.OutBack;    // 주황색 텍스트 등장 이징
    [SerializeField] private Ease fadeEase = Ease.InOutCubic;
    [SerializeField] private Ease plankEase = Ease.InCubic;
    [SerializeField] private Ease textScaleEase = Ease.OutElastic;

    [Header("오디오")]
    public AudioSource audioSource;
    public AudioClip orangeTextSound;
    public AudioClip readySound;
    public AudioClip fightSound;
    public AudioClip plankDropSound;

    [Header("추가 효과")]
    [SerializeField] private ParticleSystem sparksEffect;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraShakeIntensity = 0.3f;
    [SerializeField] private float cameraShakeDuration = 0.5f;

    // 진입 조건
    public bool isAllStageCleared = false;

    // DOTween 시퀀스
    private Sequence introSequence;

    private void Start()
    {
        InitializeUI();
        SetupDOTweenSettings();
    }

    private void SetupDOTweenSettings()
    {
        // DOTween 전역 설정
       // DOTween.defaultEaseType = Ease.OutCubic; // 기본 이징 설정
        // DOTween.defaultAutoPlay = AutoPlay.None; // 자동 재생 비활성화
    }

    private void InitializeUI()
    {
        // 초기 상태 설정
        orangeTextCanvasGroup.alpha = 0f;
        orangeTextRect.anchoredPosition = new Vector2(Screen.width + 200f, 0f);
        orangeText.text = "BOSS BATTLE!"; // 임시에서 실제 텍스트로 변경

        fadeCanvasGroup.alpha = 0f;
        readyFightCanvasGroup.alpha = 0f;

        readyText.gameObject.SetActive(true);
        fightText.gameObject.SetActive(false);

        // 텍스트 초기 스케일 설정
        readyText.transform.localScale = Vector3.zero;
        fightText.transform.localScale = Vector3.zero;

        // 판자 초기 위치 (화면 위쪽 바깥)
        plankTransform.anchoredPosition = new Vector2(0f, Screen.height + 100f);
    }

    public void OnBossChallenge() // 보스 도전 버튼 눌렀을 때 호출
    {
        StartBossIntroSequence();
    }

    public void OnAllStageCleared() //
    {
        if (!isAllStageCleared)
        {
            isAllStageCleared = true;
            StartBossIntroSequence();
        }
    }

    private void StartBossIntroSequence()
    {
        // DOTween.Append() (연결): 이전 트윈이 끝난 후 다음 트윈을 이어서 실행합니다.
        // DOTween.Join() (동시 실행): 이전 트윈과 동시에 다음 트윈을 실행합니다.
        // DOTween.AppendInterval() (간격 추가): 지정한 시간만큼 대기한 후 다음 트윈을 실행합니다.
        // DOTween.AppendCallback() (콜백 추가): 지정한 콜백 함수를 실행한 후 다음 트윈을 실행합니다.
        // 콜백함수 : 특정 이벤트가 발생했을 때 호출되는 함수입니다. 예를 들어, 버튼 클릭, 애니메이션 완료, 타이머 종료 등이 있습니다.
        // DOTween.Sequence() : 여러 개의 트윈을 순차적으로 또는 동시에 실행할 수 있는 시퀀스를 생성합니다.
        // DOTween.Kill() : 현재 실행 중인 트윈이나 시퀀스를 즉시 종료합니다.
        // DOTween.Restart() : 일시정지된 트윈이나 시퀀스를 처음부터 다시 시작합니다.
        // DOTween.Pause() : 현재 실행 중인 트윈이나 시퀀스를 일시정지합니다.
        // DOTween.Play() : 일시정지된 트윈이나 시퀀스를 다시 재생합니다.
        // DOTween.Rewind() : 트윈이나 시퀀스를 처음 상태로 되돌립니다.
        // DOTween.Flip() : 트윈이나 시퀀스의 진행 방향을 반대로 전환합니다.
        // DOTween.KillAll() : 모든 트윈과 시퀀스를 즉시 종료합니다.
        // DOTween.Clear() : 모든 트윈과 시퀀스를 메모리에서 제거합니다.
        // DOTween.Goto() : 특정 시간이나 위치로 트윈이나 시퀀스를 이동합니다.
        // DOTween.DOFade() : CanvasGroup의 알파 값을 트윈합니다. ex) fadeCanvasGroup.DOFade(1f, 0.6f) // 0.6초 동안 알파 값을 1로 변경  
        // DOTween.DOColor() : TextMeshProUGUI의 색상을 트윈합니다. ex) fightText.DOColor(Color.red, 0.3f).SetLoops(6, LoopType.Yoyo) // 빨간색으로 변경 후 원래 색상으로 반복
        // DOTween.AnchorPosX() : RectTransform의 X 위치를 트윈합니다. ex) orangeTextRect.DOAnchorPosX(0f, 1.5f).SetEase(Ease.OutBack) // X 위치를 0으로 변경
        // DOTween.DOJump() : RectTransform을 점프시키는 트윈입니다. ex) readyText.rectTransform.DOJump(endPos.position, jumpHeight, 1, 1f).SetEase(Ease.OutCubic)
        // 각 파라미터는 (목표값, 지속시간, 점프횟수, 높이)
        // DOTween.From : 현재 값에서 목표값으로 트윈합니다. ex) fightText.transform.DOScale(Vector3.one, 0.6f).From(Vector3.zero).SetEase(Ease.OutBounce) // 스케일을 0에서 1로 변경

        // 기존 시퀀스가 있다면 종료
        introSequence?.Kill();

        // 새로운 DOTween 시퀀스 생성
        introSequence = DOTween.Sequence();

        // 1단계: 주황색 텍스트 등장
        introSequence.AppendCallback(() => PlaySound(orangeTextSound))
                    .Append(ShowOrangeText())
                    .AppendInterval(orangeTextStayDuration)
                    .Append(HideOrangeText())

                    // 2단계: 페이드 아웃
                    .Append(fadeCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))

                    // 3단계: READY 표시와 함께 페이드 인
                    .AppendCallback(() => {
                        readyFightCanvasGroup.alpha = 1f;
                        PlaySound(readySound);
                    })
                    .Append(fadeCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase))
                    .Join(readyText.transform.DOScale(Vector3.one, 0.8f).SetEase(textScaleEase))
                    .AppendInterval(readyStayDuration)

                    // 4단계: 판자 하강과 READY → FIGHT 전환
                    .AppendCallback(() => {
                        PlaySound(plankDropSound);
                        if (sparksEffect) sparksEffect.Play();
                    })
                    .Append(CreatePlankDropSequence())

                    // 5단계: FIGHT 텍스트 효과
                    .AppendCallback(() => {
                        readyText.gameObject.SetActive(false);
                        fightText.gameObject.SetActive(true);
                        PlaySound(fightSound);
                        ShakeCamera();
                    })
                    .Append(fightText.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBounce))
                    .Join(fightText.DOColor(Color.red, 0.3f).SetLoops(6, LoopType.Yoyo))
                    .AppendInterval(fightStayDuration)

                    // 6단계: UI 정리 및 전투 시작
                    .Append(CreateCleanupSequence())
                    .OnComplete(StartBossBattle);

        // 시퀀스 실행
        introSequence.Play();
    }

    private Tween ShowOrangeText()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(orangeTextCanvasGroup.DOFade(1f, 0.3f))
                .Join(orangeTextRect.DOAnchorPosX(0f, orangeTextDuration).SetEase(orangeTextEase))
                .Join(orangeText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1f).SetDelay(orangeTextDuration * 0.7f));

        return sequence;
    }

    private Tween HideOrangeText()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(orangeText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
                .Join(orangeTextCanvasGroup.DOFade(0f, 0.5f))
                .Join(orangeTextRect.DOAnchorPosX(-Screen.width - 200f, 0.8f).SetEase(Ease.InCubic).SetDelay(0.2f));

        return sequence;
    }

    private Tween CreatePlankDropSequence()
    {
        var sequence = DOTween.Sequence();

        // 판자 하강 (2단계로 나누어 더 실감나게)
        sequence.Append(plankTransform.DOAnchorPosY(100f, plankDropDuration * 0.7f).SetEase(Ease.InQuad))
                .Append(plankTransform.DOAnchorPosY(-50f, plankDropDuration * 0.3f).SetEase(Ease.OutBounce))

                // 판자가 중앙을 지날 때 READY 텍스트 숨김
                .InsertCallback(plankDropDuration * 0.5f, () => {
                    readyText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                })

                // 판자 최종 위치에서 잠시 정지 후 아래로 완전히 사라짐
                .AppendInterval(0.3f)
                .Append(plankTransform.DOAnchorPosY(-Screen.height - 100f, 0.8f).SetEase(Ease.InCubic));

        return sequence;
    }

    private Tween CreateCleanupSequence()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(readyFightCanvasGroup.DOFade(0f, 0.8f).SetEase(Ease.InCubic))
                .Join(fightText.transform.DOScale(Vector3.zero, 0.8f).SetEase(Ease.InBack));

        return sequence;
    }

    private void ShakeCamera()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(cameraShakeDuration, cameraShakeIntensity, 10, 90f, false, true);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void StartBossBattle()
    {
        Debug.Log("보스 전투 시작!");

        // 추가 전투 시작 효과
        if (sparksEffect) sparksEffect.Stop();

        // 실제 보스 전투 로직 연결
        // SceneManager.LoadScene("BossBattleScene");
        // 또는 BossController.Instance.StartBattle();
    }

    // 연출 스킵 기능
    public void SkipIntro()
    {
        if (introSequence != null && introSequence.IsActive())
        {
            introSequence.Kill();

            // 즉시 최종 상태로 설정
            DOTween.Sequence()
                   .AppendCallback(() => {
                       orangeTextCanvasGroup.alpha = 0f;
                       fadeCanvasGroup.alpha = 0f;
                       readyFightCanvasGroup.alpha = 0f;
                   })
                   .AppendInterval(0.1f)
                   .OnComplete(StartBossBattle)
                   .Play();
        }
    }

    private void OnDestroy()
    {
        // DOTween 정리
        introSequence?.Kill();
        DOTween.KillAll();
    }
}
