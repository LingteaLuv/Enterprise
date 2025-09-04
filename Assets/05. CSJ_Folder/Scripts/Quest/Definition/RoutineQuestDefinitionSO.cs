using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/General/Routine")]
    public class RoutineQuestDefinitionSO : GeneralQuestDefinitionSO
    {
        [Header("루틴 퀘스트 : 루틴에 포함되기 위한 클리어 수")]
        // 퀘스트 루틴에 포함되기 위한 최소 퀘스트 클리어 수
        public int RequiredQuestClearMin;
        // 사이클 1차 확인
        public Parity_Enum Parity;
    }
}