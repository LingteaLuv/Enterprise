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

    private void Start()
    {
        StartCoroutine(InitializeGachaPool());
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

    /// <summary>
    /// BaseGachaManager로부터 특정 등급을 받아, 해당 등급의 아이템을 뽑는 실제 로직입니다。
    /// </summary>
    protected override ItemObject DrawItem(Rarity rarity)
    {
        if (gachaPool.Count == 0)
        {
            Debug.LogError("[EquipmentGachaManager] 뽑기 풀이 비어있습니다!");
            return null;
        }

        // 선택된 등급에 해당하는 아이템만 필터링
        List<ItemWeaponSO> filteredPool = gachaPool.Where(weapon => weapon.rarity == rarity).ToList();

        ItemWeaponSO drawnWeaponSO;
        if (filteredPool.Count == 0)
        {
            Debug.LogWarning($"[EquipmentGachaManager] {rarity} 등급의 아이템이 뽑기 풀에 없습니다. 전체 풀에서 무작위로 선택합니다.");
            // 해당 등급의 아이템이 없을 경우, 전체 풀에서 무작위로 선택
            drawnWeaponSO = gachaPool[Random.Range(0, gachaPool.Count)];
        }
        else
        {
            drawnWeaponSO = filteredPool[Random.Range(0, filteredPool.Count)];
        }

        Debug.Log($"[EquipmentGachaManager] 뽑힌 등급: {rarity}, 실제 뽑힌 아이템 등급: {drawnWeaponSO.rarity}, 아이템 이름: {drawnWeaponSO.itemName}");

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
        for (int i = 0; i < count; i++)
        {
            Rarity chosenRarity = GetRandomRarity();
            ItemObject drawnItem = DrawItem(chosenRarity);
            if (drawnItem != null)
            {
                LastGachaResults.Add(drawnItem);
            }
        }

        Dictionary<int, int> enhancementPointsMap = new Dictionary<int, int>();
        float totalChance = enhancementPointChances.Sum(epc => epc.chance);

        if (totalChance > 0)
        {
            foreach (var drawnItem in LastGachaResults)
            {
                if (drawnItem is WeaponObject weapon)
                {
                    float randomPoint = Random.Range(0, totalChance);
                    int pointsToAdd = 0;
                    foreach (var epc in enhancementPointChances)
                    {
                        if (randomPoint < epc.chance)
                        {
                            pointsToAdd = epc.points;
                            break;
                        }
                        else
                        {
                            randomPoint -= epc.chance;
                        }
                    }
                    InventoryManager.Instance.AddEnhancementPointsToEquipment(weapon.itemNum, pointsToAdd);
                    enhancementPointsMap[weapon.itemNum] = pointsToAdd;
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
                resultUI.DisplayEquipmentResults(LastGachaResults, enhancementPointsMap);
            }
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }

    private void AutoEnhanceWeapon(WeaponObject weapon)
    {
        if (weapon == null) return;

        Debug.Log($"[EquipmentGachaManager] {weapon.itemName} (ID: {weapon.itemNum}) 자동 강화를 시작합니다.");

        const int levelUpCost = 10;
        const int maxLevel = 50;

        // 강화 가능 조건: 포인트 충분, 최대 레벨 미만, 성급업 대기 상태 아님
        bool needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
        bool canLevelUp = InventoryManager.Instance.GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;

        while (canLevelUp)
        {
            // 레벨업 시도
            bool levelUpSuccess = InventoryManager.Instance.LevelUpEquipment(weapon.itemNum);

            if (levelUpSuccess)
            {
                Debug.Log($"[EquipmentGachaManager] {weapon.itemName} 레벨업! 현재 레벨: {weapon.ItemLevel}");

                // 레벨업 후 조건 다시 확인
                needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
                canLevelUp = InventoryManager.Instance.GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;
            }
            else
            {
                // 레벨업 실패 (포인트 부족 등) - 루프 종료
                Debug.LogWarning($"[EquipmentGachaManager] {weapon.itemName} 레벨업 실패. 자동 강화를 중단합니다.");
                break;
            }
        }

        Debug.Log($"[EquipmentGachaManager] {weapon.itemName} 자동 강화를 종료합니다. 최종 레벨: {weapon.ItemLevel}");
    }
}