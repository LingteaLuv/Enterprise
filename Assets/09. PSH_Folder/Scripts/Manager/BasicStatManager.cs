using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using JHT;
using UnityEngine;

public class BasicStatManager : Singleton<BasicStatManager>
{
    public static event Action OnBaseStatsChanged;

    private Dictionary<BasicStatType, int> _playerBasicStatLevels = new Dictionary<BasicStatType, int>();
    private int _currentStage = 1;

    public class ParsingStatData
    {
        public int Attack;
        public int Defense;
        public int Health;
        public int Stage;
    }

    private void Start()
    {
        AuthManager.Instance.LoginCompleted += async () =>
        {
            await LoadPlayerBasicStatLevels();
        };
    }

    private async UniTask LoadPlayerBasicStatLevels()
    {
        await DatabaseManager.Instance.LoadFieldsAsync<ParsingStatData>("StatusData/Stat", (result) =>
        {
            _playerBasicStatLevels[BasicStatType.Attack] = result.Attack;
            _playerBasicStatLevels[BasicStatType.Defense] = result.Defense;
            _playerBasicStatLevels[BasicStatType.Health] = result.Health;
            _currentStage = result.Stage > 0 ? result.Stage : 1;
        });

        foreach (BasicStatType type in Enum.GetValues(typeof(BasicStatType)))
        {
            if (!_playerBasicStatLevels.ContainsKey(type))
            {
                _playerBasicStatLevels[type] = 0;
            }
        }
        Debug.Log($"기본 스탯 레벨 초기화 완료. 현재 단계: {_currentStage}");
    }

    public int GetCurrentStage() { return _currentStage; }
    public int GetStatLevel(BasicStatType type) { return _playerBasicStatLevels.TryGetValue(type, out int level) ? level : 0; }

    public float GetStatValue(BasicStatType type)
    {
        if (!ReinforcementDataManager.Instance.IsInitialized) return 0f;

        float bankedBonus = 0;
        for (int stage = 1; stage < _currentStage; stage++)
        {
            var maxLevelData = ReinforcementDataManager.Instance.GetDataForMaxLevel(stage);
            if (maxLevelData != null)
            {
                switch (type)
                {
                    case BasicStatType.Attack: bankedBonus += maxLevelData.TotalAttackBonus; break;
                    case BasicStatType.Defense: bankedBonus += maxLevelData.TotalDefenseBonus; break;
                    case BasicStatType.Health: bankedBonus += maxLevelData.TotalHpBonus; break;
                }
            }
        }

        int currentLevel = GetStatLevel(type);
        var currentData = ReinforcementDataManager.Instance.GetDataForLevel(_currentStage, currentLevel);
        float currentBonus = 0;
        if (currentData != null)
        {
            switch (type)
            {
                case BasicStatType.Attack: currentBonus = currentData.TotalAttackBonus; break;
                case BasicStatType.Defense: currentBonus = currentData.TotalDefenseBonus; break;
                case BasicStatType.Health: currentBonus = currentData.TotalHpBonus; break;
            }
        }

        return bankedBonus + currentBonus;
    }

    public BigInteger GetLevelUpCost(BasicStatType type, int levelsToGain)
    {
        if (!ReinforcementDataManager.Instance.IsInitialized) return -1;

        int currentLevel = GetStatLevel(type);
        int maxLevel = ReinforcementDataManager.Instance.GetMaxLevelForStage(_currentStage);
        if (maxLevel <= 0) return -1; // 현재 스테이지 데이터 없음

        int actualLevelsToGain = Mathf.Min(levelsToGain, maxLevel - currentLevel);
        if (actualLevelsToGain <= 0) return -1;

        BigInteger totalCost = BigInteger.Zero;
        for (int i = 0; i < actualLevelsToGain; i++)
        {
            var data = ReinforcementDataManager.Instance.GetDataForLevel(_currentStage, currentLevel + i);
            if (data == null) return -1;

            switch (type)
            {
                case BasicStatType.Attack: totalCost += data.NextAttackUpgradeCost; break;
                case BasicStatType.Defense: totalCost += data.NextDefenseUpgradeCost; break;
                case BasicStatType.Health: totalCost += data.NextHpUpgradeCost; break;
            }
        }
        return totalCost;
    }

    public bool TryLevelUpStat(BasicStatType type, int levelsToGain)
    {
        int currentLevel = GetStatLevel(type);
        int maxLevel = ReinforcementDataManager.Instance.GetMaxLevelForStage(_currentStage);
        if (maxLevel <= 0) return false;

        int actualLevelsToGain = Mathf.Min(levelsToGain, maxLevel - currentLevel);

        if (actualLevelsToGain <= 0)
        {
            Debug.LogWarning($"스탯 '{type}'은(는) 이미 현재 단계의 최대 레벨({maxLevel})에 도달했습니다.");
            return false;
        }

        BigInteger requiredCost = GetLevelUpCost(type, actualLevelsToGain);
        if (requiredCost < 0)
        {
            Debug.LogError("비용 계산 오류 또는 최대 레벨 도달");
            return false;
        }

        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.Gold, requiredCost))
        {
            Debug.LogWarning($"기본 스탯 '{type}' 레벨업 실패: 재화 부족. 필요: {requiredCost}");
            return false;
        }

        _playerBasicStatLevels[type] += actualLevelsToGain;
        DatabaseManager.Instance.SaveField($"StatusData/Stat/{type.ToString()}", _playerBasicStatLevels[type]);
        Debug.Log($"기본 스탯 '{type}' 레벨업! (Lv.{currentLevel} -> Lv.{_playerBasicStatLevels[type]})");

        OnBaseStatsChanged?.Invoke();
        CheckForStageUp();

        return true;
    }

    private void CheckForStageUp()
    {
        int maxLevel = ReinforcementDataManager.Instance.GetMaxLevelForStage(_currentStage);
        if (maxLevel <= 0) return;

        int atkLvl = GetStatLevel(BasicStatType.Attack);
        int defLvl = GetStatLevel(BasicStatType.Defense);
        int hpLvl = GetStatLevel(BasicStatType.Health);

        if (atkLvl >= maxLevel && defLvl >= maxLevel && hpLvl >= maxLevel)
        {
            int nextStage = _currentStage + 1;
            if (ReinforcementDataManager.Instance.DoesStageExist(nextStage))
            {
                _currentStage = nextStage;
                _playerBasicStatLevels[BasicStatType.Attack] = 0;
                _playerBasicStatLevels[BasicStatType.Defense] = 0;
                _playerBasicStatLevels[BasicStatType.Health] = 0;

                DatabaseManager.Instance.SaveField("StatusData/Stat/Stage", _currentStage);
                DatabaseManager.Instance.SaveField("StatusData/Stat/Attack", 0);
                DatabaseManager.Instance.SaveField("StatusData/Stat/Defense", 0);
                DatabaseManager.Instance.SaveField("StatusData/Stat/Health", 0);

                Debug.Log($"<color=cyan>축하합니다! 다음 단계로 진입했습니다: Stage {_currentStage}</color>");
                OnBaseStatsChanged?.Invoke();
            }
            else
            {
                Debug.Log("최종 단계입니다. 더 이상 단계 상승을 할 수 없습니다.");
            }
        }
    }
}