using System.Collections.Generic;
using System.Linq;
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
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        questDic.Clear();
        
        var sorted = quests
            .OrderBy(q => IsClearedOrReceived(q) ? 1 : 0)
            .ThenBy(q => ParseOrderSafe(q.TemporaryQuestId));

        foreach (var inst in sorted)
        {
            var card = Instantiate(questPrefab, content);
            card.CardSet(inst);
            questDic.TryAdd(inst, card);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void Refresh(TemporaryInstance inst)
    {
        if (questDic == null) return;
        if (!questDic.TryGetValue(inst, out var card)) return;
        
        card.CardSet(inst);
        
        ReorderVisuals();
    }

    private void ReorderVisuals()
    {
        if (questDic == null || questDic.Count == 0) return;

        var ordered = questDic.Keys
            .OrderBy(q => IsClearedOrReceived(q) ? 1 : 0)
            .ThenBy(q => ParseOrderSafe(q.TemporaryQuestId))
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var go = questDic[ordered[i]].transform as RectTransform;
            go.SetSiblingIndex(i);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private static bool IsClearedOrReceived(TemporaryInstance inst)
    {
        return inst.QuestState == QuestState_Enum.Received;
    }

    private static int ParseOrderSafe(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        return int.TryParse(id, out var n) ? n : 0;
    }
}