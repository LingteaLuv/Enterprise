using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일맵 기반으로 A* 경로 탐색용 Node[,] 배열을 생성 및 관리
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Vector2 cellSize = new Vector2(1f, 0.5f);
    public int gridSizeX = 14;
    public int gridSizeY = 14;
    private Vector3 originPosition;

    [Header("타일맵 참조")]
    [SerializeField] private Tilemap tilemap; // 타일맵을 인스펙터에서 Drag & Drop

    private Node[,] grid;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        originPosition = new Vector3(-gridSizeX * 0.5f * cellSize.x,
                                 -gridSizeY * 0.5f * cellSize.y,
                                 0f);
        CreateGrid();
    }

    public Vector2Int WorldToGridPos(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - originPosition.x) / cellSize.x);
        int y = Mathf.RoundToInt((worldPosition.y - originPosition.y) / cellSize.y);
        return new Vector2Int(x, y);
    }

    public void CreateGrid()
    {
        Debug.Log("그리드 생성 호출됨");
        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = new Vector3(x * cellSize.x, y * cellSize.y, 0f) + originPosition;

                // 타일맵 기준 셀 좌표 얻기
                Vector3Int cellPos = tilemap.WorldToCell(worldPoint);

                // 타일이 존재하는지 여부 체크
                bool walkable = tilemap.HasTile(cellPos);

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node GetNodeFromWorldPos(Vector3 worldPosition)
    {
        // z값 제거
        worldPosition.z = 0f;

        int x = Mathf.RoundToInt((worldPosition.x - originPosition.x) / cellSize.x);
        int y = Mathf.RoundToInt((worldPosition.y - originPosition.y) / cellSize.y);

        Debug.Log($"월드 좌표: {worldPosition}, 계산된 그리드 좌표: ({x},{y})");

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return grid[x, y];

        return null;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (Mathf.Abs(x) + Mathf.Abs(y) > 1) continue; // 대각선 제외

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

   // 디버그용 경로 기즈모
 // private void OnDrawGizmos()
 // {
 //     if (grid != null)
 //     {
 //         foreach (Node n in grid)
 //         {
 //             Gizmos.color = n.walkable ? Color.white : Color.red;
 //             Vector3 gizmoSize = new Vector3(cellSize.x - 0.05f, cellSize.y - 0.05f, 0.1f);
 //             Gizmos.DrawCube(n.worldPosition, gizmoSize);
 //         }
 //     }
 // }
}
