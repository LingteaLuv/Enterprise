using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class RelicsStatPanel : MonoBehaviour
    {
        private RelicsObject curRelics;
        [SerializeField] private Image relicsImage;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI powerTypeText;

        public void Init(RelicsObject relics)
        {
            curRelics = relics;
            ItemRelicsSO so = (ItemRelicsSO)curRelics.itemSO;

            relicsImage.sprite = relics.itemSO.icon;
            powerText.text = relics.itemPower.ToString();
            levelText.text = relics.itemLevel.ToString();
            powerTypeText.text = relics.itemPowerType.ToString();
        }
    }
}
