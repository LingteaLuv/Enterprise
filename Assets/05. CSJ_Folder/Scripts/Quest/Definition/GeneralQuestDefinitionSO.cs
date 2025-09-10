using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/General/Default")]
    public class GeneralQuestDefinitionSO : QuestDefinitionSO
    {
        // 퀘스트의 목표 (목표, 텍스트, 목표 숫자)
        public GoalDefinitionSO Goal;
        
        [Header("퀘스트 구분자")] 
        // quest 고유 id 값 
        public string questId;

        
        [Header("일반 퀘스트 구분")]
        // 일반 퀘스트의 종류 (아니라면 None)
        public GeneralType_Enum GeneralType = GeneralType_Enum.None;
        // 반복 퀘스트인지 (루틴 퀘스트거나, 시간제 퀘스트)
        public RepeatType_Enum RepeatType;
        // 추후 퀘스트 내용에 따라서 진짜 반복 퀘스트로 내용을 변경할 수도 있음
    }
}