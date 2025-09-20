using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class CodexPrefab : MonoBehaviour
    {
        [SerializeField] private Button _CompleteButton;
        [SerializeField] private GameObject _ClearButton;
        [SerializeField] private Slider _progress;
        [SerializeField] private TextMeshProUGUI _progressContent;
        [SerializeField] private TextMeshProUGUI _questContent;
        [SerializeField] private Image RewardSprite;
        
        private UnityAction ButtonEvent;
        private Image objectImg;
        
        public void CardSet(CodexInstance inst)
        {
            
            if (inst.IsReceived)
            {
                _CompleteButton.gameObject.SetActive(false);
                _ClearButton.SetActive(true);
            }
            else if (inst.IsCleared)
            {
                _CompleteButton.gameObject.SetActive(true);
                _CompleteButton.interactable = true;
                _ClearButton.SetActive(false);
            }
            else
            {
                _CompleteButton.gameObject.SetActive(true);
                _CompleteButton.interactable = false;
                _ClearButton.gameObject.SetActive(false);
            }

            _progress.maxValue = inst.MaxProgress;
            _progress.value = inst.CurrentProgress;
            _progressContent.text = $"{inst.CurrentProgress} / {inst.MaxProgress}";
            _questContent.text = inst.ProgressText;
            //TODO : RewardImage
            // if (inst.Def.Reward?.RewardIcon is not null)
            // {
            //     RewardSprite.sprite = inst.Def.Reward.Reward.RewardIcon;
            // }

            _CompleteButton.onClick.RemoveAllListeners();
            _CompleteButton.onClick.AddListener(() => CodexSiganlManager.Instance.OnCompleteQuest(inst));
        }

        public void OnDestroy()
        {
            _CompleteButton.onClick?.RemoveAllListeners();
        }
    }
}