using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public enum CodexRewardType_Enum
    {
        CrewTicket, EquipmentTicket, RelicTicket, CrewInchantStone, Gem
    }

    public enum CodexStd_Enum
    {
        Level, Rank
    }

    public struct CodexData
    {
        public Faction Faction;
        public int LevelSum;
        public int RankSum;
        
        public int CritChance { get; private set; }
        public int CritDamage { get; private set; }


        public int ClearedLevelQuestCount;
        public int ClearedRankQuestCount;

        internal void AchieveCodex(CodexInstance inst, bool isInit)
        {
            switch (inst.StatType)
            {
                case CodexStat.CritChance:
                    AdjustCritChance(inst.StatAmount, true);
                    break;
                case CodexStat.CritDamage:
                    AdjustCritDamage(inst.StatAmount, true);
                    break;
            }
            if (isInit) return;
            switch (inst.CodexStd)
            {
                case CodexStd_Enum.Level:
                    ClearedLevelQuestCount++;
                    break;
                case CodexStd_Enum.Rank:
                    ClearedRankQuestCount++;
                    break;
            }
        }

        private void AdjustCritChance(int value, bool isAdd)
        {
            if (isAdd) CritChance += value;
            else CritChance -= value;
        }
        
        private void AdjustCritDamage(int value, bool isAdd)
        {
            if (isAdd) CritDamage += value;
            else CritDamage -= value;
        }

        internal void SetCodex(int chance, int damage)
        {
            CritChance = chance;
            CritDamage = damage;
        }

        internal void AdjustValueSum(CodexStd_Enum std, int value, bool isAdd)
        {
            switch (std)
            {
                case CodexStd_Enum.Level when isAdd:
                    LevelSum += value;
                    break;
                case CodexStd_Enum.Level:
                    LevelSum -= value;
                    break;
                case CodexStd_Enum.Rank when isAdd:
                    RankSum += value;
                    break;
                case CodexStd_Enum.Rank:
                    RankSum -= value;
                    break;
            }
        }
    }
}