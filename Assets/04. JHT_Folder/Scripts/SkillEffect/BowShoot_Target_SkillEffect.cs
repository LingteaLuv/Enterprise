using UnityEngine;

public class BowShoot_Target_SkillEffect : SkillEffectAnim
{
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Init(Transform target)
    {
        base.Init(target);
        bool targetOnRight = target.transform.position.x > transform.position.x;

        transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 180f : 0f, 0f);
        if(targetOnRight)
            gameObject.transform.position = new Vector2(target.position.x - 0.2f, target.position.y);
        else
            gameObject.transform.position = new Vector2(target.position.x + 0.2f, target.position.y);
    }
}
