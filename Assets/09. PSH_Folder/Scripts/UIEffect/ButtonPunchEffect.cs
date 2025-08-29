using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 이 스크립트를 버튼이 있는 게임 오브젝트에 붙이면, 클릭 시 '움찔'하는 효과가 적용됩니다.
[RequireComponent(typeof(Button))]
public class ButtonPunchEffect : MonoBehaviour
{
    // 인스펙터에서 효과를 조절할 수 있도록 public 변수로 선언합니다.
    [Header("애니메이션 설정")]
    public float punchAmount = 0.1f;
    public float duration = 0.2f;
    public int vibrato = 1;
    public float elasticity = 1f;

    private Button button;
    private Vector3 punchVector;

    void Awake()
    {
        button = GetComponent<Button>();
        punchVector = new Vector3(punchAmount, punchAmount, punchAmount);

        // 버튼 클릭 이벤트에 애니메이션 실행 메소드를 등록합니다.
        button.onClick.AddListener(PlayPunchAnimation);
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 리스너를 제거하여 메모리 누수를 방지합니다.
        if (button != null)
        {
            button.onClick.RemoveListener(PlayPunchAnimation);
        }
    }

    private void PlayPunchAnimation()
    {
        // 진행중인 스케일 애니메이션이 있다면 중단하고, 새로 실행합니다.
        transform.DOKill(true); // true는 OnComplete까지 즉시 실행 후 중단을 의미합니다.
        transform.DOPunchScale(punchVector, duration, vibrato, elasticity);
    }
}
