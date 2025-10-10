using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;
    private Button button;
    private bool isDragging;

    void Awake()
    {
        // 자신의 부모 계층에서 ScrollRect 컴포넌트를 찾습니다.
        scrollRect = GetComponentInParent<ScrollRect>();
        button = GetComponent<Button>();
    }

    // 드래그가 시작될 때 호출됩니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 버튼 클릭 이벤트 차단
        if (button != null)
            button.interactable = false;

        if (scrollRect != null)
        {
            // 이벤트를 ScrollRect의 OnBeginDrag 메소드로 전달합니다.
            scrollRect.OnBeginDrag(eventData);
        }
    }

    // 드래그 중일 때 매 프레임 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            // 이벤트를 ScrollRect의 OnDrag 메소드로 전달합니다.
            scrollRect.OnDrag(eventData);
        }
    }

    // 드래그가 끝났을 때 호출됩니다.
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 잠깐 비활성화 후 다시 활성화 (즉시 하면 Unity가 클릭으로 인식할 때가 있어서)
        if (button != null)
            StartCoroutine(EnableButtonNextFrame());

        if (scrollRect != null)
        {
            // 이벤트를 ScrollRect의 OnEndDrag 메소드로 전달합니다.
            scrollRect.OnEndDrag(eventData);
        }
    }

    private System.Collections.IEnumerator EnableButtonNextFrame()
    {
        yield return null;
        if (button != null)
            button.interactable = true;
    }
}