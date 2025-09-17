using System.Text;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TemporaryInstance : QuestInstance
    {
        public readonly string temporaryQuestContent;
            
        public QuestType_Enum QuestType { get; set; }
        public readonly QuestDifficult_Enum QuestDifficult;
        public readonly int DemandedGoalCount;
        public readonly string TemporaryQuestId;
        public TemporaryQuestDefinitionSO Def;
        public bool IsDaily => QuestType == QuestType_Enum.Daily;
        
        private readonly StringBuilder _sb = new StringBuilder();

        public TemporaryInstance(TemporaryQuestDefinitionSO _def, QuestType_Enum _type, int _questIndex)
        {
            Def = _def;
            QuestType = _type;
            if (QuestType == QuestType_Enum.Daily)
                TemporaryQuestId = (_def.GetQuestKeyByType(QuestType) + _questIndex).ToString();
            else
            {
                TemporaryQuestId = (_def.GetQuestKeyByType(QuestType) + _questIndex - _def.dailyCountArray.Length).ToString();
            }
            QuestDifficult = _def.DifficultByIndex(_questIndex);
            DemandedGoalCount = _def.GetQuestDemand(_questIndex);
            QuestState = QuestState_Enum.Active;

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

        public override bool IsOnce()
        {
            return false;
        }


    }
}