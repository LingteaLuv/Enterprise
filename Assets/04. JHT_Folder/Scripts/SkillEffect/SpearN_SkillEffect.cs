using UnityEngine;

public class SpearN_SkillEffect : SkillEffectAnim
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Init(Transform target)
    {
        base.Init(target);
        bool targetOnRight = target.transform.position.x > transform.position.x;

        transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 0f: 180f, 0f);
        gameObject.transform.position = target.position;
    }
}
