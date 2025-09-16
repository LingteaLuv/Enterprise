
    using System;
    using System.Text;
    using UnityEngine;

    public static class QuestKeys
    {
        public static string Compose(KeyFunc keyFunc, TypedEnumKey key)
        {
            StringBuilder sb = new StringBuilder();
            switch (keyFunc)
            {
                case KeyFunc.StageClear:
                    return stageClearkey;
                default:
                    return sb.Append(keyFunc.ToString()).Append(":").Append(key.ToKeyString()).ToString();
                
            }
        }
        private static readonly string stageClearkey = "stageClear";
        
        public static string Kill(MonsterId enemyId) => $"Kill:{enemyId.ToString()}";
        public static string Collect(MoneyId itemId) => $"Collect:{itemId.ToString()}";
        public static string GachaPull(ItemType gachaId) => $"GachaPull:{gachaId}";
        public static string LevelUp(ItemType LevelId) => $"LevelUP:{LevelId}";
        public static string Upgrade(UpgradeType UpgradeId) => $"Upgrade:{UpgradeId}";
        public static string Active(ActiveType activeType) => $"Active:{activeType}";
        public static string Achieve(string achieveType) => $"Achieve:{achieveType}";
        public static string StageClear() => stageClearkey;
        public static string DeckComposition(DeckSynergy deckSynergy) => $"DeckComposition:{deckSynergy}";
        public static string RankUp(ItemType rankUp) => $"RankUp:{rankUp}";
    }

    public enum KeyFunc
    {
        Kill, Collect, GachaPull, LevelUp, Upgrade, Active, Achieve, StageClear, Deck, RankUp
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
        Character, Equipment, Relic, All
    }

    public enum UpgradeType
    {
        Atk, Def, Hp
    }

    public enum ActiveType
    {
        Skill, AutoCombat, Dungeon
    }

    public enum DeckSynergy
    {
        Bosun, Sailor, Cook
    }

    public enum RankUp
    {
        Character, Equipment, Relic, All
    }

    
    public enum KeyKind { Monster, Money, ItemType, UpgradeType, Active, Achieve, DeckSynergy}

    [Serializable]
    public struct TypedEnumKey
    {
        public KeyKind kind;
        public MonsterId monster;
        public MoneyId money;
        public ItemType itemType;
        public UpgradeType upgrade;
        public ActiveType active;
        public DeckSynergy deck;

        [TextArea(2, 4)] public string achieveText;
        
        public string ToKeyString()
        {
            switch (kind)
            {
                case KeyKind.Monster: return monster.ToString();
                case KeyKind.Money: return money.ToString();
                case KeyKind.ItemType: return itemType.ToString();
                case KeyKind.UpgradeType: return upgrade.ToString();
                case KeyKind.Active: return active.ToString();
                case KeyKind.Achieve: return achieveText.Trim();
                case KeyKind.DeckSynergy: return deck.ToString();
                default: return string.Empty;
            }
        }
    }

