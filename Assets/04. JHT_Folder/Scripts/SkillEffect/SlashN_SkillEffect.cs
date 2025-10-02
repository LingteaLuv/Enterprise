using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class SlashN_SkillEffect : SkillEffectAnim
{
    [SerializeField] private AnimationClip[] nextClip;

    bool once;
    bool second;

    float clip1 = 0f;
    float clip2 = 0f;

    CancellationTokenSource nextToken;

    private void OnEnable()
    {
        once = true;
        second = true;

        clip1 = clip.length;
        clip2 = nextClip[0].length;
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


        if (second)
        {
            if (once)
            {
                nextToken = new();
                NextClip(nextClip[0], clip1, target).Forget();
            }
            else
            {
                NextClip(nextClip[1], clip2, target).Forget();
            }
        }
    }


    private async UniTaskVoid NextClip(AnimationClip nextClip, float duration, Transform target)
    {
        try
        {
            if (once)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: nextToken.Token);
                once = false;
                this.clip = nextClip;
                Init(target);
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: nextToken.Token);
                gameObject.transform.DORotate(new Vector3(0, 0, 20), 0);
                second = false;
                this.clip = nextClip;
                Init(target);
            }
        }
        catch (OperationCanceledException) { }
    }


    protected override async UniTaskVoid EffectTime(float spawnTime)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length + nextClip[0].length + nextClip[1].length + 0.5f), cancellationToken: token.Token);
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }
    }
}
