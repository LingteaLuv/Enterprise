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
    public QuestInstance QuestInst;
    public QuestDefinitionSO QuestDef;
    public int QuestGoalCount;
    
    private GoalDefinitionSO QuestGoal;
    private QuestRewardSO QuestReward;

    public Action<QuestDefinitionSO, QuestInstance> OnRewardRequest;

    private void Awake()
    {
        _questButton.onClick.AddListener(OnClickQuest);
        #if UNITY_EDITOR
        _CheatButton.onClick.AddListener(OnClickCheatButton);
        #endif
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

    public void UpdateQuest(QuestDefinitionSO definition, QuestInstance instance )
    {
        QuestDef = definition;
        QuestInst = instance;
        
        QuestGoal = QuestDef.Goal;
        QuestReward = QuestDef.Reward;
        QuestGoalCount = QuestInst.CurrentGoalCount;
        
        _questNumberText.text = QuestNumber.ToString();
        _questGoalText.text = QuestDef.questName;
        if (definition.questName == "stageClear")
        {
            _questGoalText.text = QuestInst.stageClearMission;
        }
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
        
        //_questCurrentGoalImage.sprite = QuestReward.Reward.RewardIcon;
    }
    
    
    #if UNITY_EDITOR
    public Action OnForceClear;
    public void OnClickCheatButton()
    {
        OnForceClear?.Invoke();
    }
    #endif
}
