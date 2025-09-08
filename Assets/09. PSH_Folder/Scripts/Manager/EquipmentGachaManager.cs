using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JHT;
using System;

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

        ItemWeaponSO drawnWeaponSO = gachaPool[UnityEngine.Random.Range(0, gachaPool.Count)];
        Debug.Log($"[EquipmentGachaManager] 뽑힌 아이템: {drawnWeaponSO.itemName}");
        return InventoryManager.Instance.AddItem(drawnWeaponSO);
    }

    public override bool PerformMultipleGacha(int count)
    {
        CurrencyType ticketType = CurrencyType.EquipDrawTicket;
        CurrencyType gemType = CurrencyType.Gem;
        int ticketCostPerGacha = 1;
        int gemCostPerTicket = 100; // 보석 교환비는 캐릭터와 동일하다고 가정

        System.Numerics.BigInteger requiredTickets = count * ticketCostPerGacha;

        if (CurrencyManager.Instance.CanSpendCurrency(ticketType, requiredTickets))
        {
            CurrencyManager.Instance.SpendCurrency(ticketType, requiredTickets);
            ExecuteGachaDraw(count);
            return true;
        }
        else
        {
            System.Numerics.BigInteger currentTickets = CurrencyManager.Instance.GetCurrency(ticketType);
            System.Numerics.BigInteger neededTickets = requiredTickets - currentTickets;
            System.Numerics.BigInteger requiredGems = neededTickets * gemCostPerTicket;

            if (CurrencyManager.Instance.CanSpendCurrency(gemType, requiredGems))
            {
                Action onConfirm = () =>
                {
                    if (currentTickets > 0)
                    {
                        CurrencyManager.Instance.SpendCurrency(ticketType, currentTickets);
                    }
                    if (CurrencyManager.Instance.SpendCurrency(gemType, requiredGems))
                    {
                        Debug.Log($"성공적으로 {requiredGems} 보석을 사용해 부족한 {neededTickets}개의 뽑기 횟수를 대체했습니다.");
                        ExecuteGachaDraw(count);
                    }
                    else
                    {
                        Debug.LogError("알 수 없는 오류로 보석 소모에 실패했습니다. 가챠를 중단합니다.");
                    }
                };

                string message = $"{neededTickets}개의 티켓이 부족합니다.\n{requiredGems}개의 보석으로 구매하시겠습니까?";
                PopManager.Instance.ShowOKCancelPopup(message, "구매", onConfirm, "취소");
                return true;
            }
            else
            {
                PopManager.Instance.ShowOKPopup($"재화가 부족합니다.\n티켓과 보석을 확인해주세요.");
                return false;
            }
        }
    }

    private void ExecuteGachaDraw(int count)
    {
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
    }

    private int GetRandomEnhancementPoints()
    {
        float totalChance = enhancementPointChances.Sum(epc => epc.chance);
        if (totalChance <= 0) return 0;

        float randomPoint = UnityEngine.Random.Range(0, totalChance);
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
        if (enhancementPointTiers == null || enhancementPointTiers.Count != 3)
        {
            Debug.LogWarning("강화 포인트 확률(EnhancementPointChances)은 3개의 등급(Low, Mid, High)에 맞게 3개 항목으로 설정해야 합니다.");
            return PointTier.Low;
        }

        if (points == enhancementPointTiers[2])
        {
            return PointTier.High;
        }
        else if (points == enhancementPointTiers[1])
        {
            return PointTier.Mid;
        }
        else
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
