using UnityEngine;
using System.Numerics; // BigInteger 사용

[System.Serializable]
public class BasicStatData
{
    public BasicStatType type; // 스탯 종류 (Attack, Defense, Health)
    public float baseValue; // Lv.1 스탯 기본값
    public float growthPerLevel; // 레벨당 스탯 증가량

    [Header("레벨업 비용 설정")]
    [SerializeField] private string baseCostString = "1000"; // 인스펙터에서 입력받을 문자열
    public double costIncreaseRatio; // 레벨업 시 비용 증가율 (예: 1.07)

    // 다른 스크립트에서 접근할 실제 BigInteger 값 (읽기 전용 프로퍼티)
    public BigInteger BaseCost { get; private set; }

    /// <summary>
    /// 인스펙터의 문자열 값을 실제 BigInteger 변수로 변환합니다.
    /// BasicStatManager의 InitializeStatDefinitions()에서 호출됩니다.
    /// </summary>
    public void Validate()
    {
        if (!BigInteger.TryParse(baseCostString, out BigInteger parsedBaseCost))
        {
            Debug.LogWarning($"BasicStatData: '{type}' 스탯의 baseCostString '{baseCostString}'파싱 실패.기본값 0으로 설정.");
            parsedBaseCost = BigInteger.Zero;
        }
        BaseCost = parsedBaseCost;
        baseCostString = BaseCost.ToString(); // 파싱된 값을 다시 문자열로 저장하여 정규화
    }
}