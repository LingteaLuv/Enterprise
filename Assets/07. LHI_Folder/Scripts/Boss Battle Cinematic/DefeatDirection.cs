using DG.Tweening;
using TMPro;
using UnityEngine;

public class DefeatDirection : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI defeatText;
    public Transform treasureChest;
    public Transform playerShip;
    public Transform enemyShip;
    public Transform plankTransform; // 판자 오브젝트

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    public AudioClip defeatSound;
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
    private Sequence defeatSequence;
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
        SetupDefeatSequence();
    }

    [ContextMenu("패배 연출 실행")]
    public void PlayDefeatDirection()
    {
        defeatSequence.Play();
    }

    [ContextMenu("초기화")]
    private void InitializeUI()
    {
        audioSource.Stop();
        defeatSequence?.Pause();

        cameraZoomTween?.Kill(); // 카메라 줌 트윈 종료
        cameraZoomTween = null;

        treasureChest.position = treasureChestPos;
        playerShip.position = playerShipPos;
        enemyShip.position = enemyShipPos;
        enemyShip.rotation = Quaternion.identity;

        plankTransform.position = plankPos;
        plankTransform.rotation = Quaternion.identity;

        defeatText.rectTransform.localScale = victoryTextScale;
        defeatText.alpha = 0f;

        mainCam.orthographicSize = defaultOrthoSize;
        mainCam.transform.position = defaultPosition; // 기본 위치로 복원
    }

    public void SetupDefeatSequence()
    {
        defeatSequence = DOTween.Sequence();

        defeatSequence.AppendCallback(() => PlaySound(defeatSound))
                       .AppendCallback(() => defeatText.DOFade(1f, 2f).SetEase(Ease.InOutSine))
                       .AppendInterval(1f)

                       .AppendCallback(() => ZoomInLeft2D())

                       .Join(treasureChest.DOMoveY(treasureChest.position.y - 6f, 1.5f).SetEase(Ease.OutBounce))

                       .Join(enemyShip.DORotate(new Vector3(0, 0, -90), 2f).SetEase(Ease.InOutSine))
                       .Join(enemyShip.DOMoveY(enemyShip.position.y - 1f, 2f).SetEase(Ease.InOutSine))

                       .Join(plankTransform.DORotate(new Vector3(0, 0, -90), 2f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveY(plankTransform.position.y - 1f, 2f).SetEase(Ease.InOutSine))
                       .Join(plankTransform.DOMoveX(plankTransform.position.x + 2f, 2f).SetEase(Ease.InOutSine))

                       .AppendInterval(1f)
                       .AppendCallback(() => PlaySound(sparkle));

        defeatSequence.Pause();
        defeatSequence.SetAutoKill(false);
    }

    public void ZoomInLeft2D()
    {
        cameraZoomTween.Kill();

        cameraZoomTween = DOTween.Sequence();

        cameraZoomTween.Append(mainCam.DOOrthoSize(3f, 1f).SetEase(Ease.InOutSine)) // 줌인
            .Join(mainCam.transform.DOMove(new Vector3(playerShip.position.x, playerShip.position.y, mainCam.transform.position.z), 1f).SetEase(Ease.InOutSine));
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDisable()
    {
        defeatSequence?.Kill();
        defeatSequence = null;

        cameraZoomTween?.Kill(); // 카메라 줌 트윈 종료
        cameraZoomTween = null;
    }
}
