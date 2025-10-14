using System;
using System.Collections.Generic;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.SO.Reward
{
    [CreateAssetMenu (menuName = "Quest/RewardBundle")]
    public class RewardBundleSO : ScriptableObject
    {
        [Serializable]
        public struct Item
        {
            public Sprite icon;            // 표시 아이콘
            public string displayName;     // 표기 이름
            public QuestRewardType_Enum rewardType;
            public int amount;             // 최종 수치
        }

        public List<Item> items = new();

        // (선택) 같은 타입 합치기
        public RewardBundleSO MergeSameTypes()
        {
            var map = new Dictionary<QuestRewardType_Enum, Item>();
            foreach (var it in items)
            {
                if (!map.TryGetValue(it.rewardType, out var acc))
                    map[it.rewardType] = it;
                else
                {
                    acc.amount += it.amount;
                    map[it.rewardType] = acc;
                }
            }
            var clone = Instantiate(this);
            clone.items = new List<Item>(map.Values);
            return clone;
        }
    }

    public static class RewardBundleSOExtensions
    {
        public static RewardBundleSO FromEntries(IEnumerable<QuestRewardSO.RewardEntry> entries)
        {
            var so = ScriptableObject.CreateInstance<RewardBundleSO>();
            foreach (var e in entries)
            {
                so.items.Add(new RewardBundleSO.Item{
                    icon = e.RewardIcon,
                    displayName = e.RewardType, // 필요시 로컬라이즈
                    rewardType = e.rewardType,
                    amount = e.GetAmount()
                });
            }
            return so;
        }
    }
}