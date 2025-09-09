using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/Temporary")]
    public class TemporaryQuestDefinitionSO : QuestDefinitionSO
    {
        public string TempoQuestContent;
        
        [Header("타입")]
        // quest 구분
        // 일반/시간제
        [SerializeField] public int DailyQuestId;
        [SerializeField] public int WeeklyQuestId;
 


        [Header("일일 퀘스트 요구량")]
        public int[] dailyCountArray;
        [Header("주간 퀘스트 요구량")]
        public int[] weeklyCountArray;
        
        public TempoGoalDefinitionSO TempoGoal;
        
        public int QuestCount => dailyCountArray.Length + weeklyCountArray.Length;
        

        /// <summary>
        /// Index를 기반하여 난이도 반환 / 0부터 시작
        /// </summary>
        /// <param name="questIndex"></param>
        /// <returns></returns>
        public QuestDifficult_Enum DifficultByIndex(int questIndex)
        {
            if (questIndex < 0 || QuestCount <= questIndex)
            {
                Debug.LogError($"Temporary 범위 계산 이상, 접근 QuestIndex : {questIndex}");
                return QuestDifficult_Enum.Hard;
            }

            if (isDaily(questIndex))
            {
                return (QuestDifficult_Enum)Mathf.Clamp(questIndex, 0, (int)QuestDifficult_Enum.Hard);
            }
            return (QuestDifficult_Enum)Mathf.Clamp(questIndex - dailyCountArray.Length, 0, (int)QuestDifficult_Enum.Hard);
        }

        public bool isDaily(int questIndex) => questIndex < dailyCountArray.Length;

        public int GetQuestDemand(int questIndex)
        {
            QuestDifficult_Enum difficult = DifficultByIndex(questIndex);

            if (isDaily(questIndex))
            {
                return dailyCountArray[(int)difficult];
            }
            return weeklyCountArray[(int)difficult];
        }

        public int GetQuestKeyByType(QuestType_Enum type)
        {
            return type == QuestType_Enum.Daily ? DailyQuestId : WeeklyQuestId;
        }
    }
}