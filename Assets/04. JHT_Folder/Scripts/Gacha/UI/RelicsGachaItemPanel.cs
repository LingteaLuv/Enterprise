using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicsGachaItemPanel : MonoBehaviour
{
    [Header("Stats")]
    private ItemRelicsSO curSO;
    private float curPower;
    private int curLevel;
    private ItemRarity curRarity;
    private PowerType curPowerType;

    [Header("UI")]
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI powerTypeText;
    [SerializeField] private TextMeshProUGUI levelText;

    public void Init(RelicsGachaManager relicsGachaManager)
    {
        curSO = relicsGachaManager.relicsResult;
        curRarity = relicsGachaManager.rarityResult;
        curLevel = relicsGachaManager.levelResult;

        curPowerType = relicsGachaManager.relicsResult.itemPowerType;

        curPower = curSO.startPower[(int)curRarity - 1] + curSO.upPower[(int)curRarity - 1] * curLevel;
        rarityImage.sprite = curSO.rarityImage[(int)curRarity - 1];

        powerText.text = curPower.ToString();
        powerTypeText.text = curPowerType.ToString();
        levelText.text = curLevel.ToString();
        itemImage.sprite = curSO.icon;

    }

}
