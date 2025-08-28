using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    [CreateAssetMenu(menuName = "Quest/Sequence/Temporary")]
    public class TemporaryQuestListSO : ScriptableObject, ISequence<QuestDefinitionSO>
    {
        public QuestType_Enum ListType;
        public QuestDefinitionSO[] Quests;

        public QuestDefinitionSO[] GetSequence()
        {
            return Quests;
        }
    }
}