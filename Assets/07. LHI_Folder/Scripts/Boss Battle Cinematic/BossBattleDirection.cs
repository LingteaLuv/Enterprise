using DG.Tweening;
using TMPro;
using UnityEngine;

public class BossBattleDirection : MonoBehaviour
{
    [Header("레디 파이트 연출 컴포넌트")]
    [SerializeField] private TextMeshProUGUI readyText;
    private Vector3 readyTextPos; // readyText 초기 위치 저장
    [SerializeField] private TextMeshProUGUI fightText;

    [Header("승리 연출 컴포넌트")]
    [SerializeField] private TextMeshProUGUI victoryText;
    private Vector3 victoryTextScale = Vector3.one * 5;

    [Header("패배 연출 컴포넌트")]
    [SerializeField] private TextMeshProUGUI defeatText;
    private Vector3 defeatTextScale = Vector3.one * 5;
    private Vector3 defeatPos;

    [Header("공통 컴포넌트")]
    [SerializeField] private Transform plankTransform; // 판자 오브젝트
    private Vector3 plankPos; // 판자의 초기 위치 저장
    [SerializeField] private Transform playerTreasureChest;
    private Vector3 playerChestPos;
    [SerializeField] private Transform enemyTreasureChest;
    private Vector3 enemyChestPos;
    [SerializeField] public Transform playerShip;
    private Vector3 playerShipPos;
    [SerializeField] public Transform enemyShip;
    private Vector3 enemyShipPos;

    [Header("오디오")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip readySound;
    [SerializeField] private AudioClip fightSound;
    [SerializeField] private AudioClip plankDropSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip sparkleSound;

    [Header("카메라")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraShakeIntensity = 0.12f;
    [SerializeField] private float cameraShakeDuration = 0.03f;
    private float defaultOrthoSize;
    private Vector3 defaultPosition;

    // DOTween 시퀀스
    private Sequence readyFightSequence;

    private Sequence victorySequence;
    private Tween scaleLoopTween;

    private Sequence defeatSequence;

    private Sequence cameraZoomTween;


    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        InitialSetup();
    }

    private void InitialSetup()
    {
        // 초기 위치 및 세팅 저장
        readyTextPos = readyText.rectTransform.localPosition;
        plankPos = plankTransform.localPosition;

        playerChestPos = playerTreasureChest.position;
        playerShipPos = playerShip.position;
        enemyShipPos = enemyShip.position;

        enemyChestPos = enemyTreasureChest.position;
        playerShipPos = playerShip.position;
        enemyShipPos = enemyShip.position;

        // 원래 카메라 세팅 저장
        defaultOrthoSize = mainCamera.orthographicSize; // 기본 FOV 저장
        defaultPosition = mainCamera.transform.position; // 기본 위치 저장
    }

    private void Start()
    {
        InitializeUI();                 // UI 초기화
         // 시퀸스 저장
        ReadyFightDirectionSequence();  
        SetupVictorySequence();
        SetupDefeatSequence(); 
    }

    [ContextMenu("레디 파이트 연출 실행")]
    public void PlayReadyFightDirection() // 보스 전투를 시작할때 호출
    {
        readyFightSequence.Restart();
    }

    [ContextMenu("패배 연출 실행")]
    public void PlayDefeatDirection()
    {
        defeatSequence.Restart();
    }

    [ContextMenu("승리 연출 실행")]
    public void PlayVictoryDirection()
    {
        victorySequence.Restart();
    }

    [ContextMenu("초기화")]
    public void InitializeUI() // UI 초기화 외부에서 호출
    {
        audioSource.Stop();
        readyFightSequence?.Pause(); // 시퀀스 일시정지
        victorySequence?.Pause();
        defeatSequence?.Pause();

        scaleLoopTween?.Kill(); // 스케일 루프 트윈 종료
        scaleLoopTween = null;

        cameraZoomTween?.Kill(); // 카메라 줌 트윈 종료
        cameraZoomTween = null;

        readyText.rectTransform.localPosition = readyTextPos;
        readyText.alpha = 1f;

        fightText.alpha = 0f;

        victoryText.rectTransform.localScale = victoryTextScale;
        victoryText.alpha = 0f;

        playerShip.position = playerShipPos;
        playerShip.rotation = Quaternion.identity;

        enemyShip.position = enemyShipPos;
        enemyShip.rotation = Quaternion.identity;

        plankTransform.position = plankPos;
        plankTransform.rotation = Quaternion.identity;

        playerTreasureChest.position = playerChestPos;
        enemyTreasureChest.position = enemyChestPos;

        defeatText.rectTransform.localScale = defeatTextScale; 
        defeatText.alpha = 0f;

        mainCamera.orthographicSize = defaultOrthoSize;
        mainCamera.transform.position = defaultPosition;
    }

    #region ReadyFightDirection
    private void ReadyFightDirectionSequence()
    {
        // 새로운 DOTween 시퀀스 생성
        readyFightSequence = DOTween.Sequence();

        // 시퀀스 구성
        readyFightSequence.Append(readyText.DOFade(1f, 2f))
                        .Join(ReadyTextDrop())
                        .AppendCallback(() => PlankDrop())
                        .AppendInterval(0.65f) // 판자가 떨어지기 전에 약간의 딜레이 추가
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
    #endregion

    #region VictoryDirection
    public void SetupVictorySequence()
    {
        victorySequence = DOTween.Sequence();

        // 1. 빅토리 텍스트가 나타남, 텍스트가 계속 커졌다가 작아졌다를 반복, 보물 상자가 떨어짐
        // 2. 플레이어 배가 화면 중심으로 이동 (카메라를 줌인하여 연출), 동시에 적 배는 침몰, 판자는 기울어져 물에 빠짐, 첨벙 소리
        // 3. 상자가 열림, 열리는 효과음

        victorySequence.AppendCallback(() => PlaySound(victorySound))
                       .AppendCallback(() => victoryText.DOFade(1f, 2f).SetEase(Ease.InOutSine))
                       .AppendCallback(() => StartScaleLoop())
                       .AppendInterval(1f)

                       .AppendCallback(() => ZoomInLeft2D(playerShip))

                       .Join(playerTreasureChest.DOMoveY(playerTreasureChest.position.y - 7.5f, 1.5f).SetEase(Ease.OutBounce))

                       .Join(enemyShip.DORotate(new Vector3(0, 0, -90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(enemyShip.DOMoveY(enemyShip.position.y - 1f, 1.5f).SetEase(Ease.InOutSine))

                       .Join(plankTransform.DORotate(new Vector3(0, 0, -90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveY(plankTransform.position.y - 9.5f, 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveX(plankTransform.position.x + 2f, 1.5f).SetEase(Ease.InOutSine))

                       .AppendInterval(0.2f)
                       .AppendCallback(() => PlaySound(sparkleSound));

        victorySequence.Pause(); // 자동 재생 비활성화
        victorySequence.SetAutoKill(false); // 시퀀스가 끝나도 자동으로 제거되지 않도록 설정
    }

    private void StartScaleLoop()
    {
        scaleLoopTween?.Kill();

        scaleLoopTween = victoryText.rectTransform
            .DOScale(7f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    #endregion

    #region DefeatDirection
    public void SetupDefeatSequence()
    {
        defeatSequence = DOTween.Sequence();

        defeatSequence.AppendCallback(() => PlaySound(defeatSound))
                       .AppendCallback(() => defeatText.DOFade(1f, 2f).SetEase(Ease.InOutSine))
                       .AppendInterval(1f)

                       .AppendCallback(() => ZoomInLeft2D(enemyShip))

                       .Join(enemyTreasureChest.DOMoveY(enemyTreasureChest.position.y - 7.5f, 1.5f).SetEase(Ease.OutBounce))

                       .Join(playerShip.DORotate(new Vector3(0, 0, -90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(playerShip.DOMoveY(playerShip.position.y - 1f, 1.5f).SetEase(Ease.InOutSine))

                       .Join(plankTransform.DORotate(new Vector3(0, 0, 90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveY(plankTransform.position.y - 9.5f, 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveX(plankTransform.position.x - 2f, 1.5f).SetEase(Ease.InOutSine))

                       .AppendInterval(0.2f)
                       .AppendCallback(() => PlaySound(sparkleSound));

        defeatSequence.Pause();
        defeatSequence.SetAutoKill(false);
    }
    #endregion

    private void PlaySound(AudioClip clip) // 오디오 클립 재생
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ZoomInLeft2D(Transform ship)
    {
        cameraZoomTween.Kill();

        cameraZoomTween = DOTween.Sequence();

        cameraZoomTween.Append(mainCamera.DOOrthoSize(3f, 1f).SetEase(Ease.InOutSine)) // 줌인
            .Join(mainCamera.transform.DOMove(new Vector3(ship.position.x, ship.position.y, mainCamera.transform.position.z), 1f).SetEase(Ease.InOutSine));
    }

    private void OnDisable()
    {
        readyFightSequence?.Kill();
        readyFightSequence = null;

        victorySequence?.Kill();
        victorySequence = null;

        scaleLoopTween?.Kill();
        scaleLoopTween = null;

        defeatSequence?.Kill();
        defeatSequence = null;

        cameraZoomTween?.Kill();
        cameraZoomTween = null;
    }
}
