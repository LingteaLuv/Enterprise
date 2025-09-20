using System;

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

        internal void ClearCodex(int value, CodexStd_Enum std)
        {
            if (std == CodexStd_Enum.Level) 
                AdjustCritDamage(value, true);
            else if (std == CodexStd_Enum.Rank)
                AdjustCritChance(value, true);
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