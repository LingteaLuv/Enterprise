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
    public class WeaponObject : ItemObject
    {
        public int itemLevel;
        public int itemStar;
        public float weaponPower;
        private bool isUpgrade;
        public bool IsUpgrade { get { return isUpgrade; } set { isUpgrade = value; OnUpgrade?.Invoke(isUpgrade); } }
        public event Action<bool> OnUpgrade;

        public Action<WeaponObject> OnUpCount;

        public WeaponObject(DataItem sample)
        {
            itemIcon = sample.itemSO.icon;
            itemName = sample.itemSO.itemName;
            itemNum = sample.itemNum;
            itemSO = sample.itemSO;

            InventoryManager.Instance.OnUpCountItem += ItemLevelUp;
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

            itemLevel++;
            OnUpCount?.Invoke(obj);
            UpCountWeapon();
        }

        public WeaponObject OnAddItem(bool value)
        {
            return this;
        }

        // 방어코드 필요시 OnUpCount 구독 -> Weapon obj 넣어서
        public void UpCountWeapon()
        {
            ItemWeaponSO weapon = (ItemWeaponSO)itemSO;
            weaponPower += weapon.upPowerPercent[itemStar];
            //while (itemLevel > weapon.maxLevelInCurStar[itemStar])
            //{
            //    IsUpgrade = true;
            //}
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
