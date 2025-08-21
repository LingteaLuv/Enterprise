using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BasicStatManager : Singleton<BasicStatManager>
{
    /// <summary>
    /// 기본 스탯이 변경되었을 때 발생하는 이벤트
    /// </summary>
    public static event Action OnBaseStatsChanged;

    [Header("기본 스탯 정의 (인스펙터에서 설정)")]
    public List<BasicStatData> statDefinitions;

    private Dictionary<BasicStatType, int> _playerBasicStatLevels = new Dictionary<BasicStatType, int>();
    private Dictionary<BasicStatType, BasicStatData> _statDefinitionsMap = new Dictionary<BasicStatType, BasicStatData>();

    protected override void Awake()
    {
        base.Awake();

        InitializeStatDefinitions();
        LoadPlayerBasicStatLevels();
    }

    private void InitializeStatDefinitions()
    {
        _statDefinitionsMap.Clear();
        foreach (var def in statDefinitions)
        {
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
        foreach (BasicStatType type in Enum.GetValues(typeof(BasicStatType)))
        {
            if (!_playerBasicStatLevels.ContainsKey(type))
            {
                _playerBasicStatLevels.Add(type, 1);
            }
        }
        Debug.Log("기본 스탯 레벨 초기화 완료.");
    }

    public int GetStatLevel(BasicStatType type)
    {
        if (_playerBasicStatLevels.TryGetValue(type, out int level))
        {
            return level;
        }
        Debug.LogError($"BasicStatManager: 스탯 '{type}'의 레벨을 찾을 수 없습니다. 기본값 1 반환.");
        return 1;
    }

    public float GetStatValue(BasicStatType type)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return 0f;
        }
        int currentLevel = GetStatLevel(type);
        return def.baseValue + (currentLevel - 1) * def.growthPerLevel;
    }

    public BigInteger GetLevelUpCost(BasicStatType type, int levelsToGain)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return BigInteger.Zero;
        }
        int currentLevel = GetStatLevel(type);
        BigInteger totalCost = BigInteger.Zero;
        for (int i = 0; i < levelsToGain; i++)
        {
            BigInteger costForNextLevel = (BigInteger)((double)def.BaseCost * Math.Pow(def.costIncreaseRatio, currentLevel + i - 1));
            totalCost += costForNextLevel;
        }
        return totalCost;
    }

    public bool TryLevelUpStat(BasicStatType type, int levelsToGain)
    {
        if (!_statDefinitionsMap.TryGetValue(type, out BasicStatData def))
        {
            Debug.LogError($"BasicStatManager: 스탯 '{type}'의 정의를 찾을 수 없습니다.");
            return false;
        }

        BigInteger requiredCost = GetLevelUpCost(type, levelsToGain);
        CurrencyType costType = CurrencyType.Gold;

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

        _playerBasicStatLevels[type] += levelsToGain;
        Debug.Log($"기본 스탯 '{type}' 레벨업! (Lv.{_playerBasicStatLevels[type] - levelsToGain} -> Lv.{_playerBasicStatLevels[type]})");

        // 기본 스탯 변경 이벤트 발생
        OnBaseStatsChanged?.Invoke();

        return true;
    }
}