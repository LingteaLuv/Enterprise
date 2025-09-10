using System.Text;
using _05._CSJ_Folder.Scripts.Quest.Definition;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TemporaryInstance :QuestInstance
    {
        public string temporaryQuestContent;
            
        public QuestType_Enum QuestType { get; set; }
        public readonly QuestDifficult_Enum QuestDifficult;
        public int DemandedGoalCount = 0;
        public readonly string TemporaryQuestId;
        public TemporaryQuestDefinitionSO Def;
        public bool IsDaily => QuestType == QuestType_Enum.Daily;
        
        private readonly StringBuilder _sb = new StringBuilder();

        public TemporaryInstance(TemporaryQuestDefinitionSO _def, QuestType_Enum _type, int _questIndex)
        {
            Def = _def;
            QuestType = _type;
            TemporaryQuestId = (_def.GetQuestKeyByType(QuestType) + _questIndex).ToString();
            QuestDifficult = _def.DifficultByIndex(_questIndex);
            DemandedGoalCount = _def.GetQuestDemand(_questIndex);

            _sb.Append(_def.questName);
            _sb.Append(DemandedGoalCount.ToString());
            _sb.Append(_def.TempoQuestContent);
            temporaryQuestContent = _sb.ToString();
        }

        public override bool IsCompleted()
        {
            // 목표보다 현재 Count수가 작다면
            if (DemandedGoalCount > CurrentGoalCount)
            {
                return false;
            }
            return true;
        }


    }
}