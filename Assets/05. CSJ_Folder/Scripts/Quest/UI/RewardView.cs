using _05._CSJ_Folder.Scripts.Quest.SO.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class RewardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;

        public void Set(RewardBundleSO.Item item)
        {
            if (icon) icon.sprite = item.icon;
            if (amountText) amountText.text = $"X{item.amount:N0}";
        }

        public void SetChar(RewardBundleSO.Item item)
        {
            if (icon){ icon.sprite = item.data.characterSprite;}
            if (amountText) amountText.text = $"X{item.amount:N0}";
        }
    }
}