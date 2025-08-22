using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

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

        public void Start()
        {
            if(weaponList == null)
                weaponList = new();

            if(relicsObject == null)
                relicsObject = new();

            OnAddInventory += AddInventroyWeapon;
            OnRemoveInventory += RemoveInventroyIndex;
        }

        public void AddItem(DataItem item)
        {
            if (item.itemSO.itemType == ItemType.Weapon)
            {
                WeaponObject existObj = weaponList.Find(x => x.itemNum == item.itemNum);
                
                if (existObj == null)
                {
                    WeaponObject obj = new WeaponObject(item);
                    OnAddInventory?.Invoke(obj);
                }
                else
                {
                    OnUpCountItem?.Invoke(existObj);
                }
                
            }
            else
            {
                //RelicsObject obj = item as RelicsObject;
            }
            
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
            weaponList.Sort((a, b) => isLevelSort ? a.itemLevel.CompareTo(b.itemLevel)          //오름
                                            : b.itemLevel.CompareTo(a.itemLevel));              //내림
        }
    }
    public enum InventoryMode
    {
        Weapon,
        Relics
    }

}
