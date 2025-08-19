using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/General/Routine")]
    public class RoutineQuestDefinitionSO : GeneralQuestDefinitionSO
    {
        // 퀘스트 루틴에 포함되기 위한 최소 스테이지 넘버
        public int RequiredStageClearMin;
        // 사이클 1차 확인
        public Parity_Enum Parity;
    }
}