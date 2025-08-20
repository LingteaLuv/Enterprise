using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    public class ItemPanelPrefab : MonoBehaviour
    {
        public ItemObject itemObject;
        public string itemName;
        public int itemLevel;
        public Image itemImage;


        #region Weapon

        public TextMeshProUGUI itemCountText;
        public Image itemStarCount;

        #endregion

        #region Relics

        #endregion


        public void Init(ItemObject item)
        {
            if (item as WeaponObject)
            {
                WeaponObject obj = (WeaponObject)item;
                SetWeapon(obj);
                obj.OnUpCount += UpCountAction;
                obj.OnUpgrade += UpGradeAction;
            }
            else
            {
                SetRelics((RelicsObject)item);
            }
        }

        private void SetWeapon(WeaponObject item)
        {
            itemObject = item;
            itemName = item.itemName;
            itemLevel = item.itemLevel;
            itemImage.sprite = item.itemIcon;
            itemCountText.text = item.itemLevel.ToString();
        }

        private void SetRelics(RelicsObject item)
        {

        }

        //Action으로 WeaponItem을 받아와야할듯? 
        private void UpCountAction(WeaponObject item)
        {
            //Weapon
            itemCountText.text = item.itemLevel.ToString();
        }

        private void UpGradeAction(WeaponObject item)
        {

        }

    }
}
