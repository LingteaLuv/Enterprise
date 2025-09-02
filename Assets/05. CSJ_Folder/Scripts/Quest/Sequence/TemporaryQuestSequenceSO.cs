using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    [CreateAssetMenu(menuName = "Quest/Sequence/Temporary")]
    public class TemporaryQuestListSO : ScriptableObject, ISequence<TemporaryQuestDefinitionSO>
    {
        public TemporaryQuestDefinitionSO[] DailyQuests;
        
        public TemporaryQuestDefinitionSO[] WeeklyQuests;

        public TemporaryQuestDefinitionSO[] GetSequence()
        {
            return DailyQuests;
        }

        public TemporaryQuestDefinitionSO[] GetWeeklyQuests()
        {
            return WeeklyQuests;
        }
    }
}