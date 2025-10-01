using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    // 퀘스트 인스턴스
    // 현재 진행중인 퀘스트를 관리
    public abstract class QuestInstance
    {        
        // 퀘스트 고유 번호
        public string QuestId
        {
            get
            {
                if (this is GeneralQuestInstance general) return general.GeneralQuestId;
                else if(this is TemporaryInstance temporary) return temporary.TemporaryQuestId;
                else
                {
                    Debug.LogError("QuestId 잘못된 접근");
                    return "";
                }
            }
        }

        // 퀘스트 상태
        public QuestState_Enum QuestState = QuestState_Enum.BeforeActive;
        // 현재 퀘스트 목표 달성 정도
        public int CurrentGoalCount { get; private set; }
        // 현재 일반 퀘스트가 몇번째 퀘스트인지 
        public int GeneralQuestCount;
        // 만약 스테이지 클리어 미션이라면 몇 스테이지를 깨야하는지
        public int needToClearStage;
        public string stageClearMission = "";



        /// <summary>
        /// 현재 퀘스트가 완료 상태인지 반환
        /// </summary>
        /// <returns></returns>
        public abstract bool IsCompleted();
        
        public abstract bool IsOnce();

        public void GoalCountAdjust(int count)
        {
            CurrentGoalCount = count;
        }
        public void GoalCountAdjust()
        {
            CurrentGoalCount++;
        }
        public void GoalCountInit()
        {
            CurrentGoalCount = 0;
        }
        
        public void ForceComplete()
        {
            CurrentGoalCount = int.MaxValue;
        }
    }
}