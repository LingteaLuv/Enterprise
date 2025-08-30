using JHT;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosePopUp : MonoBehaviour
{
    [SerializeField] private Button item1Button;
    [SerializeField] private Button item2Button;
    [SerializeField] private Button selectButton;

    [Header("Item1 information")]
    [SerializeField] private Image item1;
    [SerializeField] private Image item1RarityImg;
    [SerializeField] private Image item1RarityColor;
    [SerializeField] private GameObject item1SelectOutLine;
    [SerializeField] private TextMeshProUGUI item1RarityText;
    [SerializeField] private TextMeshProUGUI item1PowerTypeText;
    [SerializeField] private TextMeshProUGUI item1PowerText;
    [SerializeField] private TextMeshProUGUI item1Name;

    [Header("Item2 information")]
    [SerializeField] private Image item2;
    [SerializeField] private Image item2RarityImg;
    [SerializeField] private Image item2RarityColor;
    [SerializeField] private GameObject item2SelectOutLine;
    [SerializeField] private TextMeshProUGUI item2RarityText;
    [SerializeField] private TextMeshProUGUI item2PowerTypeText;
    [SerializeField] private TextMeshProUGUI item2PowerText;
    [SerializeField] private TextMeshProUGUI compareText;
    [SerializeField] private TextMeshProUGUI item2Name;


    private RelicsObject relicsObj1;
    private RelicsObject relicsObj2;

    private float compare;
    public GameObject upImage;
    public GameObject downImage;

    private bool selectMyItem;
    public bool SelectMyItem { get { return selectMyItem; } set { selectMyItem = value; OnSelect?.Invoke(selectMyItem); } }
    private event Action<bool> OnSelect;
    public void Init(RelicsObject obj1, RelicsObject obj2)
    {
        relicsObj1 = obj1;
        relicsObj2 = obj2;

        Item1Setting();
        Item2Setting();

        item1Button.onClick.AddListener(ClickButton1);
        item2Button.onClick.AddListener(ClickButton2);
        selectButton.onClick.AddListener(SelectItem);

        OnSelect += ChooseItem;

        CompareObj(obj1, obj2);
    }

    public void OnDisable()
    {
        item1Button.onClick.RemoveListener(ClickButton1);
        item2Button.onClick.RemoveListener(ClickButton2);
    }

    // 아이템 1 세팅
    private void Item1Setting()
    {
        item1.sprite = relicsObj1.itemSO.icon;
        item1RarityImg.sprite = relicsObj1.itemRarityImage;

        item1PowerText.text = $"{relicsObj1.itemPower}";
        item1PowerTypeText.text = $"{relicsObj1.itemPowerType.ToString()} : ";

        item1RarityText.text = $"{relicsObj1.curRarity}";
        item1RarityColor.color = SetItemRarityColor(relicsObj1);

        item1Name.text = relicsObj1.itemName;
    }

    // 아이템 2 세팅
    private void Item2Setting()
    {
        item2.sprite = relicsObj2.itemSO.icon;
        item2RarityImg.sprite = relicsObj2.itemRarityImage;

        item2PowerText.text = $"{relicsObj2.itemPower}";

        item2PowerTypeText.text = $"{relicsObj2.itemPowerType.ToString()} : ";

        item2RarityText.text = $"{relicsObj2.curRarity}";

        item2RarityColor.color = SetItemRarityColor(relicsObj2);
        item2Name.text = relicsObj2.itemName;

    }

    private void ClickButton1()
    {
        SelectMyItem = true;
    }

    private void ClickButton2()
    {
        SelectMyItem = false;
    }

    private void SelectItem()
    {
        InventoryManager.Instance.OnChangeItem?.Invoke(relicsObj1, relicsObj2, selectMyItem);
        gameObject.SetActive(false);
    }

    private Color SetItemRarityColor(RelicsObject obj)
    {
        switch (obj.curRarity)
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

    private void CompareObj(RelicsObject obj1, RelicsObject obj2)
    {
        compare = obj1.itemPower - obj2.itemPower;
        if (compare > 0)
        {
            downImage.SetActive(true);
            upImage.SetActive(false);
        }
        else if (compare < 0)
        {
            upImage.SetActive(true);
            downImage.SetActive(false);
        }
        else
        {
            upImage.SetActive(false);
            downImage.SetActive(false);
        }

        compareText.text = $"{Mathf.Abs(compare)}";
    }

    private void ChooseItem(bool value)
    {
        item1SelectOutLine.SetActive(value);
        item2SelectOutLine.SetActive(!value);
    }
}
