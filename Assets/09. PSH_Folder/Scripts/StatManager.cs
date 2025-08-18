
using System.Numerics;
using System;

// static 클래스로 변경하여, 어디서든 접근 가능한 스탯 계산기로 만듭니다.
public static class StatManager
{
    /// <summary>
    /// 특정 스탯의 현재 레벨에 해당하는 최종 능력치를 계산합니다.
    /// </summary>
    /// <param name="stat">계산할 스탯의 원본 데이터 (기본값, 성장치 등)</param>
    /// <param name="level">현재 스탯 레벨</param>
    /// <returns>계산된 최종 능력치</returns>
    public static System.Numerics.BigInteger CalculateStatValue(StatData stat, int level)
    {
        if (level <= 0) level = 1;
        return stat.BaseStat + (System.Numerics.BigInteger)(level - 1) * stat.StatIncreasePerLevel;
    }

    /// <summary>
    /// 특정 스탯의 다음 레벨 업에 필요한 비용을 계산합니다.
    /// </summary>
    /// <param name="stat">계산할 스탯의 원본 데이터 (기본 비용, 비용 증가율 등)</param>
    /// <param name="currentLevel">현재 스탯 레벨</param>
    /// <returns>계산된 업그레이드 비용</returns>
    public static System.Numerics.BigInteger CalculateUpgradeCost(StatData stat, int currentLevel)
    {
        if (currentLevel <= 0) currentLevel = 1;
        // StatData에 있는 baseCost의 Cost(BigInteger) 값을 기반으로 계산합니다.
        double cost = (double)stat.baseCost.Cost * Math.Pow(stat.costIncreaseRatio, currentLevel - 1);
        return (System.Numerics.BigInteger)cost;
    }
}
