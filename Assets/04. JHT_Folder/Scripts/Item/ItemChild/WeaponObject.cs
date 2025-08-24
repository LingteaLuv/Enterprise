using JHT;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace JHT
{
    [Serializable]
    public class WeaponObject : ItemObject
    {
        public float weaponPower;
        public WeaponRarity curRarity;

        private int itemLevel;
        public int ItemLevel { get { return itemLevel;} set { itemLevel = value;  OnChangeLevel?.Invoke(itemLevel); } }
        public Action<int> OnChangeLevel;

        public int itemStar = 0;
        public int ItemStar { get { return itemStar; } set { itemStar = value; OnChangeStar?.Invoke(itemStar); } }
        public Action<int> OnChangeStar;

        private bool isUpgrade;
        public bool IsUpgrade { get { return isUpgrade; } set { isUpgrade = value; OnUpgrade?.Invoke(isUpgrade); } }
        public event Action<bool> OnUpgrade;

        public Action OnUpCount;
        public Action OnAddStar;

        public WeaponObject(ItemWeaponSO sample, WeaponRarity rarity)
        {
            itemIcon = sample.icon;
            itemName = sample.itemName;
            itemNum = sample.itemNum;
            itemSO = sample;
            curRarity = rarity;
            InventoryManager.Instance.OnUpCountItem += ItemLevelUp;
            OnAddStar += UpGradeItemForStar;
        }

        public void ItemUpCount(ItemWeaponSO weapon)
        {

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
            ItemLevel++;

            if (itemLevel > so.maxLevelInCurStar[itemLevel])
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
            ItemWeaponSO weapon = (ItemWeaponSO)itemSO;
            weaponPower += weapon.upPowerPercent[itemStar];
            if (itemLevel >= weapon.maxLevelInCurStar[itemStar])
            {
                IsUpgrade = true;
            }
        }

        private void UpGradeItemForStar()
        {
            ItemWeaponSO so = (ItemWeaponSO)itemSO;
            IsUpgrade = false;
            itemLevel -= so.maxLevelInCurStar[itemStar];
            ItemStar += 1;
            curRarity += (int)curRarity + 1;
            weaponPower = so.upPowerPercent[itemStar] * itemLevel;
        }

        public WeaponObject OnAddItem(bool value)
        {
            return this;
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
