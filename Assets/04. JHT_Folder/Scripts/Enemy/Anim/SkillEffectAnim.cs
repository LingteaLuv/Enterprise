using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public abstract class SkillEffectAnim : MonoBehaviour
{
    [SerializeField] private AnimationClip clip;
    [SerializeField] protected float spawnTime;
    [SerializeField] protected Rigidbody2D rigid = null;
    [SerializeField] protected float moveSpeed;

    private Animator animator;

    private AnimatorOverrideController aoc;
    public RuntimeAnimatorController rac;

    CancellationTokenSource token;
    public DG.Tweening.Sequence sequence;


    protected virtual void OnDisable()
    {
        if (token != null)
        {
            token.Cancel();
            token.Dispose();
            token = null;
        }

        sequence?.Kill();
        sequence = null;
    }

    public virtual void Init(Transform target)
    {
        sequence = DOTween.Sequence();
        token = new();
        animator = GetComponent<Animator>();

        if (animator == null || clip == null || target == null)
            return;

        //var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        //aoc.GetOverrides(overrides);
        //for (int i = 0; i < overrides.Count; i++)
        //{
        //    var original = overrides[i].Key;
        //    if (original != null && original.name == "Effect")
        //    {
        //        overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, clip);
        //        break;
        //    }
        //}
        //aoc.ApplyOverrides(overrides);

        aoc = new AnimatorOverrideController(rac);
        animator.runtimeAnimatorController = aoc;

        if (clip != null)
        {
            aoc["Effect"] = clip;
        }

        animator.Play("Effect");

        EffectTime(spawnTime).Forget();
    }

    protected async UniTaskVoid EffectTime(float spawnTime)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(spawnTime), cancellationToken: token.Token);
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }
    }

}
