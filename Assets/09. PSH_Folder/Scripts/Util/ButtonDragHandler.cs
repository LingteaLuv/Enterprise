using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;

    void Awake()
    {
        // 자신의 부모 계층에서 ScrollRect 컴포넌트를 찾습니다.
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    // 드래그가 시작될 때 호출됩니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
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
        if (scrollRect != null)
        {
            // 이벤트를 ScrollRect의 OnEndDrag 메소드로 전달합니다.
            scrollRect.OnEndDrag(eventData);
        }
    }
}