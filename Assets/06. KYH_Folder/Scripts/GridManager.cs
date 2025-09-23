using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 아이소매트릭 타일맵 기반 A* 경로 탐색용 Node 그리드 매니저
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private Tilemap tilemap;           // ← 외부 주입
    private Tilemap obstacleTilemap;   // ← 외부 주입

    [Header("그리드 크기 설정 (자동 생성됨)")]
    public int gridSizeX = 14;
    public int gridSizeY = 14;

    private Node[,] grid;
    private Vector3Int gridOffset;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 외부에서 타일맵 참조 설정
    /// </summary>
    public void SetTilemaps(Tilemap baseTilemap, Tilemap obstacleTilemap)
    {
        this.tilemap = baseTilemap;
        this.obstacleTilemap = obstacleTilemap;
    }

    /// <summary>
    /// 현재 설정된 타일맵 기준으로 그리드 생성
    /// </summary>
    public void CreateGrid()
    {
        if (tilemap == null)
        {
            Debug.LogError("[GridManager] tilemap이 설정되지 않았습니다.");
            return;
        }

        Debug.Log("[GridManager] 그리드 생성 시작");

        gridOffset = tilemap.cellBounds.min;
        gridSizeX = tilemap.cellBounds.size.x;
        gridSizeY = tilemap.cellBounds.size.y;

        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int cellPos = new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0);
                Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

                bool walkable = tilemap.HasTile(cellPos);
                if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos))
                    walkable = false;

                grid[x, y] = new Node(walkable, worldPos, x, y);
            }
        }

        Debug.Log("[GridManager] 그리드 생성 완료");
    }

    public Node GetNodeFromWorldPos(Vector3 worldPosition)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        int x = cellPos.x - gridOffset.x;
        int y = cellPos.y - gridOffset.y;

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return grid[x, y];

        return null;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1) continue;

                int checkX = node.gridX + dx;
                int checkY = node.gridY + dy;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public Vector2Int WorldToGridPos(Vector3 worldPosition)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPos.x - gridOffset.x, cellPos.y - gridOffset.y);
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        foreach (Node n in grid)
        {
            if (n == null) continue;
            Gizmos.color = n.walkable ? Color.white : Color.red;

            float w = tilemap.cellSize.x * 0.5f;
            float h = tilemap.cellSize.y * 0.5f;

            Vector3[] verts = new Vector3[4];
            verts[0] = n.worldPosition + new Vector3(0, h, 0);
            verts[1] = n.worldPosition + new Vector3(w, 0, 0);
            verts[2] = n.worldPosition + new Vector3(0, -h, 0);
            verts[3] = n.worldPosition + new Vector3(-w, 0, 0);

            Gizmos.DrawLine(verts[0], verts[1]);
            Gizmos.DrawLine(verts[1], verts[2]);
            Gizmos.DrawLine(verts[2], verts[3]);
            Gizmos.DrawLine(verts[3], verts[0]);
        }
    }
}
