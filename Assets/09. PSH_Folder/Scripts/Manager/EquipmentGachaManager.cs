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

    [Header("강화 포인트 등급별 색상")]
    [Tooltip("포인트 등급 순서대로 색상을 지정합니다. (낮은->높은 순). 예를 들어 포인트 등급이 3개면, 색상은 4개(기본색상 포함) 필요합니다.")]
    public List<Color> enhancementTierColors;

    private List<int> enhancementPointTiers;

    protected override void Start()
    {
        base.Start(); // BaseGachaManager의 Start() 호출
        StartCoroutine(InitializeGachaPool());
        InitializeTiers();
    }

    private void InitializeTiers()
    {
        if (enhancementPointChances != null)
        {
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

        // 모든 아이템을 동일한 확률로 뽑습니다.
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
        Dictionary<int, int> enhancementPointsByItem = new Dictionary<int, int>();

        for (int i = 0; i < count; i++)
        {
            ItemObject drawnItem = DrawItem();
            if (drawnItem != null)
            {
                LastGachaResults.Add(drawnItem);
                if (drawnItem is WeaponObject weapon)
                {
                    int pointsToAdd = GetRandomEnhancementPoints();
                    enhancementPointsByItem[weapon.itemNum] = pointsToAdd;
                    InventoryManager.Instance.AddEnhancementPointsToEquipment(weapon.itemNum, pointsToAdd);
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
                resultUI.DisplayEquipmentResults(LastGachaResults, enhancementPointsByItem);
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

    public Color GetColorForPoints(int points)
    {
        if (enhancementTierColors == null || enhancementTierColors.Count == 0)
        {
            return Color.white;
        }

        int tierIndex = 0;
        for (int i = 0; i < enhancementPointTiers.Count; i++)
        {
            if (points >= enhancementPointTiers[i])
            {
                tierIndex = i + 1;
            }
            else
            {
                break;
            }
        }

        tierIndex = Mathf.Clamp(tierIndex, 0, enhancementTierColors.Count - 1);
        return enhancementTierColors[tierIndex];
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
