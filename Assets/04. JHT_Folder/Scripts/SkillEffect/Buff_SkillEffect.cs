using UnityEngine;

public class Buff_SkillEffect : SkillEffectAnim
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Init(Transform target)
    {
        base.Init(target);

        gameObject.transform.position = target.position;
    }
}
