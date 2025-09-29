using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class CodexPrefab : MonoBehaviour
    {
        [SerializeField] private Button _CompleteButton;
        [SerializeField] private TextMeshProUGUI _QuestContent;
        [SerializeField] private GameObject _FlagImage;
        [SerializeField] private Image[] RewardSprite;
        [SerializeField] private TextMeshProUGUI[] RewardText;

        [SerializeField] private Image _statImage;

        [SerializeField] private Sprite _critChance;
        [SerializeField] private Sprite _critDamage;
        
        private UnityAction ButtonEvent;
        private Image objectImg;
        [NonSerialized] public bool IsReceived;
        public bool isFirst = false;
        
        public void CardSet(CodexInstance inst)
        {
            
            if (inst.IsReceived)
            {
                _CompleteButton.gameObject.SetActive(false);
                IsReceived = true;
            }
            else if (inst.IsCleared)
            {
                _CompleteButton.gameObject.SetActive(true);
                if (isFirst)
                    _CompleteButton.interactable = true;
                else
                    _CompleteButton.interactable = false;
                _FlagImage.SetActive(true);
            }
            else
            {
                _CompleteButton.gameObject.SetActive(true);
                _CompleteButton.interactable = false;
                _FlagImage.SetActive(false);
            }

            _QuestContent.text = (inst.Index+1).ToString();
            //TODO : RewardImage
            if (inst.RewardSO.RewardContents is not null)
            {
                for (var i = 0; i < inst.RewardSO.RewardContents.Length; i++)
                {
                    RewardSprite[i].sprite = inst.RewardSO.RewardContents[i].RewardIcon;
                    RewardText[i].text = inst.RewardSO.RewardContents[i].amount.ToString();
                }
            }
            _statImage.sprite = inst.StatType == CodexStat.CritChance ? _critChance : _critDamage;

            _CompleteButton.onClick.RemoveAllListeners();
            _CompleteButton.onClick.AddListener(() => CodexSiganlManager.Instance.OnCompleteQuest(inst));
        }

        public void OnDestroy()
        {
            _CompleteButton.onClick?.RemoveAllListeners();
        }
    }
}