
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JHT;

    // 장비 가챠의 로직을 담당하는 클래스
    public class EquipmentGachaManager : BaseGachaManager<ItemObject>
    {
        private List<ItemWeaponSO> gachaPool = new List<ItemWeaponSO>();

        private void Start()
        {
            // ItemDataManager로부터 전체 무기 리스트 받아오기
            StartCoroutine(InitializeGachaPool());
        }

        // ItemDataManager가 로딩될 때까지 기다린 후 gachaPool을 초기화합니다.
        private System.Collections.IEnumerator InitializeGachaPool()
        {
            // ItemDataManager가 로딩될 때까지 기다립니다.
            while (ItemDataManager.Instance == null || !ItemDataManager.Instance.IsDataLoaded)
            {
                yield return null;
            }
            gachaPool = ItemDataManager.Instance.GetAllWeaponData().Values.ToList();
            Debug.Log($"[EquipmentGachaManager] {gachaPool.Count}개의 무기를 뽑기 풀에 추가했습니다.");
        }

        /// <summary>
        /// BaseGachaManager로부터 특정 등급을 받아, 해당 등급의 아이템을 뽑는 실제 로직입니다.
        /// </summary>
        /// <param name="rarity">BaseGachaManager에서 결정된 등급</param>
        /// <returns>뽑힌 장비 아이템</returns>
        protected override ItemObject DrawItem(Rarity rarity)
        {
            if (gachaPool.Count == 0)
            {
                Debug.LogError("[EquipmentGachaManager] 뽑기 풀이 비어있습니다!");
                return null;
            }

            // 1. 전체 무기 풀에서 무작위로 무기 ScriptableObject를 하나 선택
            ItemWeaponSO drawnWeaponSO = gachaPool[Random.Range(0, gachaPool.Count)];

            // 2. BaseGachaManager에서 결정된 등급(rarity)과 무기 SO를 인벤토리에 추가하고, 생성된 WeaponObject를 반환
            // Rarity(BaseGachaManager)와 WeaponRarity(ItemWeaponSO)의 enum 값이 일치해야 합니다.
            return InventoryManager.Instance.AddItem(drawnWeaponSO);
        }
    }

