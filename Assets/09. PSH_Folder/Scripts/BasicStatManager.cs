using System; // Math.Pow 사용
using System.Collections.Generic;
using System.Numerics; // BigInteger 사용
using UnityEngine;

public class BasicStatManager : MonoBehaviour
{
    public static BasicStatManager Instance { get; private set; }

    [Header("기본 스탯 정의 (인스펙터에서 설정)")]
    public List<BasicStatData> statDefinitions; // 각 스탯의 기본값, 성장치, 비용 정의

    // 플레이어의 현재 기본 스탯 레벨 (Key: 스탯 종류, Value: 현재 레벨)
    private Dictionary<BasicStatType, int> _playerBasicStatLevels = new
Dictionary<BasicStatType, int>();

    // 스탯 정의를 빠르게 찾기 위한 맵
    private Dictionary<BasicStatType, BasicStatData> _statDefinitionsMap = new
Dictionary<BasicStatType, BasicStatData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializeStatDefinitions();
        LoadPlayerBasicStatLevels(); // 저장된 데이터 로드 (초기에는 기본값)
    }

    private void InitializeStatDefinitions()
    {
        _statDefinitionsMap.Clear();
        foreach (var def in statDefinitions)
        {
            // BasicStatData의 Validate() 메서드를 호출하여 BigInteger 파싱
            def.Validate();

            if (!_statDefinitionsMap.ContainsKey(def.type))
            {
                _statDefinitionsMap.Add(def.type, def);
            }
            else
            {
                Debug.LogWarning($"BasicStatManager: 스탯 정의에 중복된 타입이 있습니다: {def.type}");
            }
        }
    }

    private void LoadPlayerBasicStatLevels()
    {
        // TODO: 실제 게임에서는 저장된 데이터를 로드해야 합니다.
        // 현재는 모든 스탯을 레벨 1로 초기화합니다.
        foreach (BasicStatType type in Enum.GetValues(typeof(BasicStatType)))
        {
            if (!_playerBasicStatLevels.ContainsKey(type))
            {
                _playerBasicStatLevels.Add(type, 1); // 모든 스탯을 레벨 1로 초기화
            }
        }
        Debug.Log("기본 스탯 레벨 초기화 완료.");
    }

    /// <summary>
    /// 특정 기본 스탯의 현재 레벨을 가져옵니다.
    /// </summary>
    public int GetStatLevel(BasicStatType type)
    {
        if (_playerBasicStatLevels.TryGetValue(type, out int level))
        {
            return level;
        }
        Debug.LogError($"BasicStatManager: 스탯 '{type}'의 레벨을 찾을 수 없습니다. 기본값 1 반환.");
        return 1;
    }

    /// <summary>
    /// 특정 기본 스탯의 현재 최종 값을 계산하여 가져옵니다.
    /// </summary>
    public float GetStatValue(BasicStatType type)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return 0f;
        }

        int currentLevel = GetStatLevel(type);
        // 공식: 기본값 + (현재 레벨 - 1) * 레벨당 성장치
        return def.baseValue + (currentLevel - 1) * def.growthPerLevel;
    }

    /// <summary>
    /// 특정 기본 스탯을 지정된 레벨만큼 올리는 데 필요한 총 비용을 계산합니다.
    /// </summary>
    /// <param name="type">스탯 종류</param>
    /// <param name="levelsToGain">올릴 레벨 수 (1 또는 10)</param>
    /// <returns>총 비용 (BigInteger)</returns>
    public BigInteger GetLevelUpCost(BasicStatType type, int levelsToGain)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return BigInteger.Zero;
        }

        int currentLevel = GetStatLevel(type);
        BigInteger totalCost = BigInteger.Zero;

        // 등비수열 합 공식: a * (r^n - 1) / (r - 1)
        // 여기서 a는 첫 번째 레벨업 비용, r은 비용 증가율, n은 올릴 레벨 수
        // 각 레벨업 비용을 개별적으로 더하는 방식 (정확하고 안전함)
        for (int i = 0; i < levelsToGain; i++)
        {
            // def.BaseCost 프로퍼티 사용
            BigInteger costForNextLevel = (BigInteger)((double)def.BaseCost * Math.Pow(def.costIncreaseRatio, currentLevel + i - 1));
            totalCost += costForNextLevel;
        }
        return totalCost;
    }

    /// <summary>
    /// 특정 기본 스탯을 레벨업 시도합니다.
    /// </summary>
    /// <param name="type">스탯 종류</param>
    /// <param name="levelsToGain">올릴 레벨 수 (1 또는 10)</param>
    /// <returns>성공 여부</returns>
    public bool TryLevelUpStat(BasicStatType type, int levelsToGain)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return false;
        }

        BigInteger requiredCost = GetLevelUpCost(type, levelsToGain);
        CurrencyType costType = CurrencyType.Gold; // 기본 스탯 레벨업 비용은 골드라고 가정

        // 재화 확인 및 소모
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance가 초기화되지 않았습니다.");
            return false;
        }
        if (!CurrencyManager.Instance.SpendCurrency(costType, requiredCost))
        {
            Debug.LogWarning($"기본 스탯 '{type}' 레벨업 실패: {costType} 재화 부족. 필요: {requiredCost}");
            return false;
        }

        // 레벨업 진행
        _playerBasicStatLevels[type] += levelsToGain;
        Debug.Log($"기본 스탯 '{type}' 레벨업! (Lv.{_playerBasicStatLevels[type] - levelsToGain} -> Lv.{_playerBasicStatLevels[type]})");

        // TODO: UI 갱신 이벤트 발생 (BasicStatUI에서 구독)
        return true;
    }
}