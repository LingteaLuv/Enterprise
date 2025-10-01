using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using DG;
using Unity.VisualScripting;

public class ShieldPush_SkillEffect : SkillEffectAnim
{
    [SerializeField] GameObject fadeTarget;

    public override void Init(Transform target)
    {
        base.Init(target);

        bool targetOnRight = target.transform.position.x > transform.position.x;

        transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 40f : -40f, 0f);

        //rigid.linearVelocity = ((Vector2)target.position - (Vector2)transform.position).normalized * moveSpeed;
        gameObject.transform.DOMove(target.position, moveSpeed).OnComplete(() => gameObject.transform.DOKill());

        fadeTarget.GetComponentInChildren<SpriteRenderer>().DOFade(0.3f, 1f);
    }

}
