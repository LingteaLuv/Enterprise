using UnityEngine;

public class BowShoot_owner_SkillEffect : SkillEffectAnim
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Init(Transform target)
    {
        base.Init(target);
        bool targetOnRight = target.transform.position.x > transform.position.x;

        if ((targetOnRight))
        {
            transform.localRotation = Quaternion.Euler(0, targetOnRight ? 180f : -180f, -90f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0, targetOnRight ? 180f : -180f, 90f);
        }
    }
}
