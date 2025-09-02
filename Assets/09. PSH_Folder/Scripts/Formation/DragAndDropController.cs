using UnityEngine;
using UnityEngine.UI;

// 드래그 앤 드롭 관리를 위한 싱글톤 컨트롤러
public class DragAndDropController : Singleton<DragAndDropController>
{
    [Header("UI 설정")]
    //[Tooltip("고스트 아이콘이 생성될 최상위 캔버스입니다.")]
    private Canvas mainCanvas;
    [SerializeField] private GameObject ghostIconPrefab; // 따라다니는 아이콘의 프리팹

    // 드래그 시작/종료 시 발생하는 이벤트
    public static event System.Action<PlayerCharacterData> OnDragStart;
    public static event System.Action OnDragEnd;

    private Image ghostIconImage;
    private GameObject ghostIconInstance;
    private RectTransform ghostIconRect;
    private PlayerCharacterData draggedCharacterData; // 현재 드래그 중인 캐릭터 데이터

    protected override void Awake()
    {
        base.Awake();
        if (mainCanvas == null)
        {
            Debug.Log("[DragAndDropController] 캔버스연결행애");
        }
    }

    private void InitializeGhostIcon()
    {
        if (ghostIconPrefab == null) return;
        if (ghostIconInstance != null) return;

        ghostIconInstance = Instantiate(ghostIconPrefab, mainCanvas.transform);
        ghostIconImage = ghostIconInstance.GetComponentInChildren<Image>();
        ghostIconRect = ghostIconInstance.GetComponent<RectTransform>();

        CanvasGroup canvasGroup = ghostIconInstance.GetComponent<CanvasGroup>() ?? ghostIconInstance.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        ghostIconInstance.SetActive(false);
    }

    public PlayerCharacterData GetDraggedData()
    {
        return draggedCharacterData;
    }

    public void StartDrag(PlayerCharacterData characterData, Vector2 screenPosition)
    {
        if (mainCanvas == null) { Debug.LogError("[DragAndDropController] Main Canvas가 할당되지 않았습니다!"); return; }
        InitializeGhostIcon();
        if (ghostIconInstance == null) return;

        draggedCharacterData = characterData;
        ghostIconImage.sprite = characterData.characterdata.characterSprite;
        ghostIconInstance.SetActive(true);
        ghostIconInstance.transform.SetAsLastSibling();

        UpdateDrag(screenPosition);

        OnDragStart?.Invoke(characterData);
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (ghostIconInstance == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            screenPosition,
            mainCanvas.worldCamera,
            out Vector2 localPoint);

        ghostIconRect.localPosition = localPoint;
    }

    /// <summary>
    /// 드롭 감지를 위해 고스트 아이콘만 즉시 숨깁니다.
    /// </summary>
    public void HideGhostIcon()
    {
        if (ghostIconInstance != null) ghostIconInstance.SetActive(false);
    }

    /// <summary>
    /// 드래그 데이터를 초기화하고 모든 프로세스를 종료합니다.
    /// </summary>
    public void EndDrag()
    {
        draggedCharacterData = null;
        OnDragEnd?.Invoke();
    }

    public void GetCanvas(Canvas canvas)
    {
        mainCanvas = canvas;
        Debug.Log("DragAndDropController : MainCanvcas 연결 완료");
    }
}
