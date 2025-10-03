using DG.Tweening;
using UnityEngine;

public class MoveUpEffect : MonoBehaviour, IMoveEffect
{
    public void Init(Transform target)
    {
    }

    private void OnEnable()
    {
        Vector3 v = transform.position + new Vector3 (0f,1f,0f);
        gameObject.transform.DOMove(v, 1f);
    }
}
