using UnityEngine;
using DG.Tweening;

public class TestMoveEffect : MonoBehaviour, IMoveEffect
{
    private Transform _target;
    private Vector3 _startPos;
    public GameObject obj;

    public void Init( Transform target)
    {
        //Debug.Log($"[{gameObject.name}] Init 호출됨. Caster: {caster.name}, Target: {target.name}");
        //_startPos = caster.position;
        //_target = target;
        //transform.position = _startPos;

        //Debug.Log($"[{gameObject.name}] 애니메이션 시작! {_startPos}에서 {_target.position}으로 이동합니다.");
        //// caster에서 target으로 이동하는 애니메이션
        //transform.DOMove(_target.position, 1f).SetEase(Ease.Linear)
        //    .OnComplete(() => Debug.Log($"[{gameObject.name}] 애니메이션 완료!"));

    }
}
