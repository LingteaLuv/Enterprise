using System.Numerics;
using UnityEngine;

[System.Serializable]
public class StatData
{
    public string statName = "new stat";
    [Min(1)]
    public int level = 1;

    [Header("BigInt Values")]
    [SerializeField] private string baseStatString = "10";
    [SerializeField] private string statIncreasePerLevelString = "5";
    [SerializeField] private string baseCostString = "100";

    // 다른 스크립트에서 접근할 실제 BigInteger 값들 (읽기 전용 프로퍼티)
    public BigInteger BaseStat { get; private set; }
    public BigInteger StatIncreasePerLevel { get; private set; }
    public BigInteger BaseCost { get; private set; }

    /// <summary>
    /// 인스펙터의 문자열 값들을 실제 BigInteger 변수로 변환합니다.
    /// 이 클래스를 사용하는 MonoBehaviour의 OnValidate()나 Awake()에서 호출해주세요.
    /// </summary>
    public void Validate()
    {
        if (!BigInteger.TryParse(baseStatString, out BigInteger parsedBaseStat))
            parsedBaseStat = 10;
        BaseStat = parsedBaseStat;
        baseStatString = BaseStat.ToString();

        if (!BigInteger.TryParse(statIncreasePerLevelString, out BigInteger parsedStatIncrease))
            parsedStatIncrease = 5;
        StatIncreasePerLevel = parsedStatIncrease;
        statIncreasePerLevelString = StatIncreasePerLevel.ToString();

        if (!BigInteger.TryParse(baseCostString, out BigInteger parsedBaseCost))
            parsedBaseCost = 100;
        BaseCost = parsedBaseCost;
        baseCostString = BaseCost.ToString();
    }
    public double costIncreaseRatio = 1.07;
}