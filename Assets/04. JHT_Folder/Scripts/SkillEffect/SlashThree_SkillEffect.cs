using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SlashThree_SkillEffect : SkillEffectAnim
{
    [SerializeField] private AnimationClip nextClip;

    CancellationTokenSource nextToken;

    bool once;

    private void OnEnable()
    {
        nextToken = new();
        once = true;
    }

    protected override void OnDisable()
    {
        if (nextToken != null)
        {
            nextToken.Cancel();
            nextToken.Dispose();
            nextToken = null;
        }

        base.OnDisable();

    }

    public override void Init(Transform target)
    {
        base.Init(target);
        bool targetOnRight = target.transform.position.x > transform.position.x;

        transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 180f : 0f, 0f);
        gameObject.transform.position = target.position;

        if (once)
            NextClip(clip.length,target).Forget();
    }

    private async UniTaskVoid NextClip(float spawnTime,Transform target)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(spawnTime), cancellationToken: nextToken.Token);
        gameObject.transform.DORotate(new Vector3(0,0,20),0);
        clip = nextClip;
        once = false;

        Init(target);
    }


    protected override async UniTaskVoid EffectTime(float spawnTime)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length + nextClip.length), cancellationToken: token.Token);
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }
    }
}
