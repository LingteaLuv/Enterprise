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

    [SerializeField] private SpriteRenderer playerWave;
    [SerializeField] private SpriteRenderer monsterWave;

    Vector2 playerShipPos;
    Vector2 monsterShipPos;

    private float waveTime;
    private Camera cam;
    
    CancellationTokenSource[] token;
    Sequence playerSequence, monsterSequence;

    private void Awake()
    {
        playerShipPos = playerShip.transform.position;
        monsterShipPos = monsterShip.transform.position;
    }

    private void OnEnable()
    {

        waveTime = 10;
    }

    private void OnDisable()
    {
        if (token != null)
        {
            for (int i = 0; i < token.Length; i++)
            {
                token[i].Cancel();
                token[i].Dispose();
                token[i] = null;
            }
        }
    }

    private void Start()
    {
        Init();
    }

    private void ProductInitalize()
    {

    }

    public void  Init()
    {
        cam = Camera.main;

        playerSequence = DOTween.Sequence();
        monsterSequence = DOTween.Sequence();

        //token = new CancellationTokenSource[1];

        //CameraSetting().Forget();
        
        
        playerWave.transform.DOMoveX(waveTime, 9f).SetEase(Ease.Linear).SetLoops(-1,LoopType.Yoyo);

        playerSequence.Append(playerShip.transform.DOLocalMoveX(-4f, 3));
        playerSequence.Append(playerShip.transform.DORotate(new Vector3(0, 0, 14), 1f));
        playerSequence.Append(playerShip.transform.DORotate(new Vector3(0, 0, 0), 0f));


        monsterWave.transform.DOMoveX(waveTime, 9f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        monsterSequence.Append(monsterShip.transform.DOLocalMoveX(2f, 3));
        monsterShip.transform.DORotate(new Vector3(0, 0, -14), 1f).SetDelay(2.5f);
        //monsterSequence.Append();
        monsterSequence.Append(monsterShip.transform.DORotate(new Vector3(0, 0, 0), 0f));

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
