using System;

// static 클래스로 변경하여, 어디서든 접근 가능한 스탯 계산기로 만듭니다.
public static class StatManager
{
    /// <summary>
    /// 특정 스탯의 현재 레벨에 해당하는 최종 능력치를 계산합니다.
    /// (StatData.value가 이미 최종 스탯 값이라고 가정)
    /// </summary>
    /// <param name="stat">계산할 스탯의 원본 데이터 (StatData.value)</param>
    /// <param name="level">현재 스탯 레벨 (현재 StatData.value가 최종값이면 이 파라미터는 사용되지 않음)</param>
    /// <returns>계산된 최종 능력치 (float)</returns>
    public static float CalculateStatValue(StatData stat, int level)
    {
        // StatData.value가 이미 최종 스탯 값이라면, 레벨은 계산에 사용되지 않습니다.
        // 만약 StatData.value가 '기본 스탯'이고 레벨에 따른 성장이 필요하다면,
        // StatData에 '성장치' 필드를 추가하고 여기에 계산 로직을 구현해야 합니다.
        return stat.value;
    }

    /// <summary>
    /// 특정 스탯의 다음 레벨 업에 필요한 비용을 계산합니다.
    /// (현재 StatData 클래스에는 비용 정보가 없으므로 이 함수는 작동할 수 없습니다.)
    /// </summary>
    /// <param name="stat">계산할 스탯의 원본 데이터 (현재 비용 정보 없음)</param>
    /// <param name="currentLevel">현재 스탯 레벨</param>
    /// <returns>계산된 업그레이드 비용 (현재 구현 불가)</returns>
    public static float CalculateUpgradeCost(StatData stat, int currentLevel)
    {
        // StatData 클래스에 비용 관련 필드(예: baseCost, costIncreaseRatio)가 없으므로
        // 이 함수는 현재 구현할 수 없습니다.
        // 비용 계산이 필요하다면 StatData에 비용 관련 필드를 추가하거나,
        // 별도의 비용 데이터 클래스를 정의해야 합니다.
        UnityEngine.Debug.LogError("StatManager: CalculateUpgradeCost 함수는 StatData에 비용 정보가 없어 계산할 수 없습니다.");
        return 0f; // 계산 불가 시 0 반환
    }
}