using System;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.Sequence;
using UnityEngine;
using UnityEngine.UI;

public class TemporaryQuestController : UIBase
{
    
    [SerializeField] private QuestListController _questListController;
    [SerializeField] private Button _closeButton;
    
    [SerializeField] private Button _dailyButton;
    //[SerializeField] private GameObject _dailyPanel;
    
    [SerializeField] private Button _weeklyButton;
    //[SerializeField] private GameObject _weeklyPanel;
    
    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;

    [SerializeField] private Button[] _ChangeButtons;

    private List<TemporaryInstance> _dailyQuests;
    private List<TemporaryInstance> _weeklyQuests;
    private Button[] _stdButtons;
    private bool isDaily = true;
    
    public Action OnTouchedExitBtn;

    
    private void Start()
    {
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        _closeButton.onClick.AddListener(CloseQuestTab);
        _dailyButton.onClick.AddListener(() => ChangeQuestType(QuestType_Enum.Daily));
        _weeklyButton.onClick.AddListener(() => ChangeQuestType(QuestType_Enum.Weekly));
        foreach (var button in _ChangeButtons)
        {
            button.onClick.AddListener(() => ChangeQuestType(isDaily ? QuestType_Enum.Weekly : QuestType_Enum.Daily));
        }

        _stdButtons = new[] {_dailyButton, _weeklyButton};
        OpenQuestTab();
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveAllListeners();
        _dailyButton.onClick.RemoveAllListeners();
        _weeklyButton.onClick.RemoveAllListeners();
        foreach (var button in _ChangeButtons)
        {
            button.onClick.RemoveAllListeners();       
        }
    }
    private void OpenQuestTab()
    {
        ChangeQuestType(QuestType_Enum.Daily);
    }

    private void CloseQuestTab()
    {  
        OnTouchedExitBtn?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void ChangeQuestType(QuestType_Enum questType)
    {
        switch (questType)
        {
            case QuestType_Enum.Daily:
            {
                // _dailyPanel.SetActive(true);
                // _weeklyPanel.SetActive(false);

                for (var i = 0; i < 2; i++)
                {
                    _stdButtons[i].interactable = i == 1;
                    _stdButtons[i].image.sprite = i == 0 ? _activeSprite : _inactiveSprite;
                }
            
                _questListController.RebuildList(_dailyQuests);
                isDaily = true;
                
                break;
            }
            case QuestType_Enum.Weekly:
            {
                // _dailyPanel.SetActive(false);
                // _weeklyPanel.SetActive(true);

                for (var i = 0; i < 2; i++)
                {
                    _stdButtons[i].interactable = i == 0;
                    _stdButtons[i].image.sprite = i == 0 ? _inactiveSprite : _activeSprite;
                }
                
                _questListController.RebuildList(_weeklyQuests);
                isDaily = false;
                
                break;
            }
        }
    }

    public void UpdateQuest(TemporaryInstance inst)
    {
        _questListController.Refresh(inst);
    }

    public void QuestInit(List<TemporaryInstance> temporaryQuests, Dictionary<string, TemporaryInstance> temporaryInstances)
    {
        _dailyQuests = new List<TemporaryInstance>();
        _weeklyQuests = new List<TemporaryInstance>();

        foreach (var inst in temporaryQuests)
        {
            var list = inst.IsDaily ? _dailyQuests : _weeklyQuests;
            list.Add(inst);
        }
    }
}
