using DG.Tweening;
using UnityEngine;
using TMPro;

public class VictoryDirection : MonoBehaviour
{
    [Header("오디오")]
    public AudioClip victoryFanfare; // 승리 팡파레
    public AudioClip shipSinking; // 배 침몰 소리
    private AudioSource audioSource;

    [Header("승리")]
    public GameObject victoryUI; // 승리 UI

    [Header("배")]
    public GameObject ship; // 배 오브젝트
    public GameObject enemyShip; // 적군 배 오브젝트

    [Header("보물 상자")]
    public GameObject treasureChest; // 보물 상자 오브젝트

    private Sequence victorySequence;

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        InitializeUI(); // UI 초기화
        StartVictorySequence(); // 시퀀스 저장
    }

    [ContextMenu("OnVictory")]
    private void OnVictory()
    {
        victorySequence.Restart();
    }

    [ContextMenu("초기화")]
    public void InitializeUI() // UI 초기화 외부에서 호출
    {
        audioSource.Stop();
        victorySequence?.Pause(); // 시퀀스 일시정지

        victoryUI.SetActive(false); // 승리 UI 비활성화

        // 보물 상자가 y축으로만 이동해서 화면 밖으로 나가도록 설정
        Vector3 chestStartPos = treasureChest.transform.position;
        chestStartPos.y += 6f; // y축으로 6만큼 위로 이동
        treasureChest.transform.position = chestStartPos;



        // 초기 상태 설정
    }

    private void StartVictorySequence()
    {
        // 연출 순서 : 승리 팡파레 - 보물 상자 떨어짐(아군배) -> 배 침몰(적군배) - 판자 떨어짐 - 배침몰 사운드 ->
        // 아군배가 화면중심으로 이동 - 카메라 줌인 - 승리 UI 등장

        victorySequence = DOTween.Sequence();
        victorySequence.AppendCallback(() => PlaySound(victoryFanfare)) // 승리 팡파레
                       .Append(treasureChest.transform.DOMoveY(0.45f, 2f).SetEase(Ease.OutBounce)) // 보물 상자 떨어짐

                       .AppendCallback(() => PlaySound(shipSinking)) // 배 침몰 소리
                       .AppendInterval(3f) // 침몰 소리 후 대기
                       .AppendCallback(() => treasureChest.SetActive(true)) // 보물 상자 등장
                       .AppendInterval(1f) // 보물 상자 등장 후 대기
                       .AppendCallback(() => victoryUI.SetActive(true)); // 승리 UI 등장

        victorySequence.Pause(); // 자동 재생 비활성화 설정에 따라 일시정지 상태로 시작
        victorySequence.SetAutoKill(false); // 시퀀스가 끝나도 자동으로 제거되지 않도록 설정
    }

    // 배 및 판자 침몰 애니메이션
    private Tween sinking()
    {

        return null;
    }    

    private void PlaySound(AudioClip clip) // 오디오 클립 재생
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
