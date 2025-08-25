using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public List<WeaponObject> weaponList;
        public List<RelicsObject> relicsList;
        public float myMoney;

        public Action<ItemObject> OnAddInventory;
        public Action<ItemObject> OnRemoveInventory;
        public Action<WeaponObject> OnUpCountItem;

        public Action<RelicsObject, RelicsObject> OnChooseItem;
        public Action<RelicsObject, RelicsObject, bool> OnChangeItem;

        public InventoryMode currentMode;

        public void OnEnable()
        {
            if (weaponList == null)
                weaponList = new();

            if (relicsList == null)
                relicsList = new();

            OnAddInventory += AddInventroyItem;
            OnRemoveInventory += RemoveInventroyIndex;
            OnChangeItem += AddRelicsItem;
        }

        public void OnDisable()
        {
            OnAddInventory -= AddInventroyItem;
            OnRemoveInventory -= RemoveInventroyIndex;
            OnChangeItem -= AddRelicsItem;
        }

        public ItemObject AddItem(ItemSO item, ItemRarity rarity, int level = -1)
        {
            if (item.itemType == ItemType.Weapon)
            {
                WeaponObject obj = weaponList.Find(x => x.itemNum == item.itemNum);
                ItemWeaponSO so = (ItemWeaponSO)item;

                if (obj == null)
                {
                    obj = new WeaponObject(so, rarity);
                    OnAddInventory?.Invoke(obj);
                    OnUpCountItem?.Invoke(obj);
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
                RelicsObject obj = relicsList.Find(x => x.itemNum == item.itemNum);
                ItemRelicsSO so = (ItemRelicsSO)item;

                if (obj == null)
                {
                    obj = new RelicsObject(so, rarity, level);
                    OnAddInventory?.Invoke(obj);
                    return obj;
                }
                else
                {
                    OnChooseItem?.Invoke(obj, new RelicsObject(so, rarity, level));
                    return obj;
                }
            }
        }


        public void AddRelicsItem(RelicsObject obj1, RelicsObject obj2, bool value)
        {
            if (value)
            {
                RelicsObject inst = relicsList.Find(x => x.itemNum == obj1.itemNum);

                DestoryRelics(obj2);
                if (inst == null)
                {
                    relicsList.Add(obj1);
                }
            }
            else
            {
                DestoryRelics(obj1);

                relicsList.Add(obj2);
            }

        }

        public void DestoryRelics(RelicsObject obj)
        {
            myMoney += obj.itemCost;
            RemoveItem(obj);
        }

        public ItemObject GetItemData(ItemObject obj)
        {
            if (obj == null || obj.itemSO == null) return null;

            switch (obj.itemSO.itemType)
            {
                case ItemType.Weapon:
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

        public void RemoveItem(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Weapon)
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
            if (item.itemSO.itemType == ItemType.Weapon)
            {
                weaponList.Add((WeaponObject)item);
            }
            else
            {
                relicsList.Add((RelicsObject)item);
            }
        }

        private void RemoveInventroyIndex(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Weapon)
            {
                weaponList.Remove((WeaponObject)item);
            }
            else
            {
                relicsList.Remove((RelicsObject)item);
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