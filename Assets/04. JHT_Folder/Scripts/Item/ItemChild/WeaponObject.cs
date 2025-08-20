using JHT;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace JHT
{
    public class WeaponObject : ItemObject
    {
        public ItemWeaponSO weapon;
        public int itemLevel;
        public int itemStar;
        public float weaponPower;

        public Action<WeaponObject> OnUpgrade;
        public Action<WeaponObject> OnUpCount;

        Dictionary<int, int[]> valueTable = null;

        public override void Init()
        {
            base.Init();
            weapon = itemSO as ItemWeaponSO;

            OnUpCount += UpCountWeapon;
            InventoryManager.Instance.OnUpCountItem += ItemLevelUp;

            valueTable = new();

            for (int i = 0; i < weapon.weaponClasses.Length; i++)
            {
                valueTable.Add(i, weapon.weaponClasses[i].levels);
            }
        }

        public void ItemUpCount(ItemWeaponSO weapon)
        {

        }

        // 레벨이 오를경우
        public void ItemLevelUp(WeaponObject obj)
        {
            if (obj != this)
                return;

            itemLevel++;
            OnUpCount?.Invoke(obj);

            //if (upCount >= weapon.MaxItemCount[itemLevel])
            //{
            //    //지금은 자동형식 팝업통해 아이템 업글할건지 체크
            //    Debug.Log("In");
            //    OnUpgrade?.Invoke(obj);
            //    upCount -= weapon.MaxItemCount[itemLevel];
            //    itemStar++;
            //}
        }


        // 레벨업시 아이템에 효과 줄 메서드
        public void UpgradeWeapon(WeaponObject item)
        {
            if (item != this)
                return;

            weaponPower = GetPower(item.itemStar, item.itemLevel);
        }

        public void UpCountWeapon(WeaponObject item)
        {
            if (item != this)
                return;

            weaponPower = GetPower(item.itemStar, item.itemLevel);
            Debug.Log($"WeaponPower : {GetPower(item.itemStar, item.itemLevel)}");
        }

        public float GetPower(int _star, int _upCount)
        {
            if (valueTable.TryGetValue(_star, out int[] values))
            {
                //Debug.Log($"딕셔너리 갯수: valueTable : {valueTable.Count} ----- values : {values.Length}");
                return values[_upCount];
            }
            else
            {
                Debug.LogError($"업카운트 {_upCount} 가 {values.Length} 범위를 벗어남");
                return 0f;
            }
        }
    }
}
