using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;
public class ScreenScrollEffectManager : MonoBehaviour
{
    public static ScreenScrollEffectManager Instance { get; private set; }

    [Header("검은 패널의 RectTransform")]
    [SerializeField] private RectTransform blackPanel;

    [Header("타자 효과용 텍스트")]
    [SerializeField] private TextMeshProUGUI typingText;
    [SerializeField, TextArea] private string message; 

    [Header("스크롤 연출 관련 설정")]
    [SerializeField] private float scrollDuration = 1f;
    [SerializeField] private float waitDuration = 1f;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("패널 위치 좌표")]
    [SerializeField] private Vector2 offScreenRight = new Vector2(1920, 0);
    [SerializeField] private Vector2 centerScreen = Vector2.zero;
    [SerializeField] private Vector2 offScreenLeft = new Vector2(-1920, 0);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        blackPanel.anchoredPosition = offScreenRight;
    }

    public void ShowScrollEffect(System.Action onComplete = null)
    {
        // 초기화
        typingText.text = "";

        Sequence seq = DOTween.Sequence();

        // 1. 오른쪽 → 중앙으로 이동
        seq.Append(blackPanel.DOAnchorPos(centerScreen, scrollDuration).SetEase(Ease.InOutSine));

        // 2. 텍스트 타자 효과 → 코루틴으로 연결
        seq.AppendCallback(() =>
        {
            StartCoroutine(PlayTypingEffect(() =>
            {
                // 3. 텍스트 출력 완료 후 → 중앙에서 왼쪽으로 이동
                Sequence outSeq = DOTween.Sequence();
                outSeq.AppendInterval(waitDuration);
                outSeq.Append(blackPanel.DOAnchorPos(offScreenLeft, scrollDuration).SetEase(Ease.InOutSine));
                outSeq.OnComplete(() => onComplete?.Invoke());
            }));
        });
    }

    private IEnumerator PlayTypingEffect(System.Action onTyped)
    {
        for (int i = 0; i <= message.Length; i++)
        {
            typingText.text = message.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }

        onTyped?.Invoke();
    }
}
