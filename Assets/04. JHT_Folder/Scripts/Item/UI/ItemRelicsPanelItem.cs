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
        public Button itemDetailButton;
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI levelText;
        public Image itemRarityImage;

        [Header("Current Click Item")]
        public GameObject curClickItem;

        

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

        private void ShowItem()
        {
            ItemEventManager.Instance.ClickItem(relicsObject);
        }

        private void SetRelics()
        {
            if (relicsObject == null)
                return;

            frontImage.sprite = relicsObject.itemIcon;
            powerText.text = $"{relicsObject.itemPowerType.ToString()} : {relicsObject.itemPower} Up!";
            itemRarityImage.sprite = relicsObject.itemRarityImage;
            levelText.text = $"Level : {relicsObject.itemLevel}";
            itemDetailButton.onClick.AddListener(ShowItem);
        }

        private void HandleSelected(ItemObject clicked)
        {
            if (clicked == null || clicked is WeaponObject)
                return;

            bool value = ReferenceEquals(clicked, relicsObject)
                        || (clicked != null && clicked.itemNum == relicsObject.itemNum);

            
            curClickItem.SetActive(value);
        }

    }
}
