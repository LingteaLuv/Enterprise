using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Reward")]
    public class QuestRewardSO : ScriptableObject
    {
        [System.Serializable]
        public struct RewardEntry
        {
            public string RewardType;
            public int Amount;
        }
    
        public RewardEntry[] Rewards;

        public void AddReward(IRewardGiver giver)
        {
            foreach (var reward in Rewards)
            {
                giver.Give(reward.RewardType, reward.Amount);
            }
        }
    }
    
    public interface IRewardGiver
    {
        public void Give(string type, int amount);
    }
}