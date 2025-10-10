using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BossBattleProduct : MonoBehaviour
{
    [SerializeField] private GameObject playerShip;
    [SerializeField] private GameObject monsterShip;
    [SerializeField] private GameObject wave;

    [SerializeField] private float crashPos;

    Vector2 playerShipPos;
    Vector2 monsterShipPos;
    private bool canBattleStart;

    private Camera cam;
    
    CancellationTokenSource[] token;

    private Tween waveTween;
    Sequence playerSequence, monsterSequence;
    Sequence loseSequence;
    private void Awake()
    {
        playerShipPos = playerShip.transform.position;
        monsterShipPos = monsterShip.transform.position;
    }

    private void OnEnable()
    {
        token = new CancellationTokenSource[2];
    }

    private void OnDisable()
    {

        if (token != null)
        {
            for (int i = 0; i < token.Length; i++)
            {
                if (token[i] != null)
                {
                    token[i].Cancel();
                    token[i].Dispose();
                    token[i] = null;
                }
            }
        }
    }


    public void  Init()
    {
        canBattleStart = false;

        cam = Camera.main;
        BossBattleStart().Forget();
        // 기존 트윈 정리(중복 방지)
        playerSequence?.Kill();
        monsterSequence?.Kill();
        waveTween?.Kill();

        // 파도(좌우 왕복)
        waveTween = wave.transform.DOLocalMoveX(-1.8f, 3f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);

        // 공통 타이밍
        float approachDur = 3.0f;     // 서로 접근 시간
        float hitTime = approachDur; // 충돌 시점(두 트윈이 동시에 끝나는 시점)
        float tiltDur = 0.25f;    // 충돌 시 기울이기
        float tiltBackDur = 0.4f;    // 원래 각도로 복귀
        float retreatDur = 2.5f;     // 후퇴 시간

        // 플레이어 배 시퀀스
        playerSequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
        loseSequence = DOTween.Sequence();
        // 1) 전진 (왼쪽 → 충돌지점으로)
        playerSequence.Append(playerShip.transform.DOLocalMoveX(-crashPos, approachDur)
            .SetEase(Ease.InOutSine));

        // 2) 충돌 순간에 살짝 기울임
        playerSequence.Insert(hitTime - tiltDur,
            playerShip.transform.DORotate(new Vector3(0, 0, 14f), tiltDur)
                .SetEase(Ease.OutQuad));

        // 3) 원래 각도로 복귀
        playerSequence.Insert(hitTime,
            playerShip.transform.DORotate(Vector3.zero, tiltBackDur)
                .SetEase(Ease.OutBack));


        // 5) 후퇴(원위치로 복귀)
        playerSequence.Append(playerShip.transform.DOLocalMoveX(-3.7f, retreatDur)
            .SetEase(Ease.InOutSine));

        // 몬스터 배 시퀀스(우측 → 충돌지점)
        monsterSequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);

        monsterSequence.Append(monsterShip.transform.DOLocalMoveX(crashPos, approachDur)
            .SetEase(Ease.InOutSine));

        // 우측 배는 반대 방향으로 약간 기울이기(-Z)
        monsterSequence.Insert(hitTime - tiltDur,
            monsterShip.transform.DORotate(new Vector3(0, 0, -14f), tiltDur)
                .SetEase(Ease.OutQuad));

        monsterSequence.Insert(hitTime,
            monsterShip.transform.DORotate(Vector3.zero, tiltBackDur)
                .SetEase(Ease.OutBack));


        monsterSequence.Append(monsterShip.transform.DOLocalMoveX(3.7f, retreatDur)
            .SetEase(Ease.InOutSine));

        //SetBattleStart(canBattleStart);
        // 동시에 시작하고 싶다면 별도 마스터 시퀀스로 묶기 (선택)
        //DOTween.Sequence()
        //    .AppendCallback(() => { playerSequence.Restart(); monsterSequence.Restart();});
        int done = 0;
        void MarkDone() { if (++done == 2) canBattleStart = true; }

        playerSequence.OnComplete(MarkDone);
        monsterSequence.OnComplete(MarkDone);
    }

    private async UniTask BossBattleStart()
    {
        BossBattleManager.Instance.direction.Init();

        while (!canBattleStart)
        {
            await UniTask.Yield();
        }

        BossBattleManager.Instance.direction.PlayReadyFightDirection();

    }
    private bool isEnding; // 중복 호출 방지

    public void LoseProduct(bool isWin)
    {
        if (isEnding) return;
        isEnding = true;

        // 진행 중 트윈 정리 (충돌 방지)
        playerSequence?.Kill();
        monsterSequence?.Kill();
        waveTween?.Kill();
        loseSequence?.Kill();

        loseSequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);

        // 침몰 대상/파라미터 결정
        Transform targetShip = isWin ? monsterShip.transform : playerShip.transform;
        float tiltZ = isWin ? -60f : 60f;   // 기울이는 방향
        float sinkDistance = 6f;            // 내려갈 거리(월드 Y 기준)
        float tiltDur = 0.35f;
        float sinkDur = 1.8f;
        float fadeDur = 1.2f;

        // 자식 스프라이트 페이드용(없으면 무시)
        var rends = targetShip.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        // 1) 살짝 기울이기
        loseSequence.Append(targetShip.DORotate(new Vector3(0f, 0f, tiltZ), tiltDur)
            .SetEase(Ease.OutCubic));

        // 2) 아래로 침몰(월드 좌표 기준으로 자연스럽게)
        float targetY = targetShip.position.y - sinkDistance;
        loseSequence.Append(targetShip.DOMoveY(targetY, sinkDur)
            .SetEase(Ease.InQuad));

        // 3) 동시에 페이드아웃(스프라이트가 있다면)
        foreach (var r in rends)
            loseSequence.Join(r.DOFade(0f, fadeDur));

        // 4) 끝난 뒤 후 처리(예: UI 연출 트리거, 오브젝트 비활성 등)
        loseSequence.OnComplete(() =>
        {
            // 필요 시: 패배 배 비활성화
            // targetShip.gameObject.SetActive(false);

            // 다음 연출 호출(예시)
            // BossBattleManager.Instance.direction.PlayLoseDirection();

            // 플래그 해제/정리
            canBattleStart = false;
        });
    }
    private async UniTask CameraSetting()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token[0].Token);
        float t = 10;
        while (t >= 10)
        {
            t -= Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 5, t);
        }
    }

}
