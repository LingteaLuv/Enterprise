using _05._CSJ_Folder.Scripts.Quest.Definition;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class GeneralQuestInstance : QuestInstance
    {
        public string GeneralQuestId;
        
        private GeneralQuestDefinitionSO _def;

        public GeneralQuestInstance(string questId, GeneralQuestDefinitionSO def)
        {
            GeneralQuestId = QuestId;
            _def = def;
        }
        

        public override bool IsCompleted()
        {
            int goalRequiredCount = _def.Goal.RequireCount;
            // 목표보다 현재 Count수가 작다면
            if (goalRequiredCount > CurrentGoalCount)
            {
                return false;
            }
            return true;
        }
    }
}