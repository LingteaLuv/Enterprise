using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using System;
using TMPro;

public class StatManager : MonoBehaviour
{
    [Header("--- 관리할 스탯 목록 ---")]
    public List<StatData> stats = new List<StatData>();

    [Header("--- 연결할 UI ---")]
    public List<StatDisplayPanel> statDisplays = new List<StatDisplayPanel>();
    public TextMeshProUGUI moneyDisplayText;

    // 공용 데이터
    public static BigInteger playerMoney = 100000;
    private int upgradeAmount = 1;

    void Start()
    {
        // 시작할 때 각 UI 패널을 초기화합니다.
        for (int i = 0; i < statDisplays.Count; i++)
        {
            if (i < stats.Count)
            {
                statDisplays[i].Initialize(this, i); // UI 패널에 매니저와 인덱스 정보 제공
                UpdateDisplayForStat(i);
            }
        }
        UpdateMoneyDisplay();
    }

    // UI 패널로부터 업그레이드 요청을 받습니다.
    public void AttemptUpgrade(int statIndex)
    {
        BigInteger totalCost = CalculateBatchCost(statIndex);

        if (playerMoney >= totalCost)
        {
            playerMoney -= totalCost;
            stats[statIndex].level += upgradeAmount;

            UpdateDisplayForStat(statIndex);
            UpdateMoneyDisplay();
        }
    }

    // 특정 스탯의 현재 능력치 값을 계산합니다.
    public BigInteger GetCurrentStatValue(int statIndex)
    {
        StatData stat = stats[statIndex];
        return stat.baseStat + (BigInteger)(stat.level - 1) * stat.statIncreasePerLevel;
    }

    // 특정 스탯의 묶음 업그레이드 비용을 계산합니다.
    public BigInteger CalculateBatchCost(int statIndex)
    {
        StatData stat = stats[statIndex];
        BigInteger totalCost = 0;
        for (int i = 0; i < upgradeAmount; i++)
        {
            int targetLevel = stat.level + i;
            double cost = (double)stat.baseCost * Math.Pow(stat.costIncreaseRatio, targetLevel - 1);
            totalCost += (BigInteger)cost;
        }
        return totalCost;
    }

    // 업그레이드 단위를 변경하고 모든 UI를 갱신합니다.
    public void SetUpgradeAmount(int amount)
    {
        upgradeAmount = amount;
        for (int i = 0; i < stats.Count; i++)
        {
            UpdateDisplayForStat(i);
        }
    }

    // 특정 스탯의 UI를 업데이트하도록 StatDisplayPanel에 요청합니다.
    public void UpdateDisplayForStat(int statIndex)
    {
        if (statIndex < statDisplays.Count)
        {
            StatData stat = stats[statIndex];
            BigInteger currentVal = GetCurrentStatValue(statIndex);
            BigInteger cost = CalculateBatchCost(statIndex);
            statDisplays[statIndex].UpdateDisplay(stat.statName, stat.level, currentVal, cost, upgradeAmount);
        }
    }

    public void UpdateMoneyDisplay()
    {
        if (moneyDisplayText != null)
        {
            moneyDisplayText.text = $"gold: {DataUtility.FormatNumber(playerMoney)}";
        }
    }
}