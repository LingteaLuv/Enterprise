using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A* 알고리즘을 이용한 경로 탐색기 (Z축 고정)
/// - GridManager를 통해 시작/목표 노드를 받아 경로를 탐색함
/// - 경로가 없으면 null 반환
/// </summary>
public class AStarPathfinding : MonoBehaviour
{
    public static AStarPathfinding Instance;

    private void Awake()
    {
        // 싱글톤 패턴
        Instance = this;
    }

    /// <summary>
    /// 월드 좌표 기준으로 A* 경로를 계산해 Vector3 리스트 반환 (Z=0 고정)
    /// </summary>
    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        // 시작/목표 위치에 해당하는 노드를 GridManager에서 가져옴
        Node startNode = GridManager.Instance.GetNodeFromWorldPos(startWorldPos);
        Node targetNode = GridManager.Instance.GetNodeFromWorldPos(targetWorldPos);

        // 디버깅용 (그리드 좌표 확인)
        Vector2Int startGrid = GridManager.Instance.WorldToGridPos(startWorldPos);
        // Debug.Log($"[FindPath] 시작 위치: {startWorldPos}, 변환된 그리드 좌표: {startGrid}");

        // 시작 또는 목표 노드가 없을 경우 경로 실패 처리
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

        // A* 알고리즘을 위한 open/closed 집합 초기화
        List<Node> openSet = new();        // 아직 평가되지 않은 노드
        HashSet<Node> closedSet = new();   // 이미 평가한 노드

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // FCost가 가장 낮은 노드를 openSet에서 선택
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

            // 도착 노드에 도달하면 경로 역추적 시작
            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            // 인접 노드 평가
            foreach (Node neighbor in GridManager.Instance.GetNeighbours(currentNode))
            {
                if (!neighbor.walkable) // 이동 불가능한 노드는 스킵
                {
                    // Debug.Log($"[A*] Blocked Node at {neighbor.gridX},{neighbor.gridY} → walkable = false");
                    continue;
                }

                if (closedSet.Contains(neighbor))
                    continue;

                // 현재까지 이동 거리 + 인접 노드 거리
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    // 더 나은 경로 발견 → 값 갱신
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // 경로를 찾지 못한 경우
        Debug.LogWarning("경로 없음");
        return null;
    }

    /// <summary>
    /// 탐색이 완료된 경로를 시작점부터 역으로 추적
    /// - Node에서 worldPosition을 꺼내어 Vector3 리스트 반환
    /// - Z축은 항상 0으로 고정
    /// </summary>
    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            Vector3 pos = currentNode.worldPosition;
            pos.z = 0f; // Z축 고정 (2D 환경 고려)
            path.Add(pos);

            currentNode = currentNode.parent;
        }

        path.Reverse(); // 시작점부터 순서대로 정렬
        return path;
    }

    /// <summary>
    /// Manhattan 거리 계산
    /// - 대각선 이동 없이 직선 거리만 고려 (가중치: 1)
    /// </summary>
    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);
        return dstX + dstY;
    }
}
