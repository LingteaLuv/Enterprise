using System;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.RewardCal;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Reward")]
    public class QuestRewardSO : ScriptableObject
    {

        [Serializable]
        // 리워드 물품과 숫자 구조체 묶음
        public struct RewardEntry
        {
            public Sprite RewardIcon;
            public QuestRewardType_Enum rewardType;

            public string RewardType => rewardType.ToString();
            [Tooltip("보상을 가변으로 할지에 대한 여부")] public bool IsVariableReward;
            [Tooltip("고정 보상량")] public int FixedAmount;

            [Tooltip("가변 보상의 가중치 / 해당 값을 기반으로 계산하여 보상 지급")]
            public int VariableAmount;

            [Tooltip("가변 보상에 사용할 계산식 / 미지정시 가중치 값을 보상으로 반환")]
            public RewardCalculateSO RewardCalculate;

            public int GetAmount(QuestDefinitionSO def, QuestInstance inst)
            {
                if (!IsVariableReward)
                    return FixedAmount;
                if (RewardCalculate is not null)
                    return RewardCalculate.CalculateReward(def, inst, VariableAmount);
                return VariableAmount;
            }

            public int GetAmount()
            {
                if (!IsVariableReward)
                    return FixedAmount;
                return VariableAmount;
            }
        }

        public RewardEntry Reward;


        public void AddReward(IRewardGiver giver, QuestDefinitionSO def, QuestInstance inst)
        {
            var entry = Reward;
            var amount = entry.GetAmount(def, inst);
            giver.Give(new RewardEntry
            {
                RewardIcon = entry.RewardIcon,
                rewardType = entry.rewardType,
                FixedAmount = amount,
                IsVariableReward = false
            });
        }
    }
    public interface IRewardGiver
    {
        public void Give(QuestRewardSO.RewardEntry entry);
    }
}