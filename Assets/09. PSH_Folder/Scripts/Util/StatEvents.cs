using System;
using System.Numerics;
using UnityEngine;

public static class StatEvents
{
    /// <summary>
    /// 개별 캐릭터의 전투력 변경 이벤트 (해당 캐릭터, 이전 전투력, 변경 후 전투력)
    /// </summary>
    public static event Action<PlayerCharacterData, BigInteger, BigInteger> OnCharacterBattlePowerChanged;

    /// <summary>
    /// 팀 전투력 변경 이벤트 (이전 팀 전투력, 새로운 팀 전투력)
    /// </summary>
    public static event Action<BigInteger, BigInteger> OnTeamBattlePowerChanged;

    /// <summary>
    /// 스탯 재계산 필요 이벤트
    /// </summary>
    public static event Action OnStatsNeedRecalculation;

    /// <summary>
    /// 개별 캐릭터의 전투력 변경 이벤트 호출
    /// </summary>
    public static void RaiseCharacterBattlePowerChanged(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        OnCharacterBattlePowerChanged?.Invoke(character, oldPower, newPower);
    }

    /// <summary>
    /// 팀 전투력 변경 이벤트 호출
    /// </summary>
    public static void RaiseTeamBattlePowerChanged(BigInteger oldTeamPower, BigInteger newTeamPower)
    {
        OnTeamBattlePowerChanged?.Invoke(oldTeamPower, newTeamPower);
        Debug.Log($"팀 전투력 변경: {oldTeamPower} -> {newTeamPower}");
    }

    /// <summary>
    /// 스탯 재계산 필요 이벤트 호출
    /// </summary>
    public static void RaiseStatsNeedRecalculation()
    {
        Debug.Log("스탯 재계산 필요 이벤트 발생!");
        OnStatsNeedRecalculation?.Invoke();
    }
}