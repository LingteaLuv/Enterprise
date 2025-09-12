using _05._CSJ_Folder.Scripts.Quest.Definition;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class GeneralQuestInstance : QuestInstance
    {
        public readonly string GeneralQuestId;
        
        public GeneralQuestDefinitionSO Def { get; }

        public GeneralQuestInstance(string questId, GeneralQuestDefinitionSO def)
        {
            GeneralQuestId = questId;
            Def = def;
        }
        

        public override bool IsCompleted()
        {
            int goalRequiredCount = Def.Goal.RequireCount;
            // 목표보다 현재 Count수가 작다면
            if (goalRequiredCount > CurrentGoalCount)
            {
                return false;
            }
            return true;
        }

        public override bool IsOnce()
        {
            if (Def.RepeatType == RepeatType_Enum.Once)
                return true;
            return false;
        }
    }
}