using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 아이소매트릭 타일맵 기반 A* 경로 탐색용 Node 그리드 매니저
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("타일맵 설정")]
    [SerializeField] private Tilemap tilemap;         // 기본 타일맵 (지형)
    [SerializeField] private Tilemap ObstacleTilemap; // 장애물 타일맵

    [Header("그리드 크기 설정")]
    public int gridSizeX = 14;
    public int gridSizeY = 14;

    private Node[,] grid;
    private Vector3Int gridOffset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateGrid();
    }

    /// <summary>
    /// 그리드 생성: 각 셀에 대해 타일 존재 여부와 장애물 여부 검사
    /// </summary>
    public void CreateGrid()
    {
        Debug.Log("[GridManager] 그리드 생성 시작");

        gridOffset = tilemap.cellBounds.min; // ← 셀 오프셋 적용
        gridSizeX = tilemap.cellBounds.size.x;
        gridSizeY = tilemap.cellBounds.size.y;

        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int cellPos = new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0);
                Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos); // 셀 중심 위치

                bool walkable = tilemap.HasTile(cellPos);

                if (ObstacleTilemap != null && ObstacleTilemap.HasTile(cellPos))
                    walkable = false;

                grid[x, y] = new Node(walkable, worldPos, x, y);
            }
        }

        Debug.Log("[GridManager] 그리드 생성 완료");
    }



    /// <summary>
    /// 월드 좌표에서 Node 반환 (isometric 대응)
    /// </summary>
    public Node GetNodeFromWorldPos(Vector3 worldPosition)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        int x = cellPos.x - gridOffset.x;
        int y = cellPos.y - gridOffset.y;

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return grid[x, y];

        return null;
    }

    /// <summary>
    /// 이웃 노드 반환 (상하좌우만, 대각선 제외)
    /// </summary>
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1) continue; // 대각선 제외

                int checkX = node.gridX + dx;
                int checkY = node.gridY + dy;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// 월드 좌표를 타일맵 그리드 좌표(Vector2Int)로 변환
    /// </summary>
    public Vector2Int WorldToGridPos(Vector3 worldPosition)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPos.x - gridOffset.x, cellPos.y - gridOffset.y);
    }

    public void SetTilemaps(Tilemap baseTilemap, Tilemap obstacleTilemap)
    {
        this.tilemap = baseTilemap;
        this.ObstacleTilemap = obstacleTilemap;
    }

    /// <summary>
    /// 디버그용 기즈모로 노드 표시
    /// </summary>
    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                if (n == null) continue;

                Gizmos.color = n.walkable ? Color.white : Color.red;

                Vector3[] verts = new Vector3[4];
                float w = tilemap.cellSize.x * 0.5f;
                float h = tilemap.cellSize.y * 0.5f;

                verts[0] = n.worldPosition + new Vector3(0, h, 0);   // 위
                verts[1] = n.worldPosition + new Vector3(w, 0, 0);   // 오른쪽
                verts[2] = n.worldPosition + new Vector3(0, -h, 0);  // 아래
                verts[3] = n.worldPosition + new Vector3(-w, 0, 0);  // 왼쪽

                Gizmos.DrawLine(verts[0], verts[1]);
                Gizmos.DrawLine(verts[1], verts[2]);
                Gizmos.DrawLine(verts[2], verts[3]);
                Gizmos.DrawLine(verts[3], verts[0]);
            }
        }
    }
}
