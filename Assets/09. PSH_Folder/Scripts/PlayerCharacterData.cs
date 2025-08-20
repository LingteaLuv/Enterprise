using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class PlayerCharacterData
{
    [Header("캐릭터")]
    public CharacterData characterdata; // 캐릭터 원본 데이터
    public int characterLevel;
    public int stars; // 성급

    // Key: 스탯 이름(string), Value: 현재 스탯 양 캐릭터의 레벨에 따라서 변경
    public Dictionary<string, float> characterStats = new Dictionary<string, float>();

    //[Header("장비")]
    // 장비 코드 정보만 저장

    //[Header("유물")]
    // 적용되고 있는 유물 정보만 저장

    [Header("최종 스탯")]
    public Dictionary<string, float> finalStats = new Dictionary<string, float>();
    public BigInteger battlePower; // 전투력

    public PlayerCharacterData(CharacterData so)
    {
        characterdata = so;
        characterLevel = 1;
        stars = (int)so.rarity; // CharacterData의 rarity는 Enum이므로 int로 캐스팅

        characterStats = new Dictionary<string, float>();
        foreach (var stat in so.baseStats)
        {
            if (!characterStats.ContainsKey(stat.statName))
            {
                characterStats.Add(stat.statName, stat.value);
            }
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

        // 캐릭터 스탯 적용
        foreach (var stat in characterStats)
        {
            finalStats[stat.Key] = stat.Value;
        }

        // 기본 스탯 적용 
        if (BasicStatManager.Instance != null)
        {
            finalStats["health"] = finalStats.GetValueOrDefault("health", 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Health);
            finalStats["attackPower"] = finalStats.GetValueOrDefault("attackPower", 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Attack);
            finalStats["defensePower"] = finalStats.GetValueOrDefault("defensePower", 0) + BasicStatManager.Instance.GetStatValue(BasicStatType.Defense);
        }

        // 장비 스탯 적용
        // 유물 스탯 적용

        // --- 전투력 계산 시작 ---
        // StatCalculator.cs의 공식을 사용하여 전투력을 계산합니다.
        battlePower = (BigInteger)StatCalculator.ComputeFinalPower(this);
        // --- 전투력 계산 끝 ---


        // 전투력이 변경되었는지 확인하고 이벤트 호출
        if (oldPower != battlePower)
        {
            Debug.Log($"전투력 변경 감지: {oldPower} -> {battlePower}");
            StatEvents.RaiseBattlePowerChanged(oldPower, battlePower);
        }
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
