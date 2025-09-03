using JHT;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RelicsProbabilityItemPanel : MonoBehaviour
{
    [SerializeField] private Image rarityPanel;
    [SerializeField] private TextMeshProUGUI curWeightMap;
    [SerializeField] private TextMeshProUGUI nextWeightMap;
    [SerializeField] private TextMeshProUGUI weightDif;

    [SerializeField] private Image difUpImage;
    [SerializeField] private Image difLowImage;

    [SerializeField] private Image hahaha;

    int upgradeGauge = 0;

    Tween sway;
    public void Init(RelicsGachaManager manager,int curTableLevel,int curIndex)
    {
        if (manager.relicsGachaTableManager.relicsGachaTables.Length >= manager.relicsTablelevel)
        {

            ShowTableStat(manager, curTableLevel, curIndex);
        }
        else
        {
            ShowMaxLevelTableStat(manager, curTableLevel, curIndex);
        }
        //StartCoroutine(ImageEffect());
    }

    private void ShowTableStat(RelicsGachaManager manager,int curTableLevel,int curIndex)
    {
        rarityPanel.color = SetItemRarityColor(manager.relicsGachaTableManager.relicsGachaTables[curTableLevel]._items[curIndex].rarity);

        curWeightMap.text = manager.relicsGachaTableManager.relicsGachaTables[curTableLevel]._items[curIndex].weight.ToString();

        nextWeightMap.text = manager.relicsGachaTableManager.relicsGachaTables[curTableLevel + 1]._items[curIndex].weight.ToString();

        upgradeGauge = manager.relicsGachaTableManager.relicsGachaTables[curTableLevel]._items[curIndex].weight -
                manager.relicsGachaTableManager.relicsGachaTables[curTableLevel + 1]._items[curIndex].weight;

        if (upgradeGauge < 0)
        {
            difLowImage.gameObject.SetActive(true);
            difUpImage.gameObject.SetActive(false);
            weightDif.text = Mathf.Abs(upgradeGauge).ToString();
        }
        else if (upgradeGauge > 0)
        {
            difLowImage.gameObject.SetActive(false);
            difUpImage.gameObject.SetActive(true);
            weightDif.text = upgradeGauge.ToString();
        }
        else
        {
            difLowImage.gameObject.SetActive(false);
            difUpImage.gameObject.SetActive(false);
        }
    }


    private void ShowMaxLevelTableStat(RelicsGachaManager manager,int curTableLevel,int curIndex)
    {
        curWeightMap.text = manager.relicsGachaTableManager.relicsGachaTables[curTableLevel]._items[curIndex].weight.ToString();
        nextWeightMap.gameObject.SetActive(false);

        upgradeGauge = 0;

        weightDif.gameObject.SetActive(false);
        
    }

    private Color SetItemRarityColor(ItemRarity obj)
    {
        switch (obj)
        {
            case ItemRarity.Normal:
                return Color.white;
            case ItemRarity.Rare:
                return Color.red;
            case ItemRarity.Epic:
                return Color.cyan;
            case ItemRarity.Unique:
                return Color.yellow;
            case ItemRarity.Legend:
                return Color.green;
            default:
                return Color.white;

        }
    }

    private IEnumerator ImageEffect()
    {
        float target = hahaha.rectTransform.anchoredPosition.x + 50f;

        if(gameObject.activeSelf)
        sway = hahaha.rectTransform.DOAnchorPosX(target,0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        
        sway.Kill();
        yield return null;
    }
}
