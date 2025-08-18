using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using TMPro;

/// <summary>
/// 재화의 종류를 정의합니다. 새로운 재화가 생기면 여기에 추가하면 됩니다.
/// </summary>
[System.Serializable]
public enum CurrencyType
{
    Gold,
    EnhancementStone,
    Crystal
}

/// <summary>
/// 비용 정보를 담는 구조체입니다. (어떤 재화가, 얼마나 필요한지)
/// </summary>
[System.Serializable]
public class CostInfo
{
    public CurrencyType currencyType;
    public string costString = "100"; // 인스펙터에서 큰 숫자를 편하게 입력하기 위한 문자열

    public BigInteger Cost { get; private set; }

    public void Validate() // BigInteger로 변환
    {
        if (!BigInteger.TryParse(costString, out BigInteger parsedCost))
            parsedCost = 100;
        Cost = parsedCost;
        costString = Cost.ToString();
    }
}


/// <summary>
/// 플레이어의 모든 재화를 중앙에서 관리하는 싱글턴 매니저입니다.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // 각 재화(CurrencyType)를 얼마나 보유하고 있는지 저장하는 딕셔너리
    private Dictionary<CurrencyType, BigInteger> currencyWallet = new Dictionary<CurrencyType, BigInteger>();

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI stoneText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWallet(); // 처음 시작 시 지갑 초기화
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWallet()
    {
        // 모든 재화 종류에 대해 초기값을 0으로 설정합니다.
        foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
        {
            if (!currencyWallet.ContainsKey(type))
            {
                currencyWallet.Add(type, 0);
            }
        }
        // TODO: 여기에 게임 시작 시 저장된 재화 데이터를 불러오는 로직을 추가해야 합니다.
        // 예: LoadData();

        // 테스트용 초기 재화 지급
        AddCurrency(CurrencyType.Gold, 10000000);
        AddCurrency(CurrencyType.EnhancementStone, 1000);
        Debug.Log("[CurrencyManager] 지갑 초기화 및 테스트 재화 지급 완료.");
        UpdateCurrencyUI();
    }

    /// <summary>
    /// 특정 재화의 현재 보유량을 가져옵니다.
    /// </summary>
    public BigInteger GetCurrency(CurrencyType type)
    {
        if (currencyWallet.TryGetValue(type, out BigInteger amount))
        {
            return amount;
        }
        return 0;
    }

    /// <summary>
    /// 특정 재화를 획득합니다.
    /// </summary>
    public void AddCurrency(CurrencyType type, BigInteger amount)
    {
        if (amount <= 0) return;

        if (currencyWallet.ContainsKey(type))
        {
            currencyWallet[type] += amount;
        }
        else
        {
            currencyWallet.Add(type, amount);
        }
        Debug.Log($"[CurrencyManager] {type} 획득: +{DataUtility.FormatNumber(amount)}. 현재 보유량: {DataUtility.FormatNumber(currencyWallet[type])}");
        // TODO: 재화 UI 갱신 이벤트 호출
    }

    /// <summary>
    /// 특정 재화를 소모합니다. 재화가 충분할 때만 성공합니다.
    /// </summary>
    /// <returns>소모 성공 여부</returns>
    public bool SpendCurrency(CurrencyType type, BigInteger amount)
    {
        if (amount <= 0) return true; // 0 또는 음수 소모는 항상 성공

        BigInteger currentAmount = GetCurrency(type); // 현재 보유량 가져오기
        Debug.Log($"[CurrencyManager] {type} 소모 시도. 필요: {DataUtility.FormatNumber(amount)}, 보유: {DataUtility.FormatNumber(currentAmount)}");

        if (currentAmount >= amount)
        {
            currencyWallet[type] -= amount;
            Debug.Log($"[CurrencyManager] {type} 소모 성공: -{DataUtility.FormatNumber(amount)}. 현재 보유량: {DataUtility.FormatNumber(currencyWallet[type])}");
            // TODO: 재화 UI 갱신 이벤트 호출
            return true;
        }
        else
        {
            Debug.LogWarning($"[CurrencyManager] {type} 재화 부족! 필요: {DataUtility.FormatNumber(amount)}, 보유: {DataUtility.FormatNumber(currentAmount)}");
            return false;
        }
    }

    public void UpdateCurrencyUI()
    {
        goldText.text = $"gold : {DataUtility.FormatNumber(currencyWallet[CurrencyType.Gold])}";
        stoneText.text = $"stone : {DataUtility.FormatNumber(currencyWallet[CurrencyType.EnhancementStone])}";
    }
}