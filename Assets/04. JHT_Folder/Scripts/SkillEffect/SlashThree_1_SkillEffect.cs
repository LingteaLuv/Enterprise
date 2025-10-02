using UnityEngine;

public class SlashThree_1_SkillEffect : SkillEffectAnim
{
    protected override void OnDisable()
    {
        base.OnDisable();
        Destroy(this.gameObject);
    }

    public override void Init(Transform target)
    {
        base.Init(target);

        transform.localRotation = Quaternion.Euler(0f, 0f, 20);
        gameObject.transform.position = target.position;
        
    }

}
