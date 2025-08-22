
    public static class QuestKeys
    {
        private static readonly string stageClearkey = "stageClear";
        
        public static string Kill(MonsterId enemyId) => $"kill:{enemyId.ToString()}";
        public static string Collect(ItemId itemId) => $"collect:{itemId.ToString()}";
        public static string GachaPull(gachaType gachaId) => $"gacha:{gachaId}";
        public static string StageClear() => stageClearkey;
    }

    public enum MonsterId
    {
        Boss, General, All
    }

    public enum ItemId
    {
        Diamond, Gold
    }

    public enum gachaType
    {
        Character, Equipment
    }
