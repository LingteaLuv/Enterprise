using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class ItemRelicsPanelItem : JHT_PooledObject
    {
        public RelicsObject relicsObject;

        public Image frontImage;
        public TextMeshProUGUI itemName;
        

        public void Init(RelicsObject item)
        {
            relicsObject = item;

            ItemEventManager.Instance.OnClickItem -= HandleSelected;
            ItemEventManager.Instance.OnClickItem += HandleSelected;
            SetRelics();
        }

        public void Outit(RelicsObject item)
        {
            if (relicsObject == item)
            {
                relicsObject = null;
            }
        }

        private void SetRelics()
        {
            if (relicsObject == null)
                return;

            frontImage.sprite = relicsObject.itemIcon;
            itemName.text = relicsObject.itemName;
        }

        private void HandleSelected(ItemObject clicked)
        {
            if (clicked == null || clicked is WeaponObject)
                return;

            bool value = ReferenceEquals(clicked, relicsObject)
                        || (clicked != null && clicked.itemNum == relicsObject.itemNum);

        }

    }
}
