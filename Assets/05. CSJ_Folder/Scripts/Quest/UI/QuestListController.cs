using System;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.UI;
using UnityEngine;
using UnityEngine.UI;

public class QuestListController : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private QuestPrefab questPrefab;

    private Dictionary<TemporaryQuestDefinitionSO, QuestPrefab> questDic;

    private void Awake()
    {
        questDic = new Dictionary<TemporaryQuestDefinitionSO, QuestPrefab>();
    }

    public void RebuildList(IEnumerable<(TemporaryQuestDefinitionSO def, TemporaryInstance inst)> quests)
    {
        for(int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        questDic.Clear();
        
        foreach ((TemporaryQuestDefinitionSO def, TemporaryInstance inst) in quests)
        {
            var card = Instantiate(questPrefab, content);
            card.CardSet(def, inst);
            questDic.Add(def, card);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void Refresh(TemporaryQuestDefinitionSO quest, TemporaryInstance inst)
    {
        if(questDic.TryGetValue(quest,out var card))
            card.CardSet(quest, inst);
    }
}
