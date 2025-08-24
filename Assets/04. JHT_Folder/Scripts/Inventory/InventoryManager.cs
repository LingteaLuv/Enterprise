using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.Progress;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public List<WeaponObject> weaponList;
        public List<RelicsObject> relicsObject;

        public Action<ItemObject> OnAddInventory;
        public Action<ItemObject> OnRemoveInventory;
        public Action<WeaponObject> OnUpCountItem;

        public InventoryMode currentMode;

        public void OnEnable()
        {
            if(weaponList == null)
                weaponList = new();

            if(relicsObject == null)
                relicsObject = new();

            OnAddInventory += AddInventroyWeapon;
            OnRemoveInventory += RemoveInventroyIndex;
        }

        public void OnDisable()
        {
            OnAddInventory -= AddInventroyWeapon;
            OnRemoveInventory -= RemoveInventroyIndex;
        }

        public ItemObject AddItem(ItemWeaponSO item, WeaponRarity rarity)
        {
            if (item.itemType == ItemType.Weapon)
            {
                WeaponObject obj = weaponList.Find(x => x.itemNum == item.itemNum);
                
                if (obj == null)
                {
                    obj = new WeaponObject(item,rarity);
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
                //RelicsObject obj = item as RelicsObject;
            }

            return null;
        }

        public ItemObject GetItemData(ItemObject obj)
        {
            if (obj == null || obj.itemSO == null) return null;

            switch (obj.itemSO.itemType)
            {
                case ItemType.Weapon:
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


        public void aaaaaaa()
        {
            foreach (var w in weaponList)
            {
                Debug.Log($"이름 : {w.itemName},   공격력 : {w.weaponPower},   별 : {w.ItemStar},   아이템 레벨 : {w.ItemLevel}\n");
            }
        }
    }

    public enum InventoryMode
    {
        Weapon,
        Relics
    }

}
