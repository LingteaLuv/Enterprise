using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    [CreateAssetMenu(menuName = "Quest/Sequence/Temporary")]
    public class TemporaryQuestListSO : ScriptableObject, ISequence<TemporaryQuestDefinitionSO>
    {

        [SerializeField] private TemporaryQuestDefinitionSO[] TemporaryQuests;
        
        public TemporaryQuestDefinitionSO[] GetSequence()
        {
            return TemporaryQuests;
        }
    }
}