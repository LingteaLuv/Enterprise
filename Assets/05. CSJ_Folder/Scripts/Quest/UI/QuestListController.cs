using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.UI;
using UnityEngine;
using UnityEngine.UI;

public class QuestListController : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private QuestPrefab questPrefab;

    private Dictionary<TemporaryInstance, QuestPrefab> questDic;

    private void Awake()
    {
        questDic = new Dictionary<TemporaryInstance, QuestPrefab>();
    }

    public void RebuildList(IEnumerable<TemporaryInstance> quests)
    {
        for(int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        if (questDic == null) return;
        questDic.Clear();
        foreach (var inst in quests)
        {
            var card = Instantiate(questPrefab, content);
            card.CardSet(inst);
            questDic.Add(inst, card);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void Refresh(TemporaryInstance inst)
    {
        if (questDic == null) return;
        if (questDic.TryGetValue(inst, out var card))
            card.CardSet(inst);
    }
}
