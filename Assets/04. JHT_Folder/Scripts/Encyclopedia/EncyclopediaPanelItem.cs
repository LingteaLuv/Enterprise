using JHT;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaPanelItem : MonoBehaviour
{
    public Image encyclopediaImage;
    public Image lockImage;
    public string itemName;

    private ItemSO sample;

    InventoryManager inventoryManager;

    public void Init(ItemSO _weaponObject)
    {
        inventoryManager = InventoryManager.Instance;

        sample = _weaponObject;
        encyclopediaImage.sprite = _weaponObject.icon;
        lockImage.sprite = _weaponObject.backImage;
        itemName = _weaponObject.itemName;
        if (inventoryManager != null)
            inventoryManager.OnAddItemForEncyclopedia += OpenItem;
    }

    private void OnDestroy()
    {
        if(inventoryManager != null)
        inventoryManager.OnAddItemForEncyclopedia -= OpenItem;
    }

    private void OpenItem(ItemObject item)
    {
        if (item.itemName == sample.itemName)
        {
            lockImage.gameObject.SetActive(false);
        }
            
    }
}
