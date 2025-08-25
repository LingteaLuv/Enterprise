using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.Progress;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        // ▼▼▼ 강화 포인트 관련 코드 수정 ▼▼▼
        public Dictionary<int, int> equipmentEnhancementPoints = new Dictionary<int, int>();
        public Action<int, int> OnEquipmentEnhancementPointsChanged; // itemNum, newPoints

        public void AddEnhancementPointsToEquipment(int itemNum, int amount)
        {
            if (equipmentEnhancementPoints.ContainsKey(itemNum))
            {
                equipmentEnhancementPoints[itemNum] += amount;
            }
            else
            {
                equipmentEnhancementPoints.Add(itemNum, amount);
            }
            Debug.Log($"장비 ID: {itemNum} 에 강화 포인트 {amount} 추가! (총 {equipmentEnhancementPoints[itemNum]} 포인트)");
            OnEquipmentEnhancementPointsChanged?.Invoke(itemNum, equipmentEnhancementPoints[itemNum]);
        }

        public int GetEnhancementPoints(int itemNum)
        {
            if (equipmentEnhancementPoints.ContainsKey(itemNum))
            {
                return equipmentEnhancementPoints[itemNum];
            }
            return 0;
        }

        public void LevelUpEquipment(int itemNum)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon == null)
            {
                Debug.LogError($"[InventoryManager] LevelUpEquipment: itemNum {itemNum}에 해당하는 무기를 찾을 수 없습니다.");
                return;
            }

            int requiredPoints = 10;
            int currentPoints = GetEnhancementPoints(itemNum);

            if (currentPoints >= requiredPoints)
            {
                // 강화 포인트 차감
                AddEnhancementPointsToEquipment(itemNum, -requiredPoints);
                // 레벨업
                weapon.ItemLevel++;
                Debug.Log($"[InventoryManager] {weapon.itemName} 레벨업! (Lv.{weapon.ItemLevel - 1} -> Lv.{weapon.ItemLevel})");
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] {weapon.itemName} 레벨업 실패: 강화 포인트가 부족합니다. (필요: {requiredPoints}, 보유: {currentPoints})");
            }
        }

        public void StarUpEquipment(int itemNum)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon == null)
            {
                Debug.LogError($"[InventoryManager] StarUpEquipment: itemNum {itemNum}에 해당하는 무기를 찾을 수 없습니다.");
                return;
            }

            // 조건 없이 성급 증가
            weapon.ItemStar++;
            Debug.Log($"[InventoryManager] {weapon.itemName} 성급 증가! ({weapon.ItemStar - 1}성 -> {weapon.ItemStar}성)");
        }

        public float GetWeaponStat(int itemNum)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon == null)
            {
                Debug.LogError($"[InventoryManager] GetWeaponStat: itemNum {itemNum}에 해당하는 무기를 찾을 수 없습니다.");
                return 0f;
            }

            // 기본 스탯 15, 레벨당 0.1 증가
            float baseStat = 15f;
            float statPerLevel = 0.1f;
            float calculatedStat = baseStat + (weapon.ItemLevel * statPerLevel);

            return calculatedStat;
        }
        // ▲▲▲ 강화 포인트 관련 코드 수정 ▲▲▲

        public List<WeaponObject> weaponList;
        public List<RelicsObject> relicsObject;

        public Action<ItemObject> OnAddInventory;
        public Action<ItemObject> OnRemoveInventory;
        public Action<WeaponObject> OnUpCountItem;

        public InventoryMode currentMode;

        public void OnEnable()
        {
            if (weaponList == null)
                weaponList = new();

            if (relicsObject == null)
                relicsObject = new();

            OnAddInventory += AddInventroyWeapon;
            OnRemoveInventory += RemoveInventroyIndex;
        }

        public void OnDisable()
        {
            OnAddInventory -= AddInventroyWeapon;
            OnRemoveInventory -= RemoveInventroyIndex;
        }

        public WeaponObject AddItem(ItemWeaponSO item)
        {
            if (item == null)
            {
                Debug.LogError("AddItem 함수에 null인 ItemWeaponSO가 전달되었습니다!");
                return null;
            }


            if (item.itemType == ItemType.Equip)
            {


                WeaponObject obj = weaponList.Find(x => x.itemNum == item.itemNum);

                if (obj == null)
                {
                    obj = new WeaponObject(item);
                    OnAddInventory?.Invoke(obj);
                    return obj;
                }
                else
                {
                    OnUpCountItem?.Invoke(obj);
                    return obj;
                }

            }
            else
            {
                // [디버그 로그 추가]
                Debug.LogError($"[InventoryManager] {item.itemName}의 타입이 Equip이 아닙니다! 실제 타입: {item.itemType}. 아이템을 추가하지 않습니다.");
            }

            return null;
        }

        public ItemObject GetItemData(ItemObject obj)
        {
            if (obj == null || obj.itemSO == null) return null;

            switch (obj.itemSO.itemType)
            {
                case ItemType.Equip:
                    if (weaponList == null) return null;
                    // itemNum 일치하는 동일 무기 반환
                    return weaponList.Find(x => x.itemNum == obj.itemNum);

                case ItemType.Relics:
                    // TODO: relicsObject에서도 동일한 방식으로 찾기
                    // return relicsObject.Find(x => x.itemNum == obj.itemNum);
                    return null;
            }
            return null;
        }

        public void RemoveItem(ItemObject item)
        {
            if (item is WeaponObject obj)
            {
                if (weaponList.Contains(obj))
                {
                    OnRemoveInventory?.Invoke(item);
                }
                else
                {
                    Debug.Log($"해당 아이템 없음 : {obj.itemName} , {item.itemName}");
                }
            }
            else
            {
                //RelicsObject obj = item as RelicsObject;
            }

        }

        public void WeaponCleanInventroy()
        {
            weaponList.Clear();
        }

        private void AddInventroyWeapon(ItemObject item)
        {
            if (item is WeaponObject obj)
            {
                weaponList.Add(obj);
            }
            else
            {
                //RelicsObject obj = item as RelicsObject;
            }
        }

        private void RemoveInventroyIndex(ItemObject item)
        {
            if (item is WeaponObject obj)
            {
                weaponList.Remove(obj);
            }
            else
            {
                //RelicsObject obj = item as RelicsObject;
            }
        }


        public void WeaponLevelSort(bool isLevelSort)
        {
            weaponList.Sort((a, b) => isLevelSort ? a.ItemLevel.CompareTo(b.ItemLevel)          //오름
                                            : b.ItemLevel.CompareTo(a.ItemLevel));              //내림
        }


    }

    public enum InventoryMode
    {
        Weapon,
        Relics
    }

}