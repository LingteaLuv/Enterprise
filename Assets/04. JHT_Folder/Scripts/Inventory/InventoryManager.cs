using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;

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
        public Action<RelicsObject> OnChangeAddItem;
        public Action<RelicsObject,RelicsObject,bool> OnChangeItem;
        public Action<RelicsObject> OnChangePanel;

        public Action<ItemObject> OnAddItemForEncyclopedia;

        public Action<ItemObject> isSortingDone;

        public InventoryMode currentMode;

        public class ParsingRelicData
        {
            public int Level;
            public string Rarity;
        }
        
        public class ParsingWeaponData
        {
            public int Level;
            public int Point;
            public int Star;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            if (weaponList == null)
                weaponList = new();

            if (relicsList == null)
                relicsList = new();

        }

        private void Start()
        {
            AuthManager.Instance.LoginCompleted += InitDatabase;
        }

        private void OnEnable()
        {
            OnAddInventory += AddInventoryItem;
            OnRemoveInventory += RemoveInventoryIndex;
            OnChangeItem += AddRelicsItem;
            OnChangeAddItem += AddRelicsSolo;
        }

        private void OnDisable()
        {
            OnAddInventory -= AddInventoryItem;
            OnRemoveInventory -= RemoveInventoryIndex;
            OnChangeItem -= AddRelicsItem;
            OnChangeAddItem -= AddRelicsSolo;
        }

        #region 수현님코드
        // ▼▼▼ 강화 포인트 관련 코드 수정 ▼▼▼
        // todo석원 : 장비 DB 연동 - 강화 포인트
        //public Dictionary<int, int> equipmentEnhancementPoints = new Dictionary<int, int>();
        public Action<int, int> OnEquipmentEnhancementPointsChanged; // itemNum, newPoints
        public const int MAX_EQUIPMENT_LEVEL = 10;

        // 장비 조각
        public int equipmentFragments;

        public void AddEnhancementPointsToEquipment(int itemNum, int amount)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon.ItemStar >= 5)
            {
                equipmentFragments += amount;
            }
            else
            {
                /*if (equipmentEnhancementPoints.ContainsKey(itemNum))
                {
                    equipmentEnhancementPoints[itemNum] += amount;
                }
                else
                {
                    equipmentEnhancementPoints.Add(itemNum, amount);
                }*/
                weapon.enhancementPoint.Value += amount;
                Debug.Log($"장비 ID: {itemNum} 에 강화 포인트 {amount} 추가! (총 {weapon.enhancementPoint.Value} 포인트)");
                OnEquipmentEnhancementPointsChanged?.Invoke(itemNum, weapon.enhancementPoint.Value);
            }
        }
        
        public int GetEnhancementPoints(int itemNum)
        {
            /*if (equipmentEnhancementPoints.ContainsKey(itemNum))
            {
                return equipmentEnhancementPoints[itemNum];
            }
            return 0;*/
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            return weapon.enhancementPoint.Value;
        }

        /// <summary>
        /// 무기 성급에 따라 레벨업에 필요한 강화 포인트를 반환합니다.
        /// </summary>
        /// <param name="weapon">정보를 조회할 무기</param>
        /// <returns>필요 강화 포인트</returns>
        public int GetRequiredPointsForLevelUp(WeaponObject weapon)
        {
            if (weapon == null)
            {
                Debug.LogError("무기 정보가 없어 필요 포인트를 계산할 수 없습니다.");
                return int.MaxValue; // 오류 상황에서는 매우 큰 값을 반환하여 강화를 막습니다.
            }

            switch (weapon.ItemStar)
            {
                case 0:
                case 1:
                case 2:
                    return 100;
                case 3:
                    return 200;
                case 4:
                    return 300;
                default:
                    return 300;
            }
        }

        public bool LevelUpEquipment(int itemNum)
        {
            WeaponObject weapon = weaponList.Find(x => x.itemNum == itemNum);
            if (weapon == null)
            {
                Debug.LogError($"[InventoryManager] LevelUpEquipment: itemNum {itemNum}에 해당하는 무기를 찾을 수 없습니다.");
                return false;
            }

            // 5성 이상이면 레벨업 시도 자체를 막습니다.
            if (weapon.ItemStar >= 5)
            {
                Debug.LogWarning($"{weapon.itemName}은(는) 최종 성급이므로 레벨업할 수 없습니다.");
                return false;
            }

            int requiredPoints = GetRequiredPointsForLevelUp(weapon);
            int currentPoints = GetEnhancementPoints(itemNum);

            if (currentPoints >= requiredPoints)
            {
                AddEnhancementPointsToEquipment(itemNum, -requiredPoints);
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

            // 조건 없이 성급 증가 TODO 나중에 조건 추가
            weapon.ItemStar++;
            QuestSignalManager.Instance.RankUp(global::ItemType.Equipment, 1);

            // 5성이 되었는지 확인하는 로직
            if (weapon.ItemStar >= 5)
            {
                // 5성이 되면 레벨을 최대치로 설정
                weapon.ItemLevel = MAX_EQUIPMENT_LEVEL;
                Debug.Log($"[InventoryManager] {weapon.itemName} 최종 성급(5성) 도달! 레벨이 MAX({MAX_EQUIPMENT_LEVEL})로 고정됩니다.");

                // 남은 장비 포인트가 있다면 장비 조각으로 전환
                equipmentFragments += GetEnhancementPoints(itemNum);
                //equipmentEnhancementPoints[itemNum] = 0;
                weapon.enhancementPoint.Value = 0;
            }
            else
            {
                // 5성 미만일 때는 레벨을 0으로 초기화
                weapon.ItemLevel = 0;
                Debug.Log($"[InventoryManager] {weapon.itemName} 성급 증가! ({weapon.ItemStar - 1}성 -> {weapon.ItemStar}성), 레벨이 0으로 초기화되었습니다.");
            }

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

            // 5성일 경우, 레벨과 상관없이 고정 스탯 100을 반환합니다.
            if (weapon.ItemStar >= 5)
            {
                return 100f;
            }

            float baseStat;
            float statPerLevel;

            // 성급에 따라 시작 스탯과 레벨당 증가량을 다르게 설정합니다.
            switch (weapon.ItemStar)
            {
                case 0:
                    baseStat = 15f;
                    statPerLevel = 1f;
                    break;
                case 1:
                    baseStat = 25f;
                    statPerLevel = 1f;
                    break;
                case 2:
                    baseStat = 35f;
                    statPerLevel = 1.5f;
                    break;
                case 3:
                    baseStat = 50f;
                    statPerLevel = 2f;
                    break;
                case 4:
                    baseStat = 70f;
                    statPerLevel = 3f;
                    break;
                default:
                    baseStat = 100f;
                    statPerLevel = 0f;
                    break;
            }

            // 새로운 계산식을 적용합니다.
            float calculatedStat = baseStat + ((weapon.ItemLevel) * statPerLevel);
            return calculatedStat;
        }

        public void AutoEnhanceWeapon(WeaponObject weapon)
        {
            if (weapon == null) return;

            Debug.Log($"[InventoryManager] {weapon.itemName} (ID: {weapon.itemNum}) 자동 강화를 시작합니다.");

            const int maxLevel = MAX_EQUIPMENT_LEVEL;

            while (true)
            {
                int requiredPoints = GetRequiredPointsForLevelUp(weapon);

                bool needsStarUp = weapon.ItemLevel > 0 && weapon.ItemLevel % 10 == 0 && weapon.ItemStar < (weapon.ItemLevel / 10);
                bool canLevelUp = GetEnhancementPoints(weapon.itemNum) >= requiredPoints && weapon.ItemLevel < maxLevel && !needsStarUp;

                if (!canLevelUp)
                {
                    break; // 강화 조건을 만족하지 않으면 루프 탈출
                }

                // LevelUpEquipment 함수는 내부적으로 다시 필요 포인트를 계산하므로 안전합니다.
                if (!LevelUpEquipment(weapon.itemNum))
                {
                    break; // 레벨업에 실패하면 루프 탈출
                }
            }

            Debug.Log($"[InventoryManager] {weapon.itemName} 자동 강화를 종료합니다. 최종 레벨: {weapon.ItemLevel}");
        }
        // ▲▲▲ 강화 포인트 관련 코드 수정 ▲▲▲

        #endregion

        private void InitDatabase()
        {
            DatabaseManager.Instance.LoadRelicsAsync((result) =>
            {
                foreach (var kvp in result)
                {
                    int id = kvp.Key;
                    ParsingRelicData data = kvp.Value;
                    if (Enum.TryParse<ItemRarity>(data.Rarity, true, out ItemRarity rarity))
                    {
                        RelicsObject relic = new RelicsObject(id, rarity, data.Level);
                        relicsList.Add(relic);
                    }
                }
            });
            
            DatabaseManager.Instance.LoadWeaponsAsync((result) =>
            {
                foreach (var kvp in result)
                {
                    int id = kvp.Key;
                    ParsingWeaponData data = kvp.Value;
                    WeaponObject weapon = new WeaponObject(id, data.Level, data.Star, data.Point);
                    weaponList.Add(weapon);
                }
            });
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


        public void AddRelicsItem(RelicsObject obj1, RelicsObject obj2, bool value)
        {
            if (value)
            {
                DestroyRelics(obj2);
                relicsList.Add(obj1);
                DatabaseManager.Instance.SaveRelicDataAsync(obj1);
                OnChangePanel?.Invoke(obj1);
                PlayerDataManager.Instance.RecalculateAllCharacterStats();
            }
            else
            {
                DestroyRelics(obj1);
                relicsList.Add(obj2);
                DatabaseManager.Instance.SaveRelicDataAsync(obj2);
                OnChangePanel?.Invoke(obj2);
                PlayerDataManager.Instance.RecalculateAllCharacterStats();
            }
        }

        public void AddRelicsSolo(RelicsObject obj)
        {
            AddItem((ItemRelicsSO)obj.itemSO, obj.curRarity, obj.itemLevel);
        }

        public void AddItem(ItemRelicsSO item, ItemRarity rarity, int level = -1)
        {
            RelicsObject obj = new RelicsObject(item, rarity, level);
            OnAddInventory?.Invoke(obj);
            OnChangePanel?.Invoke(obj);
        }


        public void DestroyRelics(RelicsObject obj)
        {
            if(obj == null)
                return;

            int removed = relicsList.RemoveAll(r => r.itemNum == obj.itemNum);
            if (removed > 0)
            {
                RelicsPoints += obj.itemCost;
            }
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

        public void WeaponCleanInventory()
        {
            weaponList.Clear();
        }

        private void AddInventoryItem(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Equip)
            {
                weaponList.Add((WeaponObject)item);
                DatabaseManager.Instance.SaveWeaponDataAsync((WeaponObject)item);
                OnAddItemForEncyclopedia?.Invoke(item);
            }
            else
            {
                relicsList.Add((RelicsObject)item);
                DatabaseManager.Instance.SaveRelicDataAsync((RelicsObject)item);
                OnAddItemForEncyclopedia?.Invoke(item);
            }
        }

        private void RemoveInventoryIndex(ItemObject item)
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
