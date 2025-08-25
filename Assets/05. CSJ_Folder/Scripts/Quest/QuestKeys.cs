
    using System;
    using UnityEngine;

    public static class QuestKeys
    {
        private static readonly string stageClearkey = "stageClear";
        
        public static string Kill(MonsterId enemyId) => $"Kill:{enemyId.ToString()}";
        public static string Collect(MoneyId itemId) => $"Collect:{itemId.ToString()}";
        public static string GachaPull(ItemType gachaId) => $"Gacha:{gachaId}";
        public static string LevelUp(ItemType LevelId) => $"LevelUP:{LevelId}";
        public static string Upgrade(UpgradeType UpgradeId) => $"Upgrade:{UpgradeId}";
        public static string Achieve(string achieveType) => $"Achieve:{achieveType}";
        public static string StageClear() => stageClearkey;
    }

    public enum KeyFunc
    {
        Kill, Collect, GachaPull, LevelUp, Upgrade, Achieve, StageClear
    }

    public enum MonsterId
    {
        Boss, General, All
    }

    public enum MoneyId 
    {
        Diamond, Gold
    }

    public enum ItemType
    {
        Character, Equipment, Relic
    }

    public enum UpgradeType
    {
        Atk, Def, Hp
    }

    
    public enum KeyKind { Monster, Money, ItemType, UpgradeType, Achieve }

    [Serializable]
    public struct TypedEnumKey
    {
        public KeyKind kind;

        public MonsterId monster;
        public MoneyId money;
        public ItemType itemType;
        public UpgradeType upgrade;

        [TextArea(2, 4)] public string achieveText;
        
        public string ToKeyString()
        {
            switch (kind)
            {
                case KeyKind.Monster: return monster.ToString();
                case KeyKind.Money: return money.ToString();
                case KeyKind.ItemType: return itemType.ToString();
                case KeyKind.UpgradeType: return upgrade.ToString();
                case KeyKind.Achieve: return achieveText.Trim();
                default: return string.Empty;
            }
        }
    }

