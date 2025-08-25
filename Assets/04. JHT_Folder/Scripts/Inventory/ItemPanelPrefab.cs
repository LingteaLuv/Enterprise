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
        public Image itemRarityImage;
        #endregion


        public void Init(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Weapon)
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
                itemObject = (RelicsObject)item;
                RelicsObject obj = (RelicsObject)itemObject;

                SetRelics((RelicsObject)item);
            }
        }

        private void ShowItem()
        {
            ItemEventManager.Instance.ClickItem(itemObject);
        }


        #region Relics


        private void SetRelics(RelicsObject item)
        {
            if (item.itemNum != itemObject.itemNum)
                return;

            itemRarityImage.sprite = item.itemRarityImage;
            itemStarImage.gameObject.SetActive(false);
            itemCountText.gameObject.SetActive(false);
            itemDetail.onClick.AddListener(ShowItem);
        }

        #endregion


        #region Weapon

        private void SetWeapon(WeaponObject item)
        {
            if (item.itemNum != itemObject.itemNum)
                return;

            itemName = item.itemName;
            itemLevel = item.ItemLevel;
            itemImage.sprite = item.itemIcon;
            itemCountText.text = item.ItemLevel.ToString();

        }


        private void UpCountAction(int value)
        {
            WeaponObject obj = (WeaponObject)itemObject;
            itemCountText.text = value.ToString();
        }

        private void UpStar(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)itemObject.itemSO;
            WeaponObject obj = (WeaponObject)itemObject;
            itemStarImage.sprite = so.starImage[value];
        }
        #endregion
    }
}
