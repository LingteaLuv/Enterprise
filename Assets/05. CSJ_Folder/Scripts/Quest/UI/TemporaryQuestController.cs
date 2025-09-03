using System;
using System.Collections.Generic;
using System.Linq;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.Sequence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemporaryQuestController : MonoBehaviour
{
    
    [SerializeField] private QuestListController _questListController;
    [SerializeField] private QuestRewardController _questRewardController;
    [SerializeField] private Button _closeButton;
    
    [SerializeField] private Button _dailyButton;
    [SerializeField] private GameObject _dailyPanel;
    
    [SerializeField] private Button _weeklyButton;
    [SerializeField] private GameObject _weeklyPanel;

    [SerializeField] private TextMeshProUGUI TypeText;

    private Dictionary<TemporaryQuestDefinitionSO, TemporaryInstance> _dailyQuests;
    private Dictionary<TemporaryQuestDefinitionSO, TemporaryInstance> _weeklyQuests;

    private bool isReady = false;
    public bool isStarted = false;
    
    private void OnEnable()
    {
        if (!isReady && !isStarted)
        {
            Debug.Log("Disable");
            gameObject.SetActive(false);
            isStarted = true;
            return;
        } 
        Debug.Log("Enable");
        OpenQuestTab();
        _closeButton.onClick.AddListener(CloseQuestTab);
        _dailyButton.onClick.AddListener(() => ChangeQuestType(QuestType_Enum.Daily));
        _weeklyButton.onClick.AddListener(() => ChangeQuestType(QuestType_Enum.Weekly));
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveAllListeners();
        _dailyButton.onClick.RemoveAllListeners();
        _weeklyButton.onClick.RemoveAllListeners();
    }
    private void OpenQuestTab()
    {
        ChangeQuestType(QuestType_Enum.Daily);
    }

    private void CloseQuestTab()
    {  
        gameObject.SetActive(false);
    }
    private void ChangeQuestType(QuestType_Enum questType)
    {
        if (questType == QuestType_Enum.Daily)
        {
            Debug.Log("daily");
            _dailyPanel.SetActive(true);
            _weeklyPanel.SetActive(false);
            TypeText.text = "일일 퀘스트";
            _dailyButton.interactable = false;
            _weeklyButton.interactable = true;
            _questListController.RebuildList(_dailyQuests.Select(kv => (kv.Key, kv.Value)));
        }
        
        else if (questType == QuestType_Enum.Weekly)
        {
            Debug.Log("weekly");
            _dailyPanel.SetActive(false);
            _weeklyPanel.SetActive(true);
            TypeText.text = "주간 퀘스트";
            _weeklyButton.interactable = false;
            _dailyButton.interactable = true;
            _questListController.RebuildList(_weeklyQuests.Select(kv => (kv.Key, kv.Value)));
        }
    }

    public void UpdateQuest(TemporaryQuestDefinitionSO def, TemporaryInstance inst)
    {
        _questListController.Refresh(def, inst);
    }

    public void QuestInit(TemporaryQuestListSO temporaryQuests, Dictionary<string, TemporaryInstance> temporaryInstances)
    {
        _dailyQuests = new Dictionary<TemporaryQuestDefinitionSO, TemporaryInstance>();
        _weeklyQuests = new Dictionary<TemporaryQuestDefinitionSO, TemporaryInstance>();
        foreach (var quest in temporaryQuests.DailyQuests)
        {
            if (temporaryInstances.TryGetValue(quest.questId, out var inst))
            {
                _dailyQuests[quest] = inst;
            }
        }

        foreach (var quest in temporaryQuests.WeeklyQuests)
        {
            if (temporaryInstances.TryGetValue(quest.questId, out var inst))
            {
                _weeklyQuests[quest] = inst;
            }
        }
        
    }
}
