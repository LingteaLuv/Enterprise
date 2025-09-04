using UnityEngine;
using System.Collections.Generic;
using System;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public List<WeaponObject> weaponList;
        public List<RelicsObject> relicsList;

        [Header("유물재화")]
        private float relicsPoints;
        private int relicsCoupon;
        public float RelicsPoints { get { return relicsPoints; } set { relicsPoints = value; OnChangeRelicsPoints?.Invoke(relicsPoints); } }
        public int RelicsCoupon { get { return relicsCoupon; } set {  relicsCoupon = value; OnChangeRelicsCoupon?.Invoke(relicsCoupon); } }
        public Action<int> OnChangeRelicsCoupon;
        public Action<float> OnChangeRelicsPoints;

        public Action<ItemObject> OnAddInventory;
        public Action<ItemObject> OnRemoveInventory;
        public Action<WeaponObject> OnUpCountItem;

        public Action<RelicsObject,RelicsObject> OnChooseItem;
        public Action<RelicsObject,RelicsObject,bool> OnChangeItem;
        public Action<RelicsObject> OnChangePanel;

        public Action<ItemObject> OnAddItemForEncyclopedia;

        public Action<ItemObject> isSortingDone;


        public InventoryMode currentMode;

        protected override void Awake()
        {
            base.Awake();
            
            if (weaponList == null)
                weaponList = new();

            if (relicsList == null)
                relicsList = new();

        }

        private void OnEnable()
        {
            OnAddInventory += AddInventroyItem;
            OnRemoveInventory += RemoveInventroyIndex;
            OnChangeItem += AddRelicsItem;
        }

        private void OnDisable()
        {
            OnAddInventory -= AddInventroyItem;
            OnRemoveInventory -= RemoveInventroyIndex;
            OnChangeItem -= AddRelicsItem;
        }

        #region 수현님코드
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

        public bool LevelUpEquipment(int itemNum)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon == null)
            {
                Debug.LogError($"[InventoryManager] LevelUpEquipment: itemNum {itemNum}에 해당하는 무기를 찾을 수 없습니다.");
                return false;
            }

            int requiredPoints = 10;
            int currentPoints = GetEnhancementPoints(itemNum);

            if (currentPoints >= requiredPoints)
            {
                // 강화 포인트 차감
                AddEnhancementPointsToEquipment(itemNum, -requiredPoints);
                // 레벨업
                weapon.ItemLevel++;
                QuestSignalManager.Instance.LevelUp(global::ItemType.Equipment, 1);
                
                Debug.Log($"[InventoryManager] {weapon.itemName} 레벨업! (Lv.{weapon.ItemLevel - 1} -> Lv.{weapon.ItemLevel})");
                return true;
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] {weapon.itemName} 레벨업 실패: 강화 포인트가 부족합니다. (필요: {requiredPoints}, 보유: {currentPoints})");
                return false;
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

            // 성급업 후 자동 강화 시도
            AutoEnhanceWeapon(weapon);
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

        public void AutoEnhanceWeapon(WeaponObject weapon)
        {
            if (weapon == null) return;

            Debug.Log($"[InventoryManager] {weapon.itemName} (ID: {weapon.itemNum}) 자동 강화를 시작합니다.");

            const int levelUpCost = 10; // 레벨업 비용
            const int maxLevel = 50; // 최대 레벨

            // 강화 가능 조건: 포인트 충분, 최대 레벨 미만, 성급업 대기 상태 아님
            bool needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
            bool canLevelUp = GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;

            while (canLevelUp)
            {
                // 레벨업 시도
                bool levelUpSuccess = LevelUpEquipment(weapon.itemNum);

                if (levelUpSuccess)
                {
                    Debug.Log($"[InventoryManager] {weapon.itemName} 레벨업! 현재 레벨: {weapon.ItemLevel}");

                    // 레벨업 후 조건 다시 확인
                    needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
                    canLevelUp = GetEnhancementPoints(weapon.itemNum) >= levelUpCost && weapon.ItemLevel < maxLevel && !needsStarUp;
                }
                else
                {
                    // 레벨업 실패 (포인트 부족 등) - 루프 종료
                    Debug.LogWarning($"[InventoryManager] {weapon.itemName} 레벨업 실패. 자동 강화를 중단합니다.");
                    break;
                }
            }

            Debug.Log($"[InventoryManager] {weapon.itemName} 자동 강화를 종료합니다. 최종 레벨: {weapon.ItemLevel}");
        }
        // ▲▲▲ 강화 포인트 관련 코드 수정 ▲▲▲

        #endregion

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

        public void AddItem(ItemRelicsSO item, ItemRarity rarity,int level = -1)
        {
            RelicsObject obj = new RelicsObject(item, rarity, level);
            OnAddInventory?.Invoke(obj);
            OnChangePanel?.Invoke(obj);
        }


        public void AddRelicsItem(RelicsObject obj1, RelicsObject obj2, bool value)
        {
            if (value)
            {
                DestoryRelics(obj2);
                relicsList.Add(obj1);
                OnChangePanel?.Invoke(obj1);
                
            }
            else
            {
                DestoryRelics(obj1);
                relicsList.Add(obj2);
                OnChangePanel?.Invoke(obj2);
            }
            
        }

        public void DestoryRelics(RelicsObject obj)
        {
            if (obj == null)
                return;
            RelicsPoints += obj.itemCost;
            relicsList.RemoveAll(r => r.itemNum == obj.itemNum);
        }


        public ItemObject GetItemData(ItemObject obj)
        {
            if (obj == null || obj.itemSO == null) return null;

            switch (obj.itemSO.itemType)
            {
                case ItemType.Equip:
                    if (weaponList == null) 
                        return null;

                    return weaponList.Find(x => x.itemNum == obj.itemNum);

                case ItemType.Relics:
                    if (relicsList == null)
                        return null;

                    return relicsList.Find(x => x.itemNum == obj.itemNum);
            }
            return null;
        }

        // relics는 이제 안씀
        public void RemoveItem(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Equip)
            {
                if (weaponList.Contains((WeaponObject)item))
                {
                    OnRemoveInventory?.Invoke(item);
                }
                else
                {
                    Debug.Log($"해당 아이템 없음 : {item.itemName}");
                }
            }
            else
            {
                //relicsList.RemoveAll(r => r.itemNum == item.itemNum);
                if (relicsList.Contains((RelicsObject)item))
                {
                    OnRemoveInventory?.Invoke(item);
                }
                else
                {
                    Debug.Log($"해당 아이템 없음 : {item.itemName}");
                }
            }
            
        }

        public void WeaponCleanInventroy()
        {
            weaponList.Clear();
        }

        private void AddInventroyItem(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Equip)
            {
                weaponList.Add((WeaponObject)item);
                OnAddItemForEncyclopedia?.Invoke(item);
            }
            else
            {
                relicsList.Add((RelicsObject)item);
                OnAddItemForEncyclopedia?.Invoke(item);
            }
        }

        private void RemoveInventroyIndex(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Equip)
            {
                weaponList.Remove((WeaponObject)item);
            }
            else
            {
                relicsList.Remove((RelicsObject)item);
            }
        }

        #region sort

        public void WeaponNameSort(int value)
        {
            if (weaponList == null || weaponList.Count <= 1) return;


            List<WeaponObject> front = new(weaponList.Count);
            List<WeaponObject> back = new(weaponList.Count);

            for (int i = 0; i < weaponList.Count; i++)
            {
                if ((int)weaponList[i].equipType == value)
                    front.Add(weaponList[i]);
                else
                    back.Add(weaponList[i]);
            }

            weaponList.Clear();
            weaponList.AddRange(front);
            weaponList.AddRange(back);
            isSortingDone?.Invoke(null);

        }


        public void WeaponStarSort()
        {
            weaponList.Sort((b,a) => a.ItemStar.CompareTo(b.ItemStar));          
        }

        public void WeaponLevelSort()
        {
            if (currentMode == InventoryMode.Weapon)
            {
                weaponList.Sort((b,a) => a.ItemLevel.CompareTo(b.ItemLevel));            
            }
            else if(currentMode == InventoryMode.Relics)
            {
                relicsList.Sort((b,a) => a.itemLevel.CompareTo(b.itemLevel));      
            }
        }

        public void WeaponPowerSort()
        {
            weaponList.Sort((b,a) =>  a.ItemPower.CompareTo(b.ItemPower));             
        }

        public void WeaponRarity()
        {
            if (currentMode == InventoryMode.Weapon)
            {
                weaponList.Sort((b, a) => a.rarity.CompareTo(b.rarity));
            }
            else if (currentMode == InventoryMode.Relics)
            {
                relicsList.Sort((b, a) => a.curRarity.CompareTo(b.curRarity));
            }
        }
#endregion
    }
    public enum InventoryMode
    {
        Weapon,
        Relics,
        Soul
    }

}
