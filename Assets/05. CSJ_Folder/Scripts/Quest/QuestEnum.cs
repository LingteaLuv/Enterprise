namespace _05._CSJ_Folder.Scripts.Quest
{
    // 퀘스트의 종류 구분
    public enum QuestType_Enum
    {
        General, Daily, Weekly
    }
    
    // 퀘스트의 반복 가능성
    public enum RepeatType_Enum
    {
        Once, Repeatable
    }

    // 일반 퀘스트 구분
    public enum GeneralType_Enum
    {
        None, Tutorial, Routine, StageClear
    }

    // 일반 퀘스트의 속한 사이클
    public enum Parity_Enum
    {
        Any, Odd, Even
    }

    // 현재 퀘스트의 상태
    public enum QuestState_Enum
    {
        BeforeActive, Completed, Active, Received
    }

    public enum QuestDifficult_Enum
    {
        Easy, Normal, Hard,
    }
    
    public enum QuestRewardType_Enum
    {
        Gem, Exp, Gold, CrewTicket, EquipmentTicket, CrewEnchantStone, 
    }

    public enum TutorialStepType
    {
        Dialogue, Highlight, WaitSignal, CompleteQuest, ClaimReward,
    }
}