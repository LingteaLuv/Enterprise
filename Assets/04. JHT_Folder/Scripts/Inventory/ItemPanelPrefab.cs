using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    // 모두 IPointer 인터페이스로
    public class ItemPanelPrefab : MonoBehaviour
    {
        public ItemObject itemObject;
        public string itemName;
        public int itemLevel;
        public Image itemImage;
        public Button itemDetail;

        #region Weapon

        public TextMeshProUGUI itemCountText;
        public Image itemStarImage;

        #endregion

        #region Relics

        #endregion


        public void Init(ItemObject item)
        {
            if (item is WeaponObject)
            {
                itemObject = (WeaponObject)item;
                WeaponObject obj = (WeaponObject)itemObject;
                SetWeapon(obj);
                obj.OnChangeLevel += UpCountAction;
                obj.OnChangeStar += UpStar;
                itemDetail.onClick.AddListener(ShowItem);
                UpStar(obj.itemStar);
            }
            else
            {
                SetRelics((RelicsObject)item);
            }
        }

        private void ShowItem()
        {
            ItemEventManager.Instance.ClickItem(itemObject);
        }

        private void SetWeapon(WeaponObject item)
        {
            if (item.itemNum != itemObject.itemNum)
                return;

            itemObject = item;
            itemName = item.itemName;
            itemLevel = item.ItemLevel;
            itemImage.sprite = item.itemIcon;
            itemCountText.text = item.ItemLevel.ToString();

        }

        private void SetRelics(RelicsObject item)
        {

        }

        //Action으로 WeaponItem을 받아와야할듯? 
        private void UpCountAction(int value)
        {
            WeaponObject obj = (WeaponObject)itemObject;
            //Weapon
            itemCountText.text = value.ToString();
        }

        private void UpStar(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)itemObject.itemSO;
            WeaponObject obj = (WeaponObject)itemObject;
            itemStarImage.sprite = so.starImage[value];
        }
    }
}
