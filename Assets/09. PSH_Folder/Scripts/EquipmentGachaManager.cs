
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

        ItemWeaponSO drawnWeaponSO = gachaPool[Random.Range(0, gachaPool.Count)];
        return InventoryManager.Instance.AddItem(drawnWeaponSO);
    }

    public override bool PerformMultipleGacha(int count)
    {
        // BaseGachaManager의 기본 뽑기 로직을 먼저 실행합니다.
        // 이 호출이 재화 소모 및 아이템 뽑기를 처리하고 LastGachaResults를 채웁니다.
        bool success = base.PerformMultipleGacha(count);

        if (success)
        {
            // 강화 포인트 확률의 총합을 계산합니다.
            float totalChance = enhancementPointChances.Sum(epc => epc.chance);
            if (totalChance <= 0)
            {
                Debug.LogError("[EquipmentGachaManager] 강화 포인트 확률의 총합이 0보다 작거나 같습니다!");
                return false; // 또는 기본값으로 처리
            }

            // 뽑힌 각 아이템에 대해 강화 포인트 로직을 실행합니다.
            foreach (var drawnItem in LastGachaResults)
            {
                if (drawnItem is WeaponObject weapon)
                {
                    float randomPoint = Random.Range(0, totalChance); // 0부터 totalChance 사이의 난수

                    int pointsToAdd = 0;
                    foreach (var epc in enhancementPointChances)
                    {
                        if (randomPoint < epc.chance)
                        {
                            pointsToAdd = epc.points;
                            break; // 해당 확률 구간에 속하면 루프 종료
                        }
                        else
                        {
                            randomPoint -= epc.chance;
                        }
                    }

                    // 획득한 포인트를 해당 장비의 ID와 함께 InventoryManager에 추가
                    InventoryManager.Instance.AddEnhancementPointsToEquipment(weapon.itemNum, pointsToAdd);
                }
                else
                {
                    Debug.LogWarning($"[EquipmentGachaManager] 뽑힌 아이템이 WeaponObject가 아닙니다: {drawnItem.GetType().Name}");
                }
            }
        }
        return success;
    }
}

