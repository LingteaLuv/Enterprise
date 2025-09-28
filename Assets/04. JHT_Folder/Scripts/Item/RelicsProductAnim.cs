using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class RelicsProductAnim : MonoBehaviour
{
    [SerializeField] private Image waveImg;
    [SerializeField] private Image shipImg;

    [SerializeField] private Image lightImg;
    [SerializeField] private Image ChainImg;
    [SerializeField] private Mask chainMask;
    [SerializeField] private float waveTime;
    [SerializeField] private Image reward;

    [SerializeField] private Button panelButton;
    private Image fadeImg;

    Coroutine animStartCor;

    private CancellationTokenSource[] token = new CancellationTokenSource[3];
    RectTransform waveRT, shipRT, lightRT, chainRT, chainMaskRT, rewardRT;

    Vector2 baseWaveSize, baseWavePos, baseShipSize, baseShipPos;
    Quaternion baselightRot, baseShipRot;

    Sequence sequenceFadeInOut;


    RelicsGachaListUI inst;
    void Awake()
    {
        waveRT = waveImg.rectTransform;
        shipRT = shipImg.rectTransform;
        lightRT = lightImg.rectTransform;
        chainRT = ChainImg != null ? ChainImg.rectTransform : null;
        chainMaskRT = chainMask.rectTransform;
        rewardRT = reward.rectTransform;

        baseWaveSize = waveRT.sizeDelta;
        baseWavePos = waveRT.anchoredPosition;

        baseShipSize = shipRT.sizeDelta;
        baseShipPos = shipRT.anchoredPosition;

        baselightRot = lightRT.rotation;
        baseShipRot = shipRT.rotation;


        fadeImg = panelButton.GetComponent<Image>();
    }

    public void OnEnable()
    {
        for (int i = 0; i < token.Length; i++)
        {
            token[i] = new CancellationTokenSource();
        }
        sequenceFadeInOut = null;

        panelButton.interactable = false;

        StartAnim().Forget();

        panelButton.onClick.AddListener(Close); 

        chainMaskRT.anchoredPosition = new Vector2(0, -930);
        chainMaskRT.sizeDelta = new Vector2(1000, 1000);

        chainRT.sizeDelta = new Vector2(1000, 1000);
        chainRT.anchoredPosition = new Vector2(-75, 787);
        rewardRT.sizeDelta = new Vector2(500, 500);
        rewardRT.anchoredPosition = new Vector2(0, -700);

        waveRT.sizeDelta = baseWaveSize;
        waveRT.anchoredPosition = baseWavePos;

        shipRT.sizeDelta = baseShipSize;
        shipRT.anchoredPosition = baseShipPos;

        lightRT.rotation = baselightRot;
        shipRT.rotation = baseShipRot;

        inst = gameObject.GetComponentInParent<RelicsGachaListUI>();
    }
    private void OnDisable()
    {
        waveRT.DOKill();
        shipRT.DOKill();
        lightRT.DOKill();
        chainRT.DOKill();
        rewardRT.DOKill();

        panelButton.onClick.RemoveListener(Close);

        if (animStartCor != null)
        {
            StopCoroutine(animStartCor);
            animStartCor = null;
        }

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


    private async UniTaskVoid StartAnim()
    {
        panelButton.interactable = true;
        try
        {
            waveRT.DOAnchorPosX(waveTime, 9f);//.SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
            waveRT.DOSizeDelta(new Vector2(8000, 600), 5f);
            //waveRT.DOScale(1.3f, 8f).SetEase(Ease.InOutQuad);

            shipRT.DOLocalRotate(new Vector3(0, 0, -5f), 5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
            shipRT.DOAnchorPosY(10f, 5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            shipRT.DOSizeDelta(new Vector2(2000, 2000), 5f);

            lightRT.DOLocalRotate(new Vector3(0, 0, -5f), 5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);

            await UniTask.Delay(TimeSpan.FromSeconds(4.5f), cancellationToken: token[0].Token);
            chainMask.gameObject.SetActive(true);

            await ShowRelics();
        }
        catch (OperationCanceledException) { }
    }
    

    private async UniTask ShowRelics()
    {
        try
        {
            chainRT.DOAnchorPosY(190, 2f);
            chainRT.DOAnchorPosY(787, 2f).SetDelay(2f);

            reward.gameObject.SetActive(true);
            rewardRT.DOAnchorPosY(542, 2f).SetDelay(1f);
            rewardRT.DOShakeRotation(20, 4, 30);


            await UniTask.Delay(TimeSpan.FromSeconds(7f), cancellationToken: token[1].Token);

            await inst.OutIt();
        }
        catch (OperationCanceledException) { }
    }

    private void Close()
    {
        CloseProduct().Forget();
    }

    private async UniTask CloseProduct()
    {
        inst.OutIt().Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token[2].Token);
        gameObject.SetActive(false);
    }

}
