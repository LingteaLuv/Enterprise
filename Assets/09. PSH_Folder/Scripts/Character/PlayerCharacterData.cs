using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public enum Stat
{
    Attack,
    Health,
    Defense,
    CritChance,
    CritDamage,
    AttackSpeed
}


[System.Serializable]
public class PlayerCharacterData
{
    [Header("캐릭터")]
    public CharacterData characterdata; // 캐릭터 원본 데이터
    public int characterLevel;
    public int stars; // 성급

    [Header("직업, 속성 아이콘")]
    public Sprite crewRoleIcon;
    public Sprite factionIcon;

    // Key: 스탯 이름(string), Value: 현재 스탯 양 캐릭터의 레벨에 따라서 변경
    public Dictionary<Stat, float> characterStats = new Dictionary<Stat, float>();

    //[Header("장비")]
    // 장비 코드 정보만 저장

    //[Header("유물")]
    // 적용되고 있는 유물 정보만 저장

    [Header("최종 스탯")]
    public Dictionary<Stat, float> finalStats = new Dictionary<Stat, float>();
    public BigInteger battlePower; // 전투력

    public PlayerCharacterData(CharacterData so)
    {
        characterdata = so;
        characterLevel = 1;
        stars = (int)so.rarity; // CharacterData의 rarity는 Enum이므로 int로 캐스팅

        // 아이콘
        crewRoleIcon = GetIcon(so.crewRole);
        factionIcon = GetIcon(so.faction);

        characterStats = new Dictionary<Stat, float>();
        foreach (var stat in so.baseStats)
        {
            characterStats[stat.statName] = stat.value;
        }
        // 최초 스탯 계산. BasicStatManager가 준비되기 전에 호출될 수 있으므로 battlePower만 초기화합니다.
        battlePower = 0;
    }

    /// <summary>
    /// 캐릭터에 적용되는 모든 스탯을 계산하고 전투력 변경 이벤트를 발생시킵니다.
    /// </summary>
    public void RecaculateStats()
    {
        BigInteger oldPower = battlePower; // 기존 전투력 저장

        finalStats.Clear();

        // 1. 캐릭터의 원본 데이터(SO)를 기반으로 현재 레벨에 맞는 스탯을 계산합니다.
        if (characterdata != null && characterdata.baseStats != null)
        {
            foreach (var baseStat in characterdata.baseStats)
            {
                // StatManager를 사용하여 레벨에 맞는 스탯 값을 계산합니다.
                float calculatedValue = StatManager.CalculateStatValue(baseStat, this.characterLevel);
                finalStats[baseStat.statName] = calculatedValue;
            }
        }

        // 2. 기본 스탯 적용 (BasicStatManager)
        if (BasicStatManager.Instance != null)
        {
            finalStats[Stat.Health] = finalStats.GetValueOrDefault(Stat.Health, 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Health);
            finalStats[Stat.Attack] = finalStats.GetValueOrDefault(Stat.Attack, 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Attack);
            finalStats[Stat.Defense] = finalStats.GetValueOrDefault(Stat.Defense, 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Defense);
        }

        // 3. 장비 및 유물 스탯 적용 (향후 확장)

        // --- 전투력 계산 시작 ---
        battlePower = (BigInteger)StatCalculator.ComputeFinalPower(this);
        // --- 전투력 계산 끝 ---

        // 전투력이 변경되었는지 확인하고 이벤트 호출
        if (oldPower != battlePower)
        {
            Debug.Log($"전투력 변경 감지: {oldPower} -> {battlePower}");
            StatEvents.RaiseCharacterBattlePowerChanged(this, oldPower, battlePower);
        }
    }

    /// <summary>
    /// 다음 레벨업에 필요한 비용을 계산하여 반환합니다.
    /// </summary>
    public BigInteger GetNextLevelUpCost()
    {
        // PlayerDataManager의 설정값을 가져와 현재 레벨에 맞는 비용을 계산합니다.
        double costDouble = (double)PlayerDataManager.Instance.baseLevelUpCost * System.Math.Pow(PlayerDataManager.Instance.levelUpCostIncreaseRatio, this.characterLevel - 1);
        return (BigInteger)costDouble;
    }

    /// <summary>
    /// 캐릭터 직업, 속성에 따라 아이콘을 업데이트합니다.
    /// </summary>
    public Sprite GetIcon(CrewRole role)
    {
        string path = $"PSHTest/Icon/{role}";
        return Resources.Load<Sprite>(path);
    }
    public Sprite GetIcon(Faction role)
    {
        string path = $"PSHTest/Icon/{role}";
        return Resources.Load<Sprite>(path);
    }
}

// Dictionary에 키가 없을 때 기본값을 반환하는 확장 메소드
public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
    {
        return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
}
