using System;
using System.Collections.Generic; // Dictionary를 사용하기 위해 추가
using UnityEngine; // Debug.LogWarning을 사용하기 위해 추가

public static class StatManager
{
    // 각 스탯의 레벨당 성장치를 저장하는 딕셔너리
    private static Dictionary<Stat, float> statGrowthRates;

    // static 생성자: 클래스가 처음 로드될 때 한 번만 실행됩니다.
    static StatManager()
    {
        InitializeStatGrowthRates();
    }

    // 스탯 성장치 초기화 함수
    private static void InitializeStatGrowthRates()
    {
        statGrowthRates = new Dictionary<Stat, float>
        {
            { Stat.Health, 100f },         // 체력: 레벨당 100 증가
            { Stat.Attack, 20f },     // 공격력: 레벨당 20 증가
            { Stat.Defense, 5f },     // 방어력: 레벨당 5 증가
            //{ "critChance", 0.005f },   // 치명타 확률: 레벨당 0.5% 증가 (0.005 = 0.5%)
            //{ "critDamage", 0.01f },    // 치명타 피해: 레벨당 1% 증가 (0.01 = 1%)
            //{ "attackSpeed", 0.001f }   // 공격 속도: 레벨당 0.1% 증가
        };
    }

    /// <summary>
    /// 특정 스탯의 현재 레벨에 해당하는 최종 능력치를 계산합니다。
    /// StatData.value는 기본 스탯(Lv.1) 값으로 사용됩니다.
    /// </summary>
    /// <param name="stat">계산할 스탯의 원본 데이터 (StatData.value는 기본값)</param>
    /// <param name="level">현재 스탯 레벨</param>
    /// <returns>계산된 최종 능력치 (float)</returns>
    public static float CalculateStatValue(StatData stat, int level)
    {
        if (level <= 0) level = 1; // 레벨은 최소 1

        float baseStat = stat.value; // StatData.value는 기본 스탯 값 (Lv.1)
        float growthPerLevel = 0f;

        // 해당 스탯의 성장치를 딕셔너리에서 가져옵니다.
        if (statGrowthRates.TryGetValue(stat.statName, out growthPerLevel))
        {
            // 공식: 기본 스탯 + (현재 레벨 - 1) * 레벨당 성장치
            return baseStat + (level - 1) * growthPerLevel;
        }
        else
        {
            // 정의되지 않은 스탯의 경우 경고를 출력하고 기본 스탯 값을 반환합니다.
            Debug.LogWarning($"StatManager: 스탯 '{stat.statName}'에 대한 성장치가 정의되지 않았습니다. 기본 스탯 값을 반환합니다.");
            return baseStat;
        }
    }
}