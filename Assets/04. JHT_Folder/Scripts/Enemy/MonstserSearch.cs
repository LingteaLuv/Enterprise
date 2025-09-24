using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonstserSearch : MonoBehaviour
{
    //private Coroutine repathRoutine;
    Coroutine moveRoutine;
    public Transform target;

    private List<Vector3Int> currentPath = new();
    //private Vector3Int lastTargetPos = new Vector3Int(int.MinValue, int.MinValue, 0);
    public void SearchTarget()
    {
        Vector3Int curtrans = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
        Vector3Int targetTrans = new Vector3Int(Mathf.RoundToInt(target.position.x),
            Mathf.RoundToInt(target.position.y), 0);

        currentPath = MonsterPathFinder.Instance.GetAstarPath(curtrans, targetTrans);

        if (currentPath != null)
        {
            if (moveRoutine == null)
                moveRoutine = StartCoroutine(FollowPath());
        }
    }

    //IEnumerator RepathLoop()
    //{
    //    while (target != null)
    //    {
    //        Vector3Int myPos = ChangeRoundToInt(transform.position);
    //        Vector3Int targetPos = ChangeRoundToInt(target.position);
            
    //        bool posChanged = false;

    //        if (targetPos != lastTargetPos)
    //            posChanged = true;
    //        else
    //            posChanged = false;


    //        if (posChanged)
    //        {
                
    //            List<Vector3Int> newPath = MonsterPathFinder.Instance.GetAstarPath(myPos, targetPos);

    //            if (newPath != null && newPath.Count > 0)
    //            {
    //                currentPath = newPath;
    //                lastTargetPos = targetPos;

    //                if (moveRoutine != null)
    //                    StopCoroutine(moveRoutine);

    //                moveRoutine = StartCoroutine(FollowPath());
    //            }
    //        }

    //        yield return new WaitForSeconds(0.2f);
    //    }

    //    repathRoutine = null;
    //}


    IEnumerator FollowPath()
    {
        foreach (var targetPos in currentPath)
        {
            var node = GridManager.Instance.grid[targetPos.x, targetPos.y];
            Vector3 fixedTargetPos = node.worldPosition;

            while (Vector2.Distance(transform.position, fixedTargetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, fixedTargetPos, 1 * Time.deltaTime);
                
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

                yield return null;
            }

        }

    }
    Vector3Int ChangeRoundToInt(Vector3 p)
    {
        return new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), 0);
    }

    public void StopRoutine()
    {
        //if (repathRoutine != null)
        //    StopCoroutine(repathRoutine);
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        //repathRoutine = null;

    }

    //private void OnDrawGizmos()
    //{
    //    if (currentPath == null || currentPath.Count == 0) return;

    //    Gizmos.color = Color.red;

    //    Vector3 prev = transform.position;
    //    for (int i = 0; i < currentPath.Count; i++)
    //    {
    //        var cell = currentPath[i];
    //        var node = GridManager.Instance.grid[cell.x, cell.y];
    //        var wp = node.worldPosition;

    //        Gizmos.DrawSphere(wp, 0.1f);
    //        Gizmos.DrawLine(prev, wp);

    //        prev = wp;
    //    }
    //}
}
