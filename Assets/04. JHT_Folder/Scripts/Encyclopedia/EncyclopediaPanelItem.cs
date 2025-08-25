using JHT;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaPanelItem : MonoBehaviour
{
    public Image encyclopediaImage;
    public Image frontImage;
    public string itemName;

    private ItemSO sample;
    public void Init(ItemSO _weaponObject)
    {
        sample = _weaponObject;
        encyclopediaImage.sprite = _weaponObject.icon;
        frontImage.sprite = _weaponObject.backImage;
        itemName = _weaponObject.itemName;
        InventoryManager.Instance.OnAddInventory += OpenItem;
    }

    private void OpenItem(ItemObject item)
    {
        if (item.itemNum == sample.itemNum)
        {
            if (frontImage != null)
                Destroy(frontImage.gameObject);
        }
            
    }
}
