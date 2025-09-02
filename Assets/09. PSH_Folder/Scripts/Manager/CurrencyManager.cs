using System;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// 재화의 종류를 정의합니다. 새로운 재화가 생기면 여기에 추가하면 됩니다.
/// </summary>
[System.Serializable]
public enum CurrencyType
{
    Gold,
    EnhancementStone,
    Gem
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
public class CurrencyManager : Singleton<CurrencyManager>
{
    // 각 재화(CurrencyType)를 얼마나 보유하고 있는지 저장하는 딕셔너리
    private Dictionary<CurrencyType, BigInteger> currencyWallet = new Dictionary<CurrencyType, BigInteger>();

    // 인스펙터에서 초기 재화량을 설정하기 위한 문자열 필드
    [Header("초기 재화 설정 (인스펙터용)")]
    [SerializeField] private string _initialGoldString;
    [SerializeField] private string _initialEnhancementStoneString;
    [SerializeField] private string _initialGemString;

    public Action<string, string, string> OnUpdateCurrency;
    public bool IsFireBase;
    protected async override void Awake()
    {
        base.Awake();

        await InitializeWallet(); // 처음 시작 시 지갑 초기화
    }

    private async Task InitializeWallet()
    {
        // 모든 재화 종류에 대해 초기값을 0으로 설정합니다.
        foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
        {
            if (!currencyWallet.ContainsKey(type))
            {
                currencyWallet.Add(type, 0);
            }
        }
        if (IsFireBase)
        {
            string rootPath = $"{FirebaseManager.Auth.CurrentUser.UserId}/CreditData";

            await DatabaseManager.Instance.LoadFieldAsync<int>($"{rootPath}/Gold", (value) =>
            {
                Debug.Log($"Gold DB 로드 {value}");
                _initialGoldString = value.ToString();
            });

            await DatabaseManager.Instance.LoadFieldAsync<int>($"{rootPath}/EnhancementStone", (value) =>
            {
                Debug.Log($"EnhancementStone DB 로드 {value}");
                _initialEnhancementStoneString = value.ToString();
            });

            await DatabaseManager.Instance.LoadFieldAsync<int>($"{rootPath}/Gem", (value) =>
            {
                Debug.Log($"Gem DB 로드 {value}");
                _initialGemString = value.ToString();
            });
        }
        // 인스펙터에서 설정된 초기 재화량을 적용합니다.
        AddCurrencyFromInspectorString(CurrencyType.Gold, _initialGoldString);
        AddCurrencyFromInspectorString(CurrencyType.EnhancementStone, _initialEnhancementStoneString);
        AddCurrencyFromInspectorString(CurrencyType.Gem, _initialGemString);
    }

    private void Start()
    {
        UpdateCurrencyUI();
    }
    
    // 헬퍼 함수: 문자열에서 재화를 추가
    public void AddCurrencyFromInspectorString(CurrencyType type, string amountString)
    {
        if (BigInteger.TryParse(amountString, out BigInteger amount))
        {
            currencyWallet[type] = amount; // 직접 할당 (AddCurrency는 로그가 많으므로)
        }
        else
        {
            Debug.LogWarning($"[CurrencyManager] {type} 초기 재화 문자열 '{amountString}' 파싱 실패. 0으로 설정됩니다.");
            currencyWallet[type] = 0;
        }
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
            //currencyWallet[type] += amount;
            DatabaseManager.Instance.AddCurrency(type.ToString(), (int)amount);
        }
        else
        {
            //currencyWallet.Add(type, amount);
            DatabaseManager.Instance.AddCurrency(type.ToString(), (int)amount);
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
        string s1 = $"gold : {DataUtility.FormatNumber(currencyWallet[CurrencyType.Gold])}";
        string s2 = $"stone : {DataUtility.FormatNumber(currencyWallet[CurrencyType.EnhancementStone])}";
        string s3 = $"gem : {DataUtility.FormatNumber(currencyWallet[CurrencyType.Gem])}";
        OnUpdateCurrency?.Invoke(s1,s2,s3);
    }

    // 인스펙터에서 값이 변경될 때 호출됩니다.
    private void OnValidate()
    {
        // 에디터에서만 실행되도록 합니다. (빌드된 게임에서는 호출되지 않음)
        // 그리고 게임이 실행 중일 때만 currencyWallet을 직접 업데이트합니다.
        if (Application.isPlaying) // 게임이 실행 중일 때만
        {
            UpdateCurrencyFromInspectorStrings();
        }
        else // 게임이 실행 중이 아닐 때 (에디터에서만)
        {
            // 인스펙터에서 입력된 문자열을 BigInteger로 파싱하고 다시 문자열로 변환하여 정규화합니다.
            // Gold
            if (BigInteger.TryParse(_initialGoldString, out BigInteger parsedGold))
            {
                _initialGoldString = parsedGold.ToString();
            }
            else
            {
                _initialGoldString = "0"; // 파싱 실패 시 기본값
            }

            // EnhancementStone
            if (BigInteger.TryParse(_initialEnhancementStoneString, out BigInteger parsedStone))
            {
                _initialEnhancementStoneString = parsedStone.ToString();
            }
            else
            {
                _initialEnhancementStoneString = "0";
            }

            // Gem
            if (BigInteger.TryParse(_initialGemString, out BigInteger parsedGem))
            {
                _initialGemString = parsedGem.ToString();
            }
            else
            {
                _initialGemString = "0";
            }
        }
    }

    // 인스펙터 문자열에서 실제 currencyWallet을 업데이트하는 헬퍼 함수
    private void UpdateCurrencyFromInspectorStrings()
    {
        // currencyWallet이 초기화되지 않았을 수 있으므로 체크
        if (currencyWallet == null || currencyWallet.Count == 0)
        {
            // Debug.LogWarning("CurrencyManager: currencyWallet이 아직 초기화되지 않았습니다. OnValidate에서 업데이트를 건너뜀.");
            return;
        }

        // Gold
        if (BigInteger.TryParse(_initialGoldString, out BigInteger parsedGold))
        {
            if (currencyWallet[CurrencyType.Gold] != parsedGold) // 값이 변경되었을 때만 업데이트
            {
                currencyWallet[CurrencyType.Gold] = parsedGold;
                Debug.Log($"[CurrencyManager] 인스펙터에서 Gold 변경됨: {DataUtility.FormatNumber(parsedGold)}");
                UpdateCurrencyUI(); // UI 갱신
            }
        }
        else
        {
            _initialGoldString = currencyWallet[CurrencyType.Gold].ToString(); // 유효하지 않으면 현재 값으로 되돌림
        }

        // EnhancementStone
        if (BigInteger.TryParse(_initialEnhancementStoneString, out BigInteger parsedStone))
        {
            if (currencyWallet[CurrencyType.EnhancementStone] != parsedStone)
            {
                currencyWallet[CurrencyType.EnhancementStone] = parsedStone;
                Debug.Log($"[CurrencyManager] 인스펙터에서 Stone 변경됨: {DataUtility.FormatNumber(parsedStone)}");
                UpdateCurrencyUI();
            }
        }
        else
        {
            _initialEnhancementStoneString = currencyWallet[CurrencyType.EnhancementStone].ToString();
        }

        // Gem
        if (BigInteger.TryParse(_initialGemString, out BigInteger parsedGem))
        {
            if (currencyWallet[CurrencyType.Gem] != parsedGem)
            {
                currencyWallet[CurrencyType.Gem] = parsedGem;
                Debug.Log($"[CurrencyManager] 인스펙터에서 Gem 변경됨: {DataUtility.FormatNumber(parsedGem)}");
                UpdateCurrencyUI();
            }
        }
        else
        {
            _initialGemString = currencyWallet[CurrencyType.Gem].ToString();
        }
    }
}