using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A* 알고리즘을 이용한 경로 탐색기 (Z축 고정)
/// </summary>
public class AStarPathfinding : MonoBehaviour
{
    public static AStarPathfinding Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 월드 좌표 기준으로 A* 경로를 계산해 Vector3 리스트 반환 (Z=0 고정)
    /// </summary>
    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        Node startNode = GridManager.Instance.GetNodeFromWorldPos(startWorldPos);
        Node targetNode = GridManager.Instance.GetNodeFromWorldPos(targetWorldPos);

        Vector2Int startGrid = GridManager.Instance.WorldToGridPos(startWorldPos);
        Debug.Log($"[FindPath] 시작 위치: {startWorldPos}, 변환된 그리드 좌표: {startGrid}");

        if (startNode == null)
        {
            Debug.LogWarning("경로 탐색 실패: 시작 노드 없음");
            return null;
        }

        if (targetNode == null)
        {
            Debug.LogWarning("경로 탐색 실패: 도착 노드 없음");
            return null;
        }

        List<Node> openSet = new();
        HashSet<Node> closedSet = new();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode); // << Z=0 고정된 Vector3 경로 반환

            foreach (Node neighbor in GridManager.Instance.GetNeighbours(currentNode))
            {
                if (!neighbor.walkable)
                {
                    Debug.Log($"[A*] Blocked Node at {neighbor.gridX},{neighbor.gridY} → walkable = false");
                    continue;
                }

                if (closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("경로 없음");
        return null;
    }

    /// <summary>
    /// 경로 역추적 (Z축은 항상 0으로 고정)
    /// </summary>
    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            Vector3 pos = currentNode.worldPosition;
            pos.z = 0f; // Z축 고정
            path.Add(pos);

            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        return dstX + dstY;
    }
}
