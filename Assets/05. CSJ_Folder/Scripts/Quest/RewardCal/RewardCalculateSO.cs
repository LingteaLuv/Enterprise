using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.RewardCal
{
    public abstract class RewardCalculateSO : ScriptableObject
    {
        public abstract int CalculateReward(QuestDefinitionSO def, QuestInstance inst, int value, RewardCtx ctx = default);


        
    }
    
    /// <summary>
    /// 추후 필요하다면 해당 내용 정의
    /// </summary>
    public struct RewardCtx
    {
            
    };
}