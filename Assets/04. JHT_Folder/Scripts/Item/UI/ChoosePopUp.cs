using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoosePopUp : MonoBehaviour
{
    [SerializeField] private Button item1Button;
    [SerializeField] private Button item2Button;

    [SerializeField] private Image item1;
    [SerializeField] private Image item1RarityImg;

    [SerializeField] private Image item2;
    [SerializeField] private Image item2RarityImg;


    private RelicsObject relicsObj1;
    private RelicsObject relicsObj2;

    public void Init(RelicsObject obj1, RelicsObject obj2)
    {
        relicsObj1 = obj1;
        relicsObj2 = obj2;

        item1.sprite = relicsObj1.itemSO.icon;
        item2.sprite = relicsObj2.itemSO.icon;
        item1RarityImg.sprite = relicsObj1.itemRarityImage;
        item2RarityImg.sprite = relicsObj2.itemRarityImage;

        item1Button.onClick.AddListener(ClickButton1);
        item2Button.onClick.AddListener(ClickButton2);
    }

    public void OnDisable()
    {
        item1Button.onClick.RemoveListener(ClickButton1);
        item2Button.onClick.RemoveListener(ClickButton2);
    }

    private void ClickButton1()
    {
        InventoryManager.Instance.OnChangeItem?.Invoke(relicsObj1, relicsObj2, true);
        this.gameObject.SetActive(false);
    }

    private void ClickButton2()
    {
        InventoryManager.Instance.OnChangeItem?.Invoke(relicsObj1, relicsObj2, false);
        gameObject.SetActive(false);
    }
}
