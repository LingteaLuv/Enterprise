using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JHT;

[System.Serializable]
public class EnhancementPointChance
{
    public int points;
    [Range(0, 100)]
    public float chance; // 0-100%
}

public class EquipmentGachaManager : BaseGachaManager<ItemObject>
{
    private List<ItemWeaponSO> gachaPool = new List<ItemWeaponSO>();

    [Header("강화 포인트 확률")]
    public List<EnhancementPointChance> enhancementPointChances;

    // 강화 포인트의 등급 기준 (포인트 오름차순으로 정렬됨)
    private List<int> enhancementPointTiers;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(InitializeGachaPool());
        InitializeTiers();
    }

    private void InitializeTiers()
    {
        if (enhancementPointChances != null)
        {
            // 포인트를 기준으로 오름차순 정렬하여 등급의 기준점을 만듭니다.
            enhancementPointTiers = enhancementPointChances.Select(e => e.points).OrderBy(p => p).ToList();
        }
        else
        {
            enhancementPointTiers = new List<int>();
        }
    }

    private System.Collections.IEnumerator InitializeGachaPool()
    {
        while (ItemDataManager.Instance == null || !ItemDataManager.Instance.IsDataLoaded)
        {
            yield return null;
        }
        gachaPool = ItemDataManager.Instance.GetAllWeaponData().Values.ToList();
        Debug.Log($"[EquipmentGachaManager] {gachaPool.Count}개의 무기를 뽑기 풀에 추가했습니다.");
    }

    protected override ItemObject DrawItem()
    {
        if (gachaPool.Count == 0)
        {
            Debug.LogError("[EquipmentGachaManager] 뽑기 풀이 비어있습니다!");
            return null;
        }

        ItemWeaponSO drawnWeaponSO = gachaPool[Random.Range(0, gachaPool.Count)];
        Debug.Log($"[EquipmentGachaManager] 뽑힌 아이템: {drawnWeaponSO.itemName}");
        return InventoryManager.Instance.AddItem(drawnWeaponSO);
    }

    public override bool PerformMultipleGacha(int count)
    {
        if (!CurrencyManager.Instance.SpendCurrency(currencyType, singleGachaCost * count))
        {
            Debug.Log($"가챠 실패: 재화({currencyType})가 부족합니다.");
            return false;
        }

        LastGachaResults = new List<ItemObject>();
        Dictionary<int, PointTier> itemTiers = new Dictionary<int, PointTier>();

        for (int i = 0; i < count; i++)
        {
            ItemObject drawnItem = DrawItem();
            if (drawnItem != null)
            {
                LastGachaResults.Add(drawnItem);
                if (drawnItem is WeaponObject weapon)
                {
                    int pointsToAdd = GetRandomEnhancementPoints();
                    InventoryManager.Instance.AddEnhancementPointsToEquipment(weapon.itemNum, pointsToAdd);

                    // 획득한 포인트의 등급을 결정하여 UI에 넘겨줄 정보 저장
                    itemTiers[weapon.itemNum] = GetTierForPoints(pointsToAdd);

                    AutoEnhanceWeapon(weapon);
                }
            }
        }

        Debug.Log($"{count}회 뽑기 완료! {LastGachaResults.Count}개의 아이템 획득.");

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            GachaListUI resultUI = resultPanel.GetComponent<GachaListUI>();
            if (resultUI != null)
            {
                resultUI.DisplayEquipmentResults(LastGachaResults, itemTiers);
            }
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }

    private int GetRandomEnhancementPoints()
    {
        float totalChance = enhancementPointChances.Sum(epc => epc.chance);
        if (totalChance <= 0) return 0;

        float randomPoint = Random.Range(0, totalChance);
        foreach (var epc in enhancementPointChances)
        {
            if (randomPoint < epc.chance)
            {
                return epc.points;
            }
            randomPoint -= epc.chance;
        }
        return 0;
    }

    private PointTier GetTierForPoints(int points)
    {
        // enhancementPointTiers는 Inspector에 설정된 포인트를 오름차순으로 정렬한 리스트입니다. (예: [20, 50, 100])
        // 이 리스트는 3개의 항목(Low, Mid, High)을 가지고 있다고 가정합니다.
        if (enhancementPointTiers == null || enhancementPointTiers.Count != 3)
        {
            Debug.LogWarning("강화 포인트 확률(EnhancementPointChances)은 3개의 등급(Low, Mid, High)에 맞게 3개 항목으로 설정해야 합니다.");
            // 안전 장치: 설정이 잘못되었을 경우, 가장 낮은 등급으로 처리
            return PointTier.Low;
        }

        // 정렬된 리스트의 값과 정확히 일치하는지 확인하여 등급을 결정합니다.
        if (points == enhancementPointTiers[2]) // 가장 높은 포인트 값 = High
        {
            return PointTier.High;
        }
        else if (points == enhancementPointTiers[1]) // 중간 포인트 값 = Mid
        {
            return PointTier.Mid;
        }
        else // 가장 낮은 포인트 값 = Low
        {
            return PointTier.Low;
        }
    }

    private void AutoEnhanceWeapon(WeaponObject weapon)
    {
        if (weapon == null) return;
        const int levelUpCost = 10;
        const int maxLevel = 50;

        bool needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
        bool canLevelUp = InventoryManager.Instance.GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;

        while (canLevelUp)
        {
            bool levelUpSuccess = InventoryManager.Instance.LevelUpEquipment(weapon.itemNum);
            if (levelUpSuccess)
            {
                needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
                canLevelUp = InventoryManager.Instance.GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;
            }
            else
            {
                break;
            }
        }
    }
}