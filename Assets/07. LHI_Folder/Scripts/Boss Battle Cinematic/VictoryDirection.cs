using DG.Tweening;
using TMPro;
using UnityEngine;


public class VictoryDirection : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI victoryText;
    public Transform treasureChest;
    public Transform playerShip;
    public Transform enemyShip;
    public Transform plankTransform; // 판자 오브젝트

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    public AudioClip victorySound;
    public AudioClip sparkle;

    [Header("Camera")]
    public Camera mainCam;

    // 카메라 기본 설정 저장
    private float defaultOrthoSize;
    private Vector3 defaultPosition;

    // 초기 위치 저장
    private Vector3 treasureChestPos;
    private Vector3 playerShipPos;
    private Vector3 enemyShipPos;
    private Vector3 plankPos;
    private Vector3 victoryTextScale = Vector3.one * 5;

    // DOTween 시퀀스
    private Sequence victorySequence;
    private Tween scaleLoopTween;
    private Sequence cameraZoomTween;

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        treasureChestPos = treasureChest.position;
        playerShipPos = playerShip.position;
        enemyShipPos = enemyShip.position;
        plankPos = plankTransform.position;

        // 원래 카메라 세팅 저장
        defaultOrthoSize = mainCam.orthographicSize; // 기본 FOV 저장
        defaultPosition = mainCam.transform.position; // 기본 위치 저장
    }
    private void Start()
    {
        InitializeUI();
        SetupVictorySequence();
    }

    [ContextMenu("승리 연출 실행")]
    public void PlayVictoryDirection()
    {
        victorySequence.Restart();
    }

    [ContextMenu("초기화")]
    private void InitializeUI()
    {
        audioSource.Stop();
        victorySequence?.Pause(); // 시퀀스 일시정지

        scaleLoopTween?.Kill(); // 스케일 루프 트윈 종료
        scaleLoopTween = null;

        cameraZoomTween?.Kill(); // 카메라 줌 트윈 종료
        cameraZoomTween = null;

        treasureChest.position = treasureChestPos;
        playerShip.position = playerShipPos;
        enemyShip.position = enemyShipPos;
        enemyShip.rotation = Quaternion.identity;

        plankTransform.position = plankPos;
        plankTransform.rotation = Quaternion.identity;

        victoryText.rectTransform.localScale = victoryTextScale;
        victoryText.alpha = 0f;

        mainCam.orthographicSize = defaultOrthoSize; 
        mainCam.transform.position = defaultPosition; // 기본 위치로 복원
    }


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

                       .AppendCallback(() => ZoomInLeft2D())

                       .Join(treasureChest.DOMoveY(treasureChest.position.y -6f, 1.5f).SetEase(Ease.OutBounce))
                       
                       .Join(enemyShip.DORotate(new Vector3(0, 0, -90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(enemyShip.DOMoveY(enemyShip.position.y - 1f, 1.5f).SetEase(Ease.InOutSine))

                       .Join(plankTransform.DORotate(new Vector3(0, 0, -90), 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveY(plankTransform.position.y - 1f, 1.5f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveX(plankTransform.position.x + 2f, 1.5f).SetEase(Ease.InOutSine))

                       .AppendInterval(1f)
                       .AppendCallback(() => PlaySound(sparkle));

        victorySequence.Pause(); // 자동 재생 비활성화
        victorySequence.SetAutoKill(false); // 시퀀스가 끝나도 자동으로 제거되지 않도록 설정
    }

    public void ZoomInLeft2D()
    {
        cameraZoomTween.Kill();

        cameraZoomTween = DOTween.Sequence();

        cameraZoomTween.Append(mainCam.DOOrthoSize(3f, 1f).SetEase(Ease.InOutSine)) // 줌인
            .Join(mainCam.transform.DOMove(new Vector3(playerShip.position.x, playerShip.position.y, mainCam.transform.position.z),1f).SetEase(Ease.InOutSine));
    }

    private void StartScaleLoop()
    {
        scaleLoopTween?.Kill();

        scaleLoopTween = victoryText.rectTransform
            .DOScale(7f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
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
        victorySequence?.Kill();
        victorySequence = null;

        scaleLoopTween?.Kill();
        scaleLoopTween = null;

        cameraZoomTween?.Kill(); // 카메라 줌 트윈 종료
        cameraZoomTween = null;
    }
}
