using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    [CreateAssetMenu(menuName = "Codex/Reward")]
    public class CodexRewardSO : ScriptableObject
    {
        [Serializable]
        public struct CodexRewardEntry
        {
            public Sprite RewardIcon;
            public CodexRewardType_Enum rewardType;
            
            public string RewardType => rewardType.ToString();
            public int amount;
        }
        
        public CodexRewardEntry[] RewardContents;
        
        public void AddReward(ICodexRewardGiver giver)
        {
            foreach (var reward in RewardContents)
            {
                giver.Give(reward);
            }
        }
    }
    
    public interface ICodexRewardGiver
    {
        public void Give(CodexRewardSO.CodexRewardEntry entry);
    }
}