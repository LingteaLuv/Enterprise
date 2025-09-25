using System.Collections.Generic;
using JHT;
using UnityEngine;
using UnityEngine.Rendering;

public class MonsterPathFinder : Singleton<MonsterPathFinder>
{
    protected override void Awake()
    {
        base.Awake();
        targetNodeList = new();
    }

    Vector3Int[] directions =
    {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,-1,0),
            new Vector3Int(1,1,0),
            new Vector3Int(1,-1,0),
            new Vector3Int(-1,1,0),
            new Vector3Int(-1,-1,0),
    };

    List<Node> targetNodeList;
    int monsterCount;

    public void DataSetting()
    {
        targetNodeList = new();
        monsterCount = JHT_MonsterSpawnManager.Instance.curMonsterCountList.Count;

        if (monsterCount <= 0)
            targetNodeList.Clear();
    }

    public List<Vector3Int> GetAstarPath(Vector3Int _myPos, Vector3Int _targetPos)
    {
        var openSet = new PriorityQueue<Node>();                    // 탐색 예정인 노드들
        var parent = new Dictionary<Vector3Int, Node>();            // 현재노드의 이전노드(부모노드)
        var gScore = new Dictionary<Node, float>();                 // 시작 - 목표지점 까지의 실제 거리
        var fScore = new Dictionary<Node, float>();                 // 휴리스틱값과 gScore값을 더해서 우선순위 기준으로 값을 매기기위해 존재

        Node startNode = GridManager.Instance.GetNodeFromWorldPos(_myPos);
        Node targetNode = GridManager.Instance.GetNodeFromWorldPos(_targetPos);
        //Node targetNode = ArrayTargetPos(target);

        if (startNode == null)
        {
            return null;
        }

        if (targetNode == null)
        {
            return null;
        }
        openSet.Enqueue(startNode, 0);

        gScore[startNode] = 0;
        fScore[startNode] = gScore[startNode] + Heuristic(startNode.nodeVec, targetNode.nodeVec);
        
        HashSet<Node> closedSet = new();
        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == targetNode)
                return ReconstructPath(parent, targetNode);

            closedSet.Add(current);

            foreach (var d in directions)
            {
                var next = current.nodeVec + d;

                Node nextNode = GetNextNode(current, d);

                if (nextNode == null)
                    continue;

                if (!IsValid(nextNode.nodeVec))
                {
                    Debug.Log("다음 노드는 갈 수 없음");
                    continue;
                }

                if (closedSet.Contains(nextNode))
                {
                    continue;
                }

                float nextCost = d.x == 0 || d.y == 0 ? 1 : (float)1.4;

                float tentativeG = gScore[current] + nextCost;

                if (!gScore.ContainsKey(nextNode) || tentativeG < gScore[nextNode])
                {
                    parent[next] = current;
                    gScore[nextNode] = tentativeG;
                    fScore[nextNode] = tentativeG + Heuristic(next, targetNode.nodeVec);
                    openSet.Enqueue(nextNode, fScore[nextNode]);
                }
            }
        }

        return null;

    }

    public float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Node> parent, Node targetPos)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        var current = targetPos;

        while (parent.ContainsKey(current.nodeVec))
        {
            path.Add(current.nodeVec);
            current = parent[current.nodeVec];
        }

        path.Add(current.nodeVec);
        path.Reverse();

        return path;
    }

    private bool IsValid(Vector3Int pos)
    {
        Node[,] grid = GridManager.Instance.grid;
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
            
        return pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h && grid[pos.x, pos.y].walkable;
    }

    private Node GetNextNode(Node curNode, Vector3Int dir)
    {
        Vector3Int next = new Vector3Int(curNode.nodeVec.x + dir.x, curNode.nodeVec.y + dir.y, 0);

        if (!IsValid(next))
            return null;

        return GridManager.Instance.grid[next.x, next.y];
    }


    private Node ArrayTargetPos(Node targetNode)
    {
        if (targetNode == null) return null;

        Node[,] grid = GridManager.Instance.grid;

        foreach (var d in directions)
        {
            Vector3Int np = targetNode.nodeVec + d;

            if (!IsValid(np))
                continue;
            targetNodeList.Add(grid[(targetNode.nodeVec + d).x, (targetNode.nodeVec + d).y]);
        }

        Node randomNode = targetNodeList[UnityEngine.Random.Range(0, directions.Length)];
        targetNodeList.Remove(randomNode);
        return randomNode;
    }

}
