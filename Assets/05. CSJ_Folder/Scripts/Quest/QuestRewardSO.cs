using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Reward")]
    public class QuestRewardSO : ScriptableObject
    {
        [System.Serializable]
        // 리워드 물품과 숫자 구조체 묶음
        public struct RewardEntry
        {
            public Sprite RewardIcon;
            public string RewardType;
            public int Amount;
        }
    
        public RewardEntry Reward;

        // 해당하는 보상을 획득 처리
        public void AddReward(IRewardGiver giver)
        {
            giver.Give(Reward);
        }
    }
    
    public interface IRewardGiver
    {
        public void Give(QuestRewardSO.RewardEntry rewardEntry);
    }
}