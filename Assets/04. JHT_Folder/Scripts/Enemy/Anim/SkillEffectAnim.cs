using UnityEngine;

public class SkillEffectAnim : MonoBehaviour
{
    [SerializeField] private AnimationClip[] clip;

    private Animator animator;

    //private AnimatorOverrideController aoc;
    //public RuntimeAnimatorController rac;
    public Transform target;

    private void Start()
    {
        Init(Faction.Pirate, target);
    }

    public void Init(Faction faction,Transform target = null)
    {
        animator = GetComponent<Animator>();

        if (animator == null || clip == null)
            return;

        bool targetOnRight = target.transform.position.x > transform.position.x;

        transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 40f : -40f, 0f);

        //aoc = new AnimatorOverrideController(rac);
        //animator.runtimeAnimatorController = aoc;

        SetHexCode(faction, target);
        animator.Play("Effect");
    }

    public void SetHexCode(Faction faction, Transform target = null)
    {
        //switch (faction)
        //{
        //    case Faction.Pirate:
        //        if (clip[0] != null)
        //        {
        //            animator.["Effect"] = clip[0];
        //        }
        //        break;
        //    case Faction.Monster:
        //        if (clip[1] != null)
        //        {
        //            aoc["Effect"] = clip[1];
        //        }
        //        break;
        //    case Faction.Marine:
        //        if (clip[2] != null)
        //        {
        //            aoc["Effect"] = clip[2];
        //        }
        //        break;
        //}
    }
}
