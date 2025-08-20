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

            OnAddInventory += AddInventroyIndex;
            OnRemoveInventory += RemoveInventroyIndex;
        }

        public void AddItem(ItemObject item)
        {
            if (item as WeaponObject)
            {
                WeaponObject obj = item as WeaponObject;
                if (weaponList.Contains(obj))
                {
                    OnUpCountItem?.Invoke(obj);
                }
                else
                {
                    OnAddInventory?.Invoke(obj);
                    obj.Init();
                }
            }
            else
            {
                RelicsObject obj = item as RelicsObject;
            }
            
        }

        public void RemoveItem(ItemObject item)
        {
            if (item as WeaponObject)
            {
                WeaponObject obj = item as WeaponObject;
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
                RelicsObject obj = item as RelicsObject;
            }
            
        }

        public void WeaponCleanInventroy()
        {
            weaponList.Clear();
        }

        private void AddInventroyIndex(ItemObject item)
        {
            if (item as WeaponObject)
            {;
                weaponList.Add((WeaponObject)item);
            }
            else
            {
                RelicsObject obj = item as RelicsObject;
            }
        }

        private void RemoveInventroyIndex(ItemObject item)
        {
            if (item as WeaponObject)
            {
                WeaponObject obj = item as WeaponObject;
                weaponList.Remove(obj);
            }
            else
            {
                RelicsObject obj = item as RelicsObject;
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
