using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ShiledFall_SkillEffect : SkillEffectAnim
{
    [SerializeField] private AnimationClip nextClip;
    [SerializeField] private GameObject fallObj;
    bool once = true;

    CancellationTokenSource fallToken;
    protected override void OnDisable()
    {
        base.OnDisable();

        if (fallToken != null)
        {
            fallToken.Cancel();
            fallToken.Dispose();
            fallToken = null;
        }
        
    }

    public override void Init(Transform target)
    {
        base.Init(target);


        gameObject.transform.position = target.position;
        fallObj.transform.position = new Vector2(target.position.x,target.position.y+1);
        if (once)
        {
            fallToken = new();
            fallObj.transform.DOMoveY(target.position.y-0.5f, clip.length / 2f);
            WaitForNextClip(target).Forget();
        }
        
    }

    async UniTask WaitForNextClip(Transform target)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length - 0.3f), cancellationToken: fallToken.Token);
            once = false;
            clip = nextClip;
            fallObj.gameObject.SetActive(false);
            this.Init(target);
        }
        catch (OperationCanceledException) { }
    }

    protected override async UniTaskVoid EffectTime(float spawnTime)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(spawnTime + nextClip.length - 0.4f), cancellationToken: base.token.Token);
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }
    }
}
