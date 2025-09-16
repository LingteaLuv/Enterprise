using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    [CreateAssetMenu(menuName = "Quest/Sequence/Tutorial")]
    public class TutorialSequenceSO : ScriptableObject, ISequence<TutorialQuestDefinitionSO>
    {
            public TutorialQuestDefinitionSO[] tutorial;

            public TutorialQuestDefinitionSO[] GetSequence()
            {
                return tutorial;
            }
    }
}