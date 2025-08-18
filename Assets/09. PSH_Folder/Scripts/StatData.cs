using System.Numerics;
using UnityEngine;

[System.Serializable]
public class StatData
{
    public string statName = "new stat";

    [Header("BigInt Values")]
    [SerializeField] private string baseStatString = "10";
    [SerializeField] private string statIncreasePerLevelString = "10";
    [Header("비용 정보")]
    public CostInfo baseCost; // 어떤 재화(CurrencyType)가 필요한지에 대한 정보
    public double costIncreaseRatio = 1.07; // 레벨업 시 비용 증가율

    // 다른 스크립트에서 접근할 실제 BigInteger 값들 (읽기 전용 프로퍼티)
    public System.Numerics.BigInteger BaseStat { get; private set; }
    public System.Numerics.BigInteger StatIncreasePerLevel { get; private set; }

    /// <summary>
    /// 인스펙터의 문자열 값들을 실제 BigInteger 변수로 변환합니다.
    /// 이 클래스를 사용하는 MonoBehaviour의 OnValidate()나 Awake()에서 호출해주세요.
    /// </summary>
    public void Validate()
    {
        if (!System.Numerics.BigInteger.TryParse(baseStatString, out System.Numerics.BigInteger parsedBaseStat))
            parsedBaseStat = 10;
        BaseStat = parsedBaseStat;
        baseStatString = BaseStat.ToString();

        if (!System.Numerics.BigInteger.TryParse(statIncreasePerLevelString, out System.Numerics.BigInteger parsedStatIncrease))
            parsedStatIncrease = 5;
        StatIncreasePerLevel = parsedStatIncrease;
        statIncreasePerLevelString = StatIncreasePerLevel.ToString();

        // 비용 정보도 Validate 호출
        baseCost?.Validate();
    }
}