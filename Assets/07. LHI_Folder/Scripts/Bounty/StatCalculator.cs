using UnityEngine;

/// <summary>
/// 캐릭터 현상금(전투력) 계산을 담당하는 정적 클래스
/// 각 역할별로 스탯에 다른 가중치를 적용하여 전투력을 산출합니다.
/// </summary>
public static class StatCalculator
{
    #region 스탯별 가중치 설정
    // 역할별 공격력 가중치
    private const float CAPTAIN_ATTACK_WEIGHT = 1.5f;
    private const float DECKHAND_ATTACK_WEIGHT = 0.2f;
    private const float SAILOR_ATTACK_WEIGHT = 1.2f;
    private const float COOK_ATTACK_WEIGHT = 1.0f;

    // 역할별 체력 가중치
    private const float CAPTAIN_HEALTH_WEIGHT = 0.3f;
    private const float DECKHAND_HEALTH_WEIGHT = 0.4f;
    private const float SAILOR_HEALTH_WEIGHT = 0.3f;
    private const float COOK_HEALTH_WEIGHT = 0.4f;

    // 역할별 방어력 가중치
    private const float CAPTAIN_DEFENSE_WEIGHT = 0.3f;
    private const float DECKHAND_DEFENSE_WEIGHT = 0.4f;
    private const float SAILOR_DEFENSE_WEIGHT = 0.2f;
    private const float COOK_DEFENSE_WEIGHT = 0.3f;

    // 역할별 치명타 가중치
    private const float CAPTAIN_CRIT_WEIGHT = 0.8f;
    private const float DECKHAND_CRIT_WEIGHT = 0.1f;
    private const float SAILOR_CRIT_WEIGHT = 0.8f;
    private const float COOK_CRIT_WEIGHT = 0.3f;

    // 공격속도 가중치 (모든 역할 동일)
    private const float ATTACK_SPEED_MULTIPLIER = 1000f;
    #endregion

    /// <summary>
    /// 캐릭터의 최종 전투력(현상금)을 계산합니다.
    /// PlayerCharacterData.RecaculateStats()에서 호출됩니다.
    /// </summary>
    /// <param name="characterData">계산할 캐릭터 데이터</param>
    /// <returns>계산된 전투력 (float)</returns>
    public static float ComputeFinalPower(PlayerCharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogError("StatCalculator: 캐릭터 데이터가 null입니다.");
            return 0f;
        }

        CrewRole role = characterData.characterdata.crewRole;

        // 각 스탯별 전투력 계산
        float attackPower = CalculateAttackPower(GetStat(characterData, Stat.Attack), role);
        float healthPower = CalculateHealthPower(GetStat(characterData, Stat.Health), role);
        float defensePower = CalculateDefensePower(GetStat(characterData, Stat.Defense), role);
        float critPower = CalculateCriticalPower(
            GetStat(characterData, Stat.CritChance),
            GetStat(characterData, Stat.CritDamage),
            role
        );
        float speedPower = CalculateSpeedPower(GetStat(characterData, Stat.AttackSpeed), role);

        // 총 전투력 합산
        float totalPower = attackPower + healthPower + defensePower + critPower + speedPower;

        Debug.Log($"[전투력 계산] {characterData.characterdata.characterName}: " +
                  $"공격:{attackPower:F0} + 체력:{healthPower:F0} + 방어:{defensePower:F0} + " +
                  $"치명타:{critPower:F0} + 속도:{speedPower:F0} = {totalPower:F0} ");

        return totalPower;
    }

    #region 유틸리티 메소드
    /// <summary>
    /// 캐릭터의 특정 스탯 값을 가져옵니다.
    /// </summary>
    public static float GetStat(PlayerCharacterData character, Stat statName)
    {
        if (character.finalStats.ContainsKey(statName))
            return character.finalStats[statName];
        return 0f;
    }
    #endregion

    #region 스탯별 전투력 계산 메소드
    /// <summary>
    /// 공격력에 의한 전투력을 계산합니다.
    /// </summary>
    private static float CalculateAttackPower(float attack, CrewRole role)
    {
        float weight = role switch
        {
            CrewRole.Captain => CAPTAIN_ATTACK_WEIGHT,
            CrewRole.Deckhand => DECKHAND_ATTACK_WEIGHT,
            CrewRole.Sailor => SAILOR_ATTACK_WEIGHT,
            CrewRole.Cook => COOK_ATTACK_WEIGHT,
            _ => 1.0f
        };

        return attack * weight;
    }

    /// <summary>
    /// 체력에 의한 전투력을 계산합니다.
    /// </summary>
    private static float CalculateHealthPower(float health, CrewRole role)
    {
        float weight = role switch
        {
            CrewRole.Captain => CAPTAIN_HEALTH_WEIGHT,
            CrewRole.Deckhand => DECKHAND_HEALTH_WEIGHT,
            CrewRole.Sailor => SAILOR_HEALTH_WEIGHT,
            CrewRole.Cook => COOK_HEALTH_WEIGHT,
            _ => 0.3f
        };

        return health * weight;
    }

    /// <summary>
    /// 방어력에 의한 전투력을 계산합니다.
    /// </summary>
    private static float CalculateDefensePower(float defense, CrewRole role)
    {
        float weight = role switch
        {
            CrewRole.Captain => CAPTAIN_DEFENSE_WEIGHT,
            CrewRole.Deckhand => DECKHAND_DEFENSE_WEIGHT,
            CrewRole.Sailor => SAILOR_DEFENSE_WEIGHT,
            CrewRole.Cook => COOK_DEFENSE_WEIGHT,
            _ => 0.3f
        };

        return defense * weight;
    }

    /// <summary>
    /// 치명타 관련 스탯에 의한 전투력을 계산합니다.
    /// </summary>
    private static float CalculateCriticalPower(float critChance, float critDamage, CrewRole role)
    {
        float weight = role switch
        {
            CrewRole.Captain => CAPTAIN_CRIT_WEIGHT,
            CrewRole.Deckhand => DECKHAND_CRIT_WEIGHT,
            CrewRole.Sailor => SAILOR_CRIT_WEIGHT,
            CrewRole.Cook => COOK_CRIT_WEIGHT,
            _ => 0.5f
        };

        // 치명타 확률과 피해량을 조합하여 계산
        float critPower = (critChance * critDamage / 200f) * weight;
        return critPower;
    }

    /// <summary>
    /// 공격속도에 의한 전투력을 계산합니다.
    /// </summary>
    private static float CalculateSpeedPower(float attackSpeed, CrewRole role)
    {
        // 모든 역할에 동일한 가중치 적용
        return attackSpeed * ATTACK_SPEED_MULTIPLIER;
    }
    #endregion
}
