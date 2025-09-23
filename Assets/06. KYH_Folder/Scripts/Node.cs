using UnityEngine;
/// <summary>
/// A* 경로 탐색용 하나의 타일 노드 정보
/// </summary>
public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node parent;

    public int FCost => gCost + hCost;

    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        // z값을 0으로 고정 (경로 비교 시 안전하게 하기 위해)
        this.worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);
        this.gridX = gridX;
        this.gridY = gridY;
        nodeVec = new Vector3Int(gridX, gridY);
    }

    public Vector3Int nodeVec;
    public Vector3Int targetPos;
}
