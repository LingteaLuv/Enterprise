using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MoveDownEffect : MonoBehaviour, IMoveEffect
{
    public void Init(Transform target)
    {
    }

    private void OnEnable()
    {
        transform.DOKill(); // 기존 트윈 종료
        StartCoroutine(StartMove());
    }
    private IEnumerator StartMove()
    {
        yield return null; // 한 프레임 대기 (위치 세팅 끝난 후)
        Vector3 v = transform.position;
        transform.position = transform.position + new Vector3(0f, 1f, 0f);
        gameObject.transform.DOMove(v, 1f);
    }
}
