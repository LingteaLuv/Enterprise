using System;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class QuestUIController : MonoBehaviour
{
    [SerializeField] public Button _questButton;
    [SerializeField] private TextMeshProUGUI _questNumberText;
    [SerializeField] private TextMeshProUGUI _questGoalText;
    [SerializeField] private TextMeshProUGUI _questGoalCountText;    
    [SerializeField] private Button _CheatButton;
    [SerializeField] private Image _questCurrentGoalImage;
    [SerializeField] private Button _InitButton;

    
    [NonSerialized] public int QuestNumber;
    public GeneralQuestInstance QuestInst;
    [NonSerialized] public GeneralQuestDefinitionSO QuestDef;
    private int QuestGoalCount;
    
    private GoalDefinitionSO QuestGoal;
    private QuestRewardSO QuestReward;

    public Action<QuestDefinitionSO, QuestInstance> OnRewardRequest;

    private void Awake()
    {
        _questButton.onClick.AddListener(OnClickQuest);
        // SubmitButton();
    }

    private void OnClickQuest()
    {
        ClickQuest(QuestInst.QuestState);
    }

    private void ClickQuest(QuestState_Enum questState)
    {
        if (questState == QuestState_Enum.Completed)
        {
            OnRewardRequest?.Invoke(QuestDef, QuestInst);
        }
        // TODO : 우당탕탕 처럼 완료되지 않은 퀘스트 클릭시 수행 위치로 이동?
    }

    public void UpdateQuest(GeneralQuestDefinitionSO definition, GeneralQuestInstance instance )
    {
        QuestDef = definition;
        QuestInst = instance;
        
        QuestGoal = QuestDef.Goal;
        QuestReward = QuestDef.Reward;
        QuestGoalCount = QuestInst.CurrentGoalCount;
        
        _questNumberText.text = QuestNumber.ToString();

        if (QuestDef.GeneralType == GeneralType_Enum.StageClear)
        {
            _questGoalText.text = QuestInst.stageClearMission;
            _questGoalCountText.text = QuestInst.needToClearStage <= QuestManager.Instance.CurrentClearedStage 
                ? "클리어" 
                : $"{QuestManager.Instance.CurrentClearedStage} / {QuestInst.needToClearStage}";
        }
        else
        {
            _questGoalText.text = QuestDef.questName;
            if (QuestGoalCount < QuestGoal.RequireCount)
            {
                _questGoalCountText.text = $"{QuestGoalCount} / {QuestGoal.RequireCount}";
                _questButton.image.canvasRenderer.SetAlpha(0.3f);
            }
            else
            {
                _questGoalCountText.text = "클리어";
            }
        }
        _questButton.image.canvasRenderer.SetAlpha(
            QuestInst.QuestState == QuestState_Enum.Completed ? 0.7f : 0.3f);

        if(QuestReward.Reward.RewardIcon is not null)
            _questCurrentGoalImage.sprite = QuestReward.Reward.RewardIcon;
    }
    
    
    // public Action OnForceClear;
    // public Action OnForceInit;
    // [SerializeField] private Button KillButton;
    // [SerializeField] private Button GachaButton;
    // [SerializeField] private Button LevelUpButton;
    // [SerializeField] private Button UpgradeButton;
    // private void OnClickCheatButton()
    // {
    //     OnForceClear?.Invoke();
    // }
    //
    // private void OnClickInitButton()
    // {
    //     OnForceInit?.Invoke();
    // }
    //
    // private void SubmitButton()
    // {
    //     if (_CheatButton !=null)
    //         _CheatButton.onClick.AddListener(OnClickCheatButton);
    //     if (KillButton !=null)
    //         KillButton.onClick.AddListener(OnClickKillButton);
    //     if (GachaButton !=null)
    //         GachaButton.onClick.AddListener(OnClickGachaButton);
    //     if (LevelUpButton !=null)
    //         LevelUpButton.onClick.AddListener(OnClickLevelUpButton);
    //     if (UpgradeButton !=null)
    //         UpgradeButton.onClick.AddListener(OnClickUpgradeButton);
    //     if (_InitButton is not null)
    //         _InitButton.onClick.AddListener(OnClickInitButton);
    // }
    //
    // private void OnClickKillButton()
    // {
    //     QuestSignalManager.Instance.KillEnemy(MonsterId.All);
    // }
    //
    // private void OnClickGachaButton()
    // {
    //     QuestSignalManager.Instance.GachaPull(ItemType.Character, 5);
    //     QuestSignalManager.Instance.GachaPull(ItemType.Equipment, 5);
    //     QuestSignalManager.Instance.GachaPull(ItemType.Relic, 5);
    //     QuestSignalManager.Instance.GachaPull(ItemType.RareRelic, 5);
    // }
    //
    // private void OnClickLevelUpButton()
    // {
    //     QuestSignalManager.Instance.LevelUp(ItemType.Character, 5);
    //     QuestSignalManager.Instance.LevelUp(ItemType.Relic, 5);
    //     QuestSignalManager.Instance.LevelUp(ItemType.Equipment, 5);
    // }
    //
    // private void OnClickUpgradeButton()
    // {
    //     QuestSignalManager.Instance.Upgrade(UpgradeType.Atk,5);
    //     QuestSignalManager.Instance.Upgrade(UpgradeType.Def,5);
    //     QuestSignalManager.Instance.Upgrade(UpgradeType.Hp, 5);
    // }
    
}
