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

    private List<TemporaryInstance> _dailyQuests;
    private List<TemporaryInstance> _weeklyQuests;
    
    
    private void OnEnable()
    {
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
            _questListController.RebuildList(_dailyQuests);
        }
        
        else if (questType == QuestType_Enum.Weekly)
        {
            Debug.Log("weekly");
            _dailyPanel.SetActive(false);
            _weeklyPanel.SetActive(true);
            TypeText.text = "주간 퀘스트";
            _weeklyButton.interactable = false;
            _dailyButton.interactable = true;
            _questListController.RebuildList(_weeklyQuests);
        }
    }

    public void UpdateQuest(TemporaryInstance inst)
    {
        _questListController.Refresh(inst);
    }

    public void QuestInit(TemporaryQuestListSO temporaryQuests, Dictionary<string, TemporaryInstance> temporaryInstances)
    {
        _dailyQuests = new List<TemporaryInstance>();
        _weeklyQuests = new List<TemporaryInstance>();

        foreach (var quest in temporaryQuests.GetSequence())
        {
            for (int i = 0; i < quest.QuestCount; i++)
            {
                var id = (quest.GetQuestKeyByType(quest.isDaily(i) ? QuestType_Enum.Daily : QuestType_Enum.Weekly)+ i).ToString();
                if (temporaryInstances.TryGetValue( id, out var inst))
                {
                    var list = inst.IsDaily ? _dailyQuests : _weeklyQuests;
                    list.Add(inst);
                }
            }
        }
    }
}
