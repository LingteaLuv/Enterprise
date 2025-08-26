using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class ItemRelicsPanelItem : MonoBehaviour
    {
        public RelicsObject relicsObject;

        public Image frontImage;
        public Button itemDetailButton;
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI levelText;
        public Image itemRarityImage;

        public void Init(RelicsObject item)
        {
            relicsObject = item;
            RelicsObject obj = relicsObject;

            SetRelics();
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
    }
}
