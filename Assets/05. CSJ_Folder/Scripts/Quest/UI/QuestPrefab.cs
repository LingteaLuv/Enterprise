using System;
using _05._CSJ_Folder.Scripts.Quest.Definition;
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

        private string QuestId;
        private UnityAction ButtonEvent;
        private Image objectImg;
        
        private Color CompleteColor = new Color(20, 20, 20, 150);
        private Color ActiveColor = new Color(152, 152,152, 150);

        public void CardSet(QuestDefinitionSO  def, QuestInstance inst)
        {
            if (objectImg == null) objectImg = gameObject.GetComponent<Image>();

            switch (inst.QuestState)
            {
                case QuestState_Enum.BeforeActive:
                case QuestState_Enum.Received:
                    _CompleteButton.gameObject.SetActive(false);
                    _ClearButton.SetActive(true);
                    objectImg.color = CompleteColor;
                    break;
                case QuestState_Enum.Completed:
                    _CompleteButton.gameObject.SetActive(true);
                    _CompleteButton.interactable = true;
                    _ClearButton.SetActive(false);
                    objectImg.color = ActiveColor;
                    break;
                case QuestState_Enum.Active:
                    _CompleteButton.gameObject.SetActive(true);
                    _CompleteButton.interactable = false;
                    _ClearButton.gameObject.SetActive(false);
                    objectImg.color = ActiveColor;
                    break;
                default:
                    break;
            }

            _progress.maxValue = def.Goal.RequireCount;
            _progress.value = inst.CurrentGoalCount;
            _progressContent.text = $"{inst.CurrentGoalCount} / {def.Goal.RequireCount}";
            _questContent.text = def.questName;
            //TODO : RewardImage
            
            QuestId = def.questId;
            _CompleteButton.onClick.RemoveAllListeners();
            _CompleteButton.onClick.AddListener(() => QuestSignalManager.Instance.OnCompleteQuest(QuestId));
        }

        public void OnDestroy()
        {
            _CompleteButton.onClick?.RemoveAllListeners();
        }
    }
}