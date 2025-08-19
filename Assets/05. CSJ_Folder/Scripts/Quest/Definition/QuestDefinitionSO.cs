using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/QuestDefault")]
    public class QuestDefinitionSO : ScriptableObject
    {
        [Header("퀘스트 구분자")] 
        // quest 고유 id 값 
        public string questId;
        // quest 이름
        public string questName;
        
        [Header("타입")]
        // quest 구분
        // 일반/시간제
        public QuestType_Enum QuestType;
        // 일반 퀘스트의 종류 (아니라면 None)
        public GeneralType_Enum GeneralType = GeneralType_Enum.None;
        // 반복 퀘스트인지 (루틴 퀘스트거나, 시간제 퀘스트)
        public RepeatType_Enum RepeatType;
        // 추후 퀘스트 내용에 따라서 진짜 반복 퀘스트로 내용을 변경할 수도 있음

        [Header("퀘스트 내용")] 
        // 퀘스트의 목표 (목표, 텍스트, 목표 숫자)
        public GoalDefinitionSO[] Goals;
        
        // 퀘스트의 보상
        public QuestRewardSO Reward;

        // 프로퍼티
        /// <summary>
        /// 일반 퀘스트인지 반환
        /// </summary>
        public bool isGeneral => QuestType == QuestType_Enum.General;
    }
}