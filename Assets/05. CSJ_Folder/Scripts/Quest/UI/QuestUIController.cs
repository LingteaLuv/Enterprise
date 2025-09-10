using System;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

    
    public int QuestNumber;
    [SerializeField] public GeneralQuestInstance QuestInst;
    [SerializeField] public GeneralQuestDefinitionSO QuestDef;
    public int QuestGoalCount;
    
    private GoalDefinitionSO QuestGoal;
    private QuestRewardSO QuestReward;

    public Action<QuestDefinitionSO, QuestInstance> OnRewardRequest;

    private void Awake()
    {
        _questButton.onClick.AddListener(OnClickQuest);
        SubmitButton();
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
            if (QuestInst.needToClearStage <= QuestManager.Instance.CurrentClearedStage)
            {
                _questGoalCountText.text = "클리어";
            }
            else
            {
                _questGoalCountText.text = $"{QuestManager.Instance.CurrentClearedStage} / {QuestInst.needToClearStage}";
            }
        }
        else
        {
            _questGoalText.text = QuestDef.questName;
            if (QuestGoalCount < QuestGoal.RequireCount)
            {
                _questGoalCountText.text = $"{QuestGoalCount} / {QuestGoal.RequireCount}";
                _questButton.image.canvasRenderer.SetAlpha(0.3f);
                ColorBlock buttonColor = _questButton.colors;
                buttonColor.normalColor = Color.black;
            }
            else
            {
                _questGoalCountText.text = "클리어";
                _questButton.image.canvasRenderer.SetAlpha(0.7f);
                ColorBlock buttonColor = _questButton.colors;
                buttonColor.normalColor = Color.white;
            }
        }

        //_questCurrentGoalImage.sprite = QuestReward.Reward.RewardIcon;
    }
    
    
    public Action OnForceClear;
    [SerializeField] private Button KillButton;
    [SerializeField] private Button GachaButton;
    [SerializeField] private Button LevelUpButton;
    [SerializeField] private Button UpgradeButton;
    private void OnClickCheatButton()
    {
        OnForceClear?.Invoke();
    }
    
    private void SubmitButton()
    {
        if (_CheatButton !=null)
            _CheatButton.onClick.AddListener(OnClickCheatButton);
        if (KillButton !=null)
            KillButton.onClick.AddListener(OnClickKillButton);
        if (GachaButton !=null)
            GachaButton.onClick.AddListener(OnClickGachaButton);
        if (LevelUpButton !=null)
            LevelUpButton.onClick.AddListener(OnClickLevelUpButton);
        if (UpgradeButton !=null)
            UpgradeButton.onClick.AddListener(OnClickUpgradeButton);
    }

    private void OnClickKillButton()
    {
        QuestSignalManager.Instance.KillEnemy(MonsterId.All);
    }

    private void OnClickGachaButton()
    {
        QuestSignalManager.Instance.GachaPull(ItemType.Character, 5);
        QuestSignalManager.Instance.GachaPull(ItemType.Equipment, 5);
        QuestSignalManager.Instance.GachaPull(ItemType.Relic, 5);
    }

    private void OnClickLevelUpButton()
    {
        QuestSignalManager.Instance.LevelUp(ItemType.Character, 5);
        QuestSignalManager.Instance.LevelUp(ItemType.Relic, 5);
        QuestSignalManager.Instance.LevelUp(ItemType.Equipment, 5);
    }

    private void OnClickUpgradeButton()
    {
        QuestSignalManager.Instance.Upgrade(UpgradeType.Atk,5);
        QuestSignalManager.Instance.Upgrade(UpgradeType.Def,5);
        QuestSignalManager.Instance.Upgrade(UpgradeType.Hp, 5);
    }
}
