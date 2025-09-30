using DG.Tweening;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    private void OnEnable()
    {
        Vector3 v = transform.position + new Vector3 (0f,3f,0f);
        gameObject.transform.DOMove(v, 1f);
    }
}
