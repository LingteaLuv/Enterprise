using System.Numerics;
using UnityEngine;

public static class StatCalculator
{
    /// <summary>
    /// 플레이어 캐릭터 데이터 기반 최종 전투력 계산
    /// 장비, 유물, 레벨, 성급 등 모든 보정 포함
    /// </summary>
    public static float ComputeFinalPower(PlayerCharacterData player)
    {
        // 식은 임시로 스탯 다 더하는 식으로 함 세부 전투력 공식에 맞게 변경해야함
        float total = 0;
        foreach (var item in player.finalStats)
        {
            total += GetStat(player, item.Key);
            Debug.Log($"{item.Key} 스탯, 전투력에 {GetStat(player, item.Key)}를 더함");
        }
        return total;
    }
    public static float GetStat(PlayerCharacterData character, Stat statName)
    {
        if (character.finalStats.ContainsKey(statName))
            return character.finalStats[statName];
        return 0f;
    }
}
