using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;
public class ScreenScrollEffectManager : MonoBehaviour
{
    // 싱글톤 인스턴스 → 다른 스크립트에서 쉽게 접근 가능
    public static ScreenScrollEffectManager Instance { get; private set; }

    [Header("검은 패널의 RectTransform")]
    [SerializeField] private RectTransform blackPanel;
    // 화면을 가리거나 이동시킬 검은색 패널

    [Header("타자 효과용 텍스트")]
    [SerializeField] private TextMeshProUGUI typingText; // 타자 효과를 적용할 TextMeshPro 텍스트
    [SerializeField, TextArea] private string message;   // 출력할 메시지 내용

    [Header("스크롤 연출 관련 설정")]
    [SerializeField] private float scrollDuration = 1f; // 패널이 이동하는 시간
    [SerializeField] private float waitDuration = 1f;   // 중앙에 머무르는 시간
    [SerializeField] private float typingSpeed = 0.05f; // 글자가 출력되는 속도(타자 효과)

    [Header("패널 위치 좌표")]
    [SerializeField] private Vector2 offScreenRight = new Vector2(1920, 0); // 화면 오른쪽 바깥 위치
    [SerializeField] private Vector2 centerScreen = Vector2.zero;           // 화면 중앙 위치
    [SerializeField] private Vector2 offScreenLeft = new Vector2(-1920, 0); // 화면 왼쪽 바깥 위치

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 시작 시 검은 패널은 오른쪽 바깥에 배치
        blackPanel.anchoredPosition = offScreenRight;
    }

    /// <summary>
    /// 스크롤 연출을 실행하는 함수
    /// </summary>
    /// <param name="onComplete">애니메이션 완료 후 실행할 콜백</param>
    public void ShowScrollEffect(string customMessage, System.Action onComplete = null)
    {
        typingText.text = "";
        message = customMessage;

        Sequence seq = DOTween.Sequence();

        // 1. 오른쪽에서 중앙으로 이동
        seq.Append(blackPanel.DOAnchorPos(centerScreen, scrollDuration).SetEase(Ease.InOutSine));

        // 2. 중앙 도착 후 타자 효과
        seq.AppendCallback(() =>
        {
            StartCoroutine(PlayTypingEffect(() =>
            {
                // 3. 출력 완료 후 왼쪽으로 이동
                Sequence outSeq = DOTween.Sequence();
                outSeq.AppendInterval(waitDuration);
                outSeq.Append(blackPanel.DOAnchorPos(offScreenLeft, scrollDuration).SetEase(Ease.InOutSine));
                outSeq.OnComplete(() => onComplete?.Invoke());
            }));
        });
    }

    /// <summary>
    /// 텍스트를 한 글자씩 출력하는 타자 효과 코루틴
    /// </summary>
    /// <param name="onTyped">모든 글자 출력이 끝난 뒤 호출되는 콜백</param>
    private IEnumerator PlayTypingEffect(System.Action onTyped)
    {
        // 글자 수만큼 반복
        for (int i = 0; i <= message.Length; i++)
        {
            // 앞에서부터 i 글자만큼 잘라서 표시
            typingText.text = message.Substring(0, i);
            // 다음 글자 출력까지 대기
            yield return new WaitForSeconds(typingSpeed);
        }

        // 타자 효과 끝나면 콜백 호출
        onTyped?.Invoke();
    }
}
