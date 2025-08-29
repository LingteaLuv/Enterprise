using UnityEngine;
using UnityEngine.EventSystems;

// 편성 슬롯에 붙여서 드롭 지점 역할을 합니다.
[RequireComponent(typeof(FormationSlotUI))]
public class FormationDropTarget : MonoBehaviour, IDropHandler
{
    [Header("UI 효과")]
    [Tooltip("배치 가능한 슬롯일 때 표시할 윤곽선/하이라이트 오브젝트")]
    public GameObject outlineEffect;

    private FormationSlotUI formationSlot;

    void Awake()
    {
        formationSlot = GetComponent<FormationSlotUI>();
        if (outlineEffect != null) outlineEffect.SetActive(false);
    }

    private void OnEnable()
    {
        // 드래그 컨트롤러의 이벤트에 구독
        DragAndDropController.OnDragStart += HandleDragStart;
        DragAndDropController.OnDragEnd += HandleDragEnd;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        DragAndDropController.OnDragStart -= HandleDragStart;
        DragAndDropController.OnDragEnd -= HandleDragEnd;
    }

    private void HandleDragStart(PlayerCharacterData draggedData)
    {
        if (outlineEffect == null) return;

        // 드래그된 캐릭터의 포지션이 이 슬롯의 포지션과 일치하는지 확인
        if (draggedData != null && draggedData.characterdata.crewRole == formationSlot.assignedPosition)
        {
            outlineEffect.SetActive(true);
        }
        else
        {
            outlineEffect.SetActive(false);
        }
    }

    private void HandleDragEnd()
    {
        if (outlineEffect != null) outlineEffect.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[DropTarget] OnDrop 감지! 슬롯: {formationSlot.assignedPosition}");

        PlayerCharacterData draggedData = DragAndDropController.Instance.GetDraggedData();
        if (draggedData == null) return;

        if (draggedData.characterdata.crewRole == formationSlot.assignedPosition)
        {
            PlayerDataManager.Instance.AddCharacterToFormation(draggedData);
        }
        else
        {
            string message = $"{draggedData.characterdata.characterName}은(는) {formationSlot.assignedPosition} 포지션이 아닙니다.";
            UIManager.Instance.ShowToast(message);
        }
    }
}
