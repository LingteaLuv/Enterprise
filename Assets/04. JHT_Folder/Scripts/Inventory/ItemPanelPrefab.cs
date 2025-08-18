using JHT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemPanelPrefab : MonoBehaviour, IPointerClickHandler
{
    public ItemObject itemObject;
    public string itemName;
    public int itemLevel;
    public int itemPower;
    public Image itemImage;

    private bool isCheck;

    public void Init(ItemObject item)
    {
        itemObject = item;
        itemName = item.weapon.itemName;
        itemLevel = item.itemLevel;
        itemPower = (int)item.attackDamage;
        itemImage.sprite = item.itemIcon;
        Debug.Log($"스프라이트 : {itemImage.sprite} ::: {item.itemIcon}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isCheck = !isCheck;
        if (InventoryManager.Instance.currentMode == InventoryMode.CheckForUpgrade)
        {
            
        }
    }

    public void ForUpgrade()
    {
        if (InventoryManager.Instance.currentMode == InventoryMode.CheckForUpgrade)
        {

        }
    }

    public void ForDelete()
    {
        if (InventoryManager.Instance.currentMode == InventoryMode.CheckForUpgrade)
        {

        }
    }
}
