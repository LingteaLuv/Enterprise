using UnityEngine;

/// <summary>
/// 가로/세로 커스텀 배치, 그룹 정렬, 줄 정렬을 지원하며 각 자식의 Pivot을 보존하는 스크립트입니다.
/// </summary>
[ExecuteAlways]
public class CustomGridLayout : MonoBehaviour
{
    public enum LayoutDirection { Horizontal, Vertical }
    public enum LineAlignment { Start, Center, End }

    [Header("레이아웃 설정")]
    [Tooltip("아이템을 가로로 배치할지, 세로로 배치할지 선택합니다.")]
    public LayoutDirection direction = LayoutDirection.Vertical;

    [Tooltip("가장 긴 줄/열을 기준으로 각 라인의 아이템을 정렬합니다. (Start: 좌/상, Center: 중앙, End: 우/하)")]
    public LineAlignment lineAlignment = LineAlignment.Center;

    [Tooltip("전체 아이템 그룹을 부모의 어느 위치에 둘지 정합니다.")]
    public TextAnchor childAlignment = TextAnchor.MiddleCenter;

    [Header("배치 설정")]
    [Tooltip("가로 모드에서는 '줄 당 아이템 수', 세로 모드에서는 '열 당 아이템 수'를 의미합니다.")]
    public int[] itemsInLine = { 3, 4, 3 };

    [Header("셀 설정")]
    public Vector2 cellSize = new Vector2(260, 410);
    public Vector2 spacing = new Vector2(100, 100);

    [Header("여백")]
    public RectOffset padding;

    private void OnEnable() { RepositionChildren(); }
    private void OnValidate() { RepositionChildren(); }
    private void OnTransformChildrenChanged() { RepositionChildren(); }
    private void OnRectTransformDimensionsChange() { RepositionChildren(); }

    public void RepositionChildren()
    {
        if (direction == LayoutDirection.Horizontal) RepositionHorizontal();
        else RepositionVertical();
    }

    private void RepositionHorizontal()
    {
        if (transform.childCount == 0) return;
        RectTransform parentRect = GetComponent<RectTransform>();
        parentRect.pivot = new Vector2(0, 1);

        int maxItemsInRow = 0;
        foreach (int items in itemsInLine) { if (items > maxItemsInRow) maxItemsInRow = items; }

        float contentWidth = (maxItemsInRow * cellSize.x) + (Mathf.Max(0, maxItemsInRow - 1) * spacing.x) + padding.left + padding.right;
        float contentHeight = (itemsInLine.Length * cellSize.y) + (Mathf.Max(0, itemsInLine.Length - 1) * spacing.y) + padding.top + padding.bottom;

        Vector2 startOffset = CalculateStartOffset(parentRect, contentWidth, contentHeight);

        int childIndex = 0;
        float currentY = startOffset.y - padding.top;

        for (int i = 0; i < itemsInLine.Length; i++)
        {
            float currentLineWidth = (itemsInLine[i] * cellSize.x) + (Mathf.Max(0, itemsInLine[i] - 1) * spacing.x);
            float maxLineWidth = (maxItemsInRow * cellSize.x) + (Mathf.Max(0, maxItemsInRow - 1) * spacing.x);
            float lineOffset = CalculateLineOffset(maxLineWidth, currentLineWidth);

            float currentX = startOffset.x + padding.left + lineOffset;
            for (int j = 0; j < itemsInLine[i]; j++)
            {
                if (childIndex >= transform.childCount) break;
                PositionChild(childIndex, new Vector2(currentX, currentY));
                currentX += cellSize.x + spacing.x;
                childIndex++;
            }
            currentY -= cellSize.y + spacing.y;
            if (childIndex >= transform.childCount) break;
        }
    }

    private void RepositionVertical()
    {
        if (transform.childCount == 0) return;
        RectTransform parentRect = GetComponent<RectTransform>();
        parentRect.pivot = new Vector2(0, 1);

        int maxItemsInColumn = 0;
        foreach (int items in itemsInLine) { if (items > maxItemsInColumn) maxItemsInColumn = items; }

        float contentWidth = (itemsInLine.Length * cellSize.x) + (Mathf.Max(0, itemsInLine.Length - 1) * spacing.x) + padding.left + padding.right;
        float contentHeight = (maxItemsInColumn * cellSize.y) + (Mathf.Max(0, maxItemsInColumn - 1) * spacing.y) + padding.top + padding.bottom;

        Vector2 startOffset = CalculateStartOffset(parentRect, contentWidth, contentHeight);

        int childIndex = 0;
        float currentX = startOffset.x + padding.left;

        for (int i = 0; i < itemsInLine.Length; i++)
        {
            float currentColumnHeight = (itemsInLine[i] * cellSize.y) + (Mathf.Max(0, itemsInLine[i] - 1) * spacing.y);
            float maxColumnHeight = (maxItemsInColumn * cellSize.y) + (Mathf.Max(0, maxItemsInColumn - 1) * spacing.y);
            float lineOffset = CalculateLineOffset(maxColumnHeight, currentColumnHeight);

            float currentY = startOffset.y - padding.top - lineOffset;
            for (int j = 0; j < itemsInLine[i]; j++)
            {
                if (childIndex >= transform.childCount) break;
                PositionChild(childIndex, new Vector2(currentX, currentY));
                currentY -= cellSize.y + spacing.y;
                childIndex++;
            }
            currentX += cellSize.x + spacing.x;
            if (childIndex >= transform.childCount) break;
        }
    }

    private float CalculateLineOffset(float maxLineSize, float currentLineSize)
    {
        switch (lineAlignment)
        {
            case LineAlignment.Center:
                return (maxLineSize - currentLineSize) / 2.0f;
            case LineAlignment.End:
                return maxLineSize - currentLineSize;
            default: // Start
                return 0;
        }
    }

    private Vector2 CalculateStartOffset(RectTransform parentRect, float contentWidth, float contentHeight)
    {
        float startX = 0, startY = 0;
        switch (childAlignment)
        {
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                startX = (parentRect.rect.width - contentWidth) / 2.0f;
                break;
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                startX = parentRect.rect.width - contentWidth;
                break;
        }
        switch (childAlignment)
        {
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                startY = -((parentRect.rect.height - contentHeight) / 2.0f);
                break;
            case TextAnchor.LowerLeft:
            case TextAnchor.LowerCenter:
            case TextAnchor.LowerRight:
                startY = -(parentRect.rect.height - contentHeight);
                break;
        }
        return new Vector2(startX, startY);
    }

    // ✨ 여기가 수정된 핵심 부분이에요! ✨
    private void PositionChild(int childIndex, Vector2 position)
    {
        RectTransform child = transform.GetChild(childIndex).GetComponent<RectTransform>();
        if (child == null) return;

        // 자식의 원래 Pivot값을 가져옵니다.
        Vector2 childPivot = child.pivot;

        // 앵커와 사이즈를 먼저 설정합니다.
        child.anchorMin = new Vector2(0, 1);
        child.anchorMax = new Vector2(0, 1);
        child.sizeDelta = cellSize;

        // 자식의 Pivot을 고려해서 최종 위치를 계산하고 적용합니다.
        float finalX = position.x + (cellSize.x * childPivot.x);
        float finalY = position.y - (cellSize.y * (1 - childPivot.y));
        child.anchoredPosition = new Vector2(finalX, finalY);
    }

    [ContextMenu("Reset Values")]
    public void ResetValues()
    {
        direction = LayoutDirection.Horizontal;
        lineAlignment = LineAlignment.Start;
        childAlignment = TextAnchor.UpperLeft;
        itemsInLine = new int[] { 3, 4, 3 };
        cellSize = new Vector2(100, 100);
        spacing = new Vector2(10, 10);
        padding = new RectOffset(0, 0, 0, 0);
        Debug.Log("CustomGridLayout의 값이 초기화되었어요! ✨");
        RepositionChildren();
    }
}