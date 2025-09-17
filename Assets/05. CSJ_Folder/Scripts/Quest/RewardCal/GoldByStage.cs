using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.RewardCal
{
    [CreateAssetMenu(menuName = "Quest/Calculate/ReturnByStage")]
    public class ReturnByStage : RewardCalculateSO
    {
        public override int CalculateReward(QuestDefinitionSO def, QuestInstance inst, int value, RewardCtx ctx = default)
        {
            int stage = GlobalStageManager.Instance.CurrentStageIndex.Value;
            
            //TODO: 추후 계산식 수정
            return stage * value;
        }
    }
}