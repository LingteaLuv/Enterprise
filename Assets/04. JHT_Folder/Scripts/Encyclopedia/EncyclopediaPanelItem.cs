using JHT;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaPanelItem : MonoBehaviour
{
    public Image encyclopediaImage;
    public Image frontImage;
    public string itemName;

    private DataItem sample;
    public void Init(DataItem _weaponObject)
    {
        ItemWeaponSO weaponSO = (ItemWeaponSO)_weaponObject.itemSO;

        sample = _weaponObject;
        encyclopediaImage.sprite = _weaponObject.itemSO.icon;
        frontImage.sprite = weaponSO.backImage;
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
