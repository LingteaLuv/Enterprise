using UnityEngine;

public class Bounty : MonoBehaviour
{
    // 편성된 캐릭터의 스탯 변경시 재계산

    //Attack,
    //Health,
    //Defense,
    //CritChance,
    //CritDamage,
    //AttackSpeed

    public float combaBounty; // 현재 편성된 캐릭터들의 전투력 합산 (현상금)

    #region

    public void Start()
    {
        //StatEvents.OnCharacterBattlePowerChanged += 
        //StatEvents.
    }

    public void OnDestroy()
    {
        //StatEvents.OnCharacterBattlePowerChanged -= 
        //StatEvents.
    }

    /// <summary>
    /// 편성된 팀 전체 전투력을 다시 계산하는 메소드
    /// </summary>
    public void TeamBountyChanged()
    {

    }

    /// <summary>
    /// 한 캐릭터의 전투력을 계산하는 메소드
    /// </summary>
    /// <param name="characterData"></param>
    /// <returns></returns>
    public float ComputeFinalBounty(PlayerCharacterData characterData)
    {
        float Bounty =
            CPAttack(GetStat(characterData, Stat.Attack), characterData.characterdata.crewRole) +
            CPHealth(GetStat(characterData, Stat.Health), characterData.characterdata.crewRole) +
            CPDefense(GetStat(characterData, Stat.Defense), characterData.characterdata.crewRole) +
            CPCritical(GetStat(characterData, Stat.CritChance), GetStat(characterData, Stat.CritDamage), characterData.characterdata.crewRole) +
            CPSpeed(GetStat(characterData, Stat.AttackSpeed), characterData.characterdata.crewRole);
        return Bounty;
    }


    /// <summary>
    /// 편성 된 모든 캐릭터의 전투력을 계산하는 메소드
    /// </summary>
    /// <param name="characterData"></param>
    public float AllCPCalculate(CharacterData[] characterData)
    {
        // 초기화
        float oldCombatPower = combaBounty;
        combaBounty = 0;

        foreach (var character in characterData)
        {
            // 캐릭터가 null인 경우는 제외
            if (character == null)
            {
                continue;
            }

            //combatPower += AllCPCalculate(character);
        }

        float index = combaBounty - oldCombatPower;

        // ui 나 신호 추가
        if (index < 0)
        {
            Debug.Log($"{oldCombatPower}에서 {combaBounty} 으로 변경, 전투력이 감소했습니다. 감소량: {Mathf.Abs(index)}");
        }
        else if (index > 0)
        {
            Debug.Log($"{oldCombatPower}에서 {combaBounty} 으로 변경, 전투력이 증가했습니다. 증가량: {index}");
        }
        else
        {
            Debug.Log($"{oldCombatPower}에서 {combaBounty} 으로 변경, 전투력이 변동이 없습니다.");
        }

        return combaBounty;
    }

    

    public float GetStat(PlayerCharacterData characterData, Stat statName)
    {
        if (characterData.finalStats.ContainsKey(statName))
            return characterData.finalStats[statName];
        Debug.LogError($"{characterData} 캐릭터 에서 스탯{statName} 불러오기 에러, 0 반환");
        return 0f;
    }
    #endregion

        #region 스탯별 각 전투력 산정 메소드들
        /// <summary>
        /// 현상금 산정 공격력 부분을 계산하는 메소드
        /// </summary>
        /// <param name="attack">공격력</param>
        /// <param name="role">역할</param>
        /// <returns></returns>
    public float CPAttack(float attack, CrewRole role)
    {
        switch (role)
        {
            case CrewRole.Captain:
                return attack * 1.5f;
            case CrewRole.Deckhand:
                return attack * 0.2f;
            case CrewRole.Sailor:
                return attack * 1.2f;
            case CrewRole.Cook:
                return attack;
            default:
                Debug.LogError("공격력에 의한 현상금 산출 에러, 0 반환");
                return 0f;
        }
    }

    /// <summary>
    /// 현상금 산정 체력 부분을 계산하는 메소드
    /// </summary>
    /// <param name="health">체력</param>
    /// <param name="role">역할</param>
    /// <returns></returns>
    public float CPHealth(float health, CrewRole role)
    {
        switch (role)
        {
            case CrewRole.Captain:
                return health * 0.3f;
            case CrewRole.Deckhand:
                return health * 0.4f;
            case CrewRole.Sailor:
                return health * 0.3f;
            case CrewRole.Cook:
                return health * 0.4f;
            default:
                Debug.LogError("체력에 의한 현상금 산출 에러, 0 반환");
                return 0f;
        }
    }

    /// <summary>
    /// 현상금 산정 방어력 부분을 계산하는 메소드
    /// </summary>
    /// <param name="defence">체력</param>
    /// <param name="role">역할</param>
    /// <returns></returns>
    public float CPDefense(float defence, CrewRole role)
    {
        switch (role)
        {
            case CrewRole.Captain:
                return defence * 0.3f;
            case CrewRole.Deckhand:
                return defence * 0.4f;
            case CrewRole.Sailor:
                return defence * 0.2f;
            case CrewRole.Cook:
                return defence * 0.3f;
            default:
                Debug.LogError("방어력에 의한 현상금 산출 에러, 0 반환");
                return 0f;
        }
    }

    /// <summary>
    /// 현상금 산정 치명타 부분(확률, 배율)을 계산하는 메소드
    /// </summary>
    /// <param name="criticalChance">치명타 확률</param>
    /// <param name="criticalChance">치명타 배율</param>
    /// <param name="role">역할</param>
    /// <returns></returns>
    public float CPCritical(float criticalChance, float criticalDamage, CrewRole role)
    {
        switch (role)
        {
            case CrewRole.Captain:
                return (criticalChance * criticalDamage / 200) * 0.8f;
            case CrewRole.Deckhand:
                return (criticalChance * criticalDamage / 200) * 0.1f;
            case CrewRole.Sailor:
                return (criticalChance * criticalDamage / 200) * 0.8f;
            case CrewRole.Cook:
                return (criticalChance * criticalDamage / 200) * 0.3f;
            default:
                Debug.LogError("치명타에 의한 현상금 산출 에러, 0 반환");
                return 0f;
        }

    }

    /// <summary>
    /// 현상금 산정 공격속도 부분을 계산하는 메소드
    /// </summary>
    /// <param name="atkSpeed">공격속도</param>
    /// <param name="role">역할</param>
    /// <returns></returns>
    public float CPSpeed(float atkSpeed, CrewRole role)
    {
        switch (role)
        {
            case CrewRole.Captain:
                return atkSpeed * 1000f;
            case CrewRole.Deckhand:
                return atkSpeed * 1000f;
            case CrewRole.Sailor:
                return atkSpeed * 1000f;
            case CrewRole.Cook:
                return atkSpeed * 1000f;
            default:
                Debug.LogError("공격속도애 의한 현상금 산출 에러, 0 반환");
                return 0f;
        }
    }
    #endregion
}