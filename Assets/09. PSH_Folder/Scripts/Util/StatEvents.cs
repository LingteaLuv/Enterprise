using System;
using System.Numerics;
using UnityEngine;

public static class StatEvents
{
    /// <summary>
    /// 전투력 변경 이벤트 (이전 전투력, 변경 후 전투력)
    /// </summary>
    public static event Action<BigInteger, BigInteger> OnBattlePowerChanged;

    /// <summary>
    /// 전투력 변경 호출
    /// </summary>
    public static void RaiseBattlePowerChanged(BigInteger oldPower, BigInteger newPower)
    {
        OnBattlePowerChanged?.Invoke(oldPower, newPower);
    }
}
