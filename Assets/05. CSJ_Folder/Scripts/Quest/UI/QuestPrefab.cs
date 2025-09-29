using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class QuestPrefab : MonoBehaviour
    {
        [SerializeField] private Button _CompleteButton;
        [SerializeField] private GameObject _ClearButton;
        [SerializeField] private Slider _progress;
        [SerializeField] private TextMeshProUGUI _progressContent;
        [SerializeField] private TextMeshProUGUI _questContent;
        [SerializeField] private Image RewardSprite;
        [SerializeField] private TextMeshProUGUI RewardText;

        [SerializeField] private string QuestId;
        private UnityAction ButtonEvent;
        //private Image objectImg;
        

        
        private readonly Color CompleteColor = new Color(255, 255, 255, 255);
        private readonly Color ActiveColor = new Color(255, 255,255, 100);

        public void CardSet(TemporaryInstance inst)
        {
            //if (objectImg == null) objectImg = gameObject.GetComponent<Image>();

            switch (inst.QuestState)
            {
                case QuestState_Enum.Received:
                    _CompleteButton.gameObject.SetActive(false);
                    _ClearButton.SetActive(true);
                    //objectImg.color = CompleteColor;
                    break;
                case QuestState_Enum.Completed:
                    _CompleteButton.gameObject.SetActive(true);
                    _CompleteButton.interactable = true;
                    _ClearButton.SetActive(false);
                    _CompleteButton.image.color = CompleteColor;
                    //objectImg.color = ActiveColor;
                    break;
                case QuestState_Enum.Active:
                    _CompleteButton.gameObject.SetActive(true);
                    _CompleteButton.interactable = false;
                    _ClearButton.gameObject.SetActive(false);
                    _CompleteButton.image.color = ActiveColor;
                    //objectImg.color = ActiveColor;
                    break;
            }

            _progress.maxValue = inst.DemandedGoalCount;
            _progress.value = inst.CurrentGoalCount;
            _progressContent.text = $"{inst.CurrentGoalCount} / {inst.DemandedGoalCount}";
            _questContent.text = inst.temporaryQuestContent;
            //TODO : RewardImage
            if (inst.Def.Reward?.Reward.RewardIcon is not null)
            {
                RewardSprite.sprite = inst.Def.Reward.Reward.RewardIcon;
                RewardText.text = $"{DataUtility.FormatNumber((BigInteger)inst.Def.Reward.Reward.GetAmount(inst.Def, inst))}";
            }
            
            QuestId = inst.TemporaryQuestId;
            _CompleteButton.onClick.RemoveAllListeners();
            _CompleteButton.onClick.AddListener(() => QuestSignalManager.Instance.OnCompleteQuest(QuestId));
        }

        public void OnDestroy()
        {
            _CompleteButton.onClick?.RemoveAllListeners();
        }
    }
}