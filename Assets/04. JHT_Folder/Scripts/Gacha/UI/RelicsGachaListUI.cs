using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JHT;
using UnityEngine;
using UnityEngine.UI;

public class RelicsGachaListUI : MonoBehaviour
{
    //[Header("Panel")]
    //[SerializeField] private RelicsGachaItemPanel relicsPanelItem;
    //[SerializeField] private Transform relicsItemParent;

    [Header("PopUp")]
    [SerializeField] private ChoosePopUp choosePopup;

    //[Header("Button")]
    //[SerializeField] private Button closeButton;

    [SerializeField] private RelicsProductAnim productAnim;
    [SerializeField] private Image fadeImg;
    Sequence sequenceFadeIn;
    Sequence sequenceFadeOut;

    CancellationTokenSource token;

    public void Init(RelicsGachaManager relicsGachaManager)
    {
        relicsGachaManager.OnChooseItem -= ShowChooseItem;
        relicsGachaManager.OnChooseItem += ShowChooseItem;

        if (token != null)
        {
            token.Cancel();
            token.Dispose();
            token = null;
        }
        else
        {
            token = new CancellationTokenSource();
        }
    }

    public void OnDisable()
    {
        sequenceFadeIn.Kill();
        sequenceFadeOut.Kill();

        if (token != null)
        {
            token.Cancel();
            token.Dispose();
            token = null;
        }
    }

    public async UniTask OutIt()
    {
        fadeImg.gameObject.SetActive(true);
        sequenceFadeIn = DOTween.Sequence().SetAutoKill(false).Append(fadeImg.DOFade(1.0f, 1)).Append(fadeImg.DOFade(0.0f, 2));
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token.Token);
        productAnim.gameObject.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token.Token);
        fadeImg.gameObject.SetActive(false);
    }

    private void ShowChooseItem(RelicsObject obj1, RelicsObject obj2)
    {
        ShowChooseItemAsync(obj1, obj2).Forget();
    }

    private async UniTask ShowChooseItemAsync(RelicsObject obj1, RelicsObject obj2)
    {
        fadeImg.gameObject.SetActive(true);
        sequenceFadeIn = DOTween.Sequence().SetAutoKill(false).Append(fadeImg.DOFade(1.0f, 1)).Append(fadeImg.DOFade(0.0f, 1));

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token.Token);
        choosePopup.gameObject.SetActive(true);
        choosePopup.Init(obj1, obj2);
        productAnim.gameObject.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token.Token);
        fadeImg.gameObject.SetActive(false);
    }

}
