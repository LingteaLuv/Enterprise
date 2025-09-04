using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    [CreateAssetMenu (menuName = "Quest/Sequence/Routine")]
    public class RoutineSequenceSO : ScriptableObject, ISequence<RoutineQuestDefinitionSO>
    {
        // 사이클 2차 확인
        // 홀수 사이클
        public RoutineQuestDefinitionSO[] OddRoutine;
        // 짝수 사이클
        public RoutineQuestDefinitionSO[] EvenRoutine;

        public RoutineQuestDefinitionSO[] GetSequence()
        {
            return OddRoutine;
        }

        public RoutineQuestDefinitionSO[] GetEvenRoutine()
        {
            return EvenRoutine;
        }
    }
}