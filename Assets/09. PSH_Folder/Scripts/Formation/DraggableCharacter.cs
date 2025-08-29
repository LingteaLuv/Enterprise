using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // 코루틴 사용을 위해 추가

// 캐릭터 패널에 붙여서 드래그 및 클릭 기능을 담당합니다.
[RequireComponent(typeof(CharacterPanelUI))]
public class DraggableCharacter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{
    [Header("설정")]
    [SerializeField] private float holdDuration = 0.4f; // 꾹 누르는 시간

    private CharacterPanelUI characterPanel;
    private Coroutine holdCoroutine;
    private bool isDragging = false;
    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;

    void Awake()
    {
        characterPanel = GetComponent<CharacterPanelUI>();
    }

    void Update()
    {
        if (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (characterPanel.ownerScrollView == null || !characterPanel.ownerScrollView.isFormationMode) return;

        isPointerDown = true;
        pointerDownTimer = 0f;
        holdCoroutine = StartCoroutine(HoldToDrag(eventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        if (isDragging)
        {
            StartCoroutine(EndDragSequence());
        }
    }

    private IEnumerator EndDragSequence()
    {
        // 1. 먼저 고스트 아이콘을 숨겨서 드롭 타겟이 감지될 수 있도록 함
        DragAndDropController.Instance.HideGhostIcon();
        isDragging = false;

        // 2. 이벤트 시스템이 OnDrop을 처리할 수 있도록 한 프레임 대기
        yield return null;

        // 3. 드래그 데이터 정리
        DragAndDropController.Instance.EndDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            DragAndDropController.Instance.UpdateDrag(eventData.position);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && pointerDownTimer < holdDuration)
        {
            HandleTapToAssign();
        }
    }

    private void HandleTapToAssign()
    {
        if (characterPanel.ownerScrollView == null || !characterPanel.ownerScrollView.isFormationMode) return;

        PlayerCharacterData character = characterPanel.currentPlayerCharData;
        bool isInFormation = PlayerDataManager.Instance.IsInFormation(character);

        if (isInFormation)
        {
            PlayerDataManager.Instance.RemoveCharacterFromFormation(character);
        }
        else
        {
            int result = PlayerDataManager.Instance.AddCharacterToFormation(character);
        }
    }

    private System.Collections.IEnumerator HoldToDrag(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdDuration);

        isDragging = true;
        holdCoroutine = null;
        DragAndDropController.Instance.StartDrag(characterPanel.currentPlayerCharData, eventData.position);
    }
}

