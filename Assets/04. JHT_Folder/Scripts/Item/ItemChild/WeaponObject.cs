using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    [Serializable]
    public class WeaponObject : ItemObject
    {
        
        public ItemRarity curRarity;

        public Rarity rarity;
        public string EquippedByCharacterId { get; set; } = null; // 장착한 캐릭터의 ID
        public EquipCategory equipCategory; // 분류 - 무기 방패 갑옷
        public EquipType equipType; // 세부분류 - 무기) 칼 도끼 활 등
        public Stat statType;

        private float itemPower;
        public float ItemPower { get { return itemPower; } set { itemPower = value; OnChangePower?.Invoke(itemPower); } }
        public Action<float> OnChangePower;

        private Property<int> itemLevel;
        public int ItemLevel { get { return itemLevel.Value;} set { itemLevel.Value = value;  OnChangeLevel?.Invoke(itemLevel.Value); } }
        public Action<int> OnChangeLevel;

        private Property<int> itemStar;
        public int ItemStar { get { return itemStar.Value; } set { itemStar.Value = value; OnChangeStar?.Invoke(itemStar.Value); } }
        public Action<int> OnChangeStar;

        private bool isUpgrade;
        public bool IsUpgrade { get { return isUpgrade; } set { isUpgrade = value; OnUpgrade?.Invoke(isUpgrade); } }
        public event Action<bool> OnUpgrade;

        public Action OnUpCount;
        public Action OnAddStar;

        public WeaponObject(ItemWeaponSO sample)
        {
            itemIcon = sample.icon;
            itemName = sample.itemName;
            itemNum = sample.itemNum;
            itemSO = sample;
            itemLevel = new Property<int>(0);
            itemLevel.OnChanged += (async (value) =>
            {
                await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Weapon/{itemNum}/Level", value);
            });
            itemStar = new Property<int>(0);
            itemStar.OnChanged += (async (value) =>
            {
                await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Weapon/{itemNum}/Star", value);
            });
            equipCategory = sample.equipCategory;
            equipType = sample.equipType;
            statType = sample.statType;
        }

        public WeaponObject(int id, int level, int star)
        {
            itemSO = DataManager.Instance.AllWeapons.Find(r => r.itemNum == id);
            ItemWeaponSO data = (ItemWeaponSO)itemSO;
            
            itemLevel = new Property<int>(level);
            itemLevel.OnChanged += (async (value) =>
            {
                await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Weapon/{id}/Level", value);
            });
            itemStar = new Property<int>(star);
            itemStar.OnChanged += (async (value) =>
            {
                await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Weapon/{id}/Star", value);
            });
            itemIcon = data.icon;
            itemNum = data.itemNum;
            itemName = data.itemName;

            equipCategory = data.equipCategory;
            equipType = data.equipType;
            statType = data.statType;
        }
        
        // 레벨이 오를경우
        public void ItemLevelUp(WeaponObject obj)
        {
            if (obj != this)
            {
                Debug.Log($"{obj.itemName} 없음 ");
                return;
            }

            ItemWeaponSO so = (ItemWeaponSO)obj.itemSO;
            itemLevel.Value++;

            if (itemLevel.Value > so.maxLevelInCurStar[itemStar.Value])
            {
                Debug.Log($"아이템이 현재 등급{itemStar} 최대 레벨");
                return;
            }

            UpCountWeapon();
        }

        // 방어코드 필요시 OnUpCount 구독 -> Weapon obj 넣어서
        // IsUpgrade 위치 변경필요
        public void UpCountWeapon()
        {
            ItemWeaponSO so = (ItemWeaponSO)itemSO;
            ItemPower += so.upPowerPercent[itemStar.Value];
            CheckUpgrade(so);
        }

        private void UpGradeItemForStar()
        {
            ItemWeaponSO so = (ItemWeaponSO)itemSO;

            itemLevel.Value -= so.maxLevelInCurStar[itemStar.Value];
            itemStar.Value += 1;
            curRarity += (int)curRarity + 1;
            if (itemLevel.Value > 0)
            {
                ItemPower = itemLevel.Value >= so.maxLevelInCurStar[itemStar.Value] ?
                    so.upPowerPercent[itemStar.Value] * so.maxLevelInCurStar[itemStar.Value] :
                    so.upPowerPercent[itemStar.Value] * itemLevel.Value;
            }
            else
            {
                ItemPower = so.upPowerPercent[itemStar.Value];
            }
            ItemLevel = this.ItemLevel;
            CheckUpgrade(so);
        }

        private void CheckUpgrade(ItemWeaponSO so)
        {
            if (itemLevel.Value >= so.maxLevelInCurStar[itemStar.Value])
                IsUpgrade = true;
            else
                IsUpgrade = false;
        }

        #region 우리애기

        //Dictionary<int, int[]> valueTable = null;

        //public void Init()
        //{
        //    valueTable = new();
        //    
        //    for (int i = 0; i < weapon.weaponClasses.Length; i++)
        //    {
        //        valueTable.Add(i, weapon.weaponClasses[i].levels);
        //    }
        //}

        // 레벨업시 아이템에 효과 줄 메서드
        //public void UpgradeWeapon(WeaponObject item)
        //{
        //    if (item != this)
        //        return;
        //
        //    weaponPower = GetPower(item.itemStar, item.itemLevel);
        //}

        //public float GetPower(int _star, int _upCount)
        //{
        //    if (valueTable.TryGetValue(_star, out int[] values))
        //    {
        //        return values[_upCount];
        //    }
        //    else
        //    {
        //        Debug.LogError($"업카운트 {_upCount} 가 {values.Length} 범위를 벗어남");
        //        return 0f;
        //    }
        //}
        #endregion
    }
}
