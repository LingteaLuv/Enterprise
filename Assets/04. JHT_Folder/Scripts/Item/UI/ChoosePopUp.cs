using JHT;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosePopUp : MonoBehaviour
{
    [SerializeField] private GameObject relicsGachaListPanel;
    [SerializeField] private Button item1Button;
    [SerializeField] private Button item2Button;
    [SerializeField] private Button selectButton;

    [Header("Item1 information")]
    [SerializeField] private GameObject item1Panel;
    [SerializeField] private Image item1;
    [SerializeField] private Image item1RarityImg;
    [SerializeField] private Image item1RarityColor;
    [SerializeField] private Image item1SelectOutLine;
    [SerializeField] private TextMeshProUGUI item1RarityText;
    [SerializeField] private TextMeshProUGUI item1PowerTypeText;
    [SerializeField] private TextMeshProUGUI item1PowerText;
    [SerializeField] private TextMeshProUGUI item1Name;
    private float item1Power;

    [Header("Item2 information")]
    [SerializeField] private Image item2;
    [SerializeField] private Image item2RarityImg;
    [SerializeField] private Image item2RarityColor;
    [SerializeField] private Image item2SelectOutLine;
    [SerializeField] private TextMeshProUGUI item2RarityText;
    [SerializeField] private TextMeshProUGUI item2PowerTypeText;
    [SerializeField] private TextMeshProUGUI item2PowerText;
    [SerializeField] private TextMeshProUGUI compareText;
    [SerializeField] private TextMeshProUGUI item2Name;
    private float item2Power;


    private RelicsObject relicsObj1;
    private RelicsObject relicsObj2;

    private float compare;
    public GameObject upImage;
    public GameObject downImage;

    private bool selectMyItem;
    public bool SelectMyItem { get { return selectMyItem; } set { selectMyItem = value; OnSelect?.Invoke(selectMyItem); } }
    private event Action<bool> OnSelect;

    private RectTransform choosePanelScale;
    
    
    public void Init(RelicsObject obj1, RelicsObject obj2)
    {
        if (obj1 != null)
        {
            item1Panel.SetActive(true);
            relicsObj1 = obj1;
            Item1Setting();
            item1Button.onClick.AddListener(ClickButton1);
        }
        else
        {
            item1Panel.SetActive(false);
        }

        relicsObj2 = obj2;
        Item2Setting();
        item2Button.onClick.AddListener(ClickButton2);

        selectButton.onClick.AddListener(SelectItem);
        OnSelect += ChooseItem;

        CompareObj();
        selectButton.interactable = false;
    }


    public void OnDisable()
    {
        item1Button.onClick.RemoveListener(ClickButton1);
        item2Button.onClick.RemoveListener(ClickButton2);
    }

    // 아이템 1 세팅
    private void Item1Setting()
    {
        ItemRelicsSO so = (ItemRelicsSO)relicsObj1.itemSO;

        item1.sprite = so.icon;
        item1RarityImg.sprite = relicsObj1.itemRarityImage;

        item1Power = so.startPower[(int)relicsObj1.curRarity - 1] + so.upPower[(int)relicsObj1.curRarity - 1] * relicsObj1.itemLevel;
        item1PowerText.text = $"{item1Power.ToString()}";
        item1PowerTypeText.text = $"{relicsObj1.itemPowerType.ToString()} : ";

        item1RarityText.text = $"{relicsObj1.curRarity}";
        item1RarityColor.color = SetItemRarityColor(relicsObj1);

        item1Name.text = relicsObj1.itemName;
    }

    // 아이템 2 세팅
    private void Item2Setting()
    {
        ItemRelicsSO so = (ItemRelicsSO)relicsObj2.itemSO;

        item2.sprite = so.icon;
        item2RarityImg.sprite = relicsObj2.itemRarityImage;

        item2Power = so.startPower[(int)relicsObj2.curRarity - 1] + so.upPower[(int)relicsObj2.curRarity - 1] * relicsObj2.itemLevel;
        item2PowerText.text = $"{item2Power.ToString()}";
        item2PowerTypeText.text = $"{relicsObj2.itemPowerType.ToString()} : ";

        item2RarityText.text = $"{relicsObj2.curRarity}";

        item2RarityColor.color = SetItemRarityColor(relicsObj2);
        item2Name.text = relicsObj2.itemName;
    }

    private void ClickButton1()
    {
        SelectMyItem = true;


        if (!selectButton.interactable)
        {
            selectButton.interactable = true;
        }
    }

    private void ClickButton2()
    {
        SelectMyItem = false;

        if (!selectButton.interactable)
        {
            selectButton.interactable = true;
        }
    }

    private void SelectItem()
    {
        UIManager.Instance.ShowConfirm("정말 이 아이템을 선택 하시겠습니까?", () =>
        {
            if(relicsObj1 == null)
                InventoryManager.Instance.OnChangeAddItem?.Invoke(relicsObj2);
            else
                InventoryManager.Instance.OnChangeItem?.Invoke(relicsObj1, relicsObj2, selectMyItem);

            gameObject.SetActive(false);
            relicsGachaListPanel.SetActive(false);

            relicsObj1 = null;
            relicsObj2 = null;
        });
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

    private void CompareObj()
    {

        if (relicsObj1 == null)
        {
            compareText.text = "";
            return;
        }

        compare = item1Power - item2Power;
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

        if (compare == 0)
            compareText.text = "";
        else
            compareText.text = $"{Mathf.Abs(compare)}";
    }

    private void ChooseItem(bool value)
    {
        if (relicsObj1 == null)
            return;

        if (value)
        {
            item1SelectOutLine.color = Color.yellow;
            item2SelectOutLine.color = Color.white;
        }
        else
        {
            item2SelectOutLine.color = Color.yellow;
            item1SelectOutLine.color = Color.white;
        }
    }
}
