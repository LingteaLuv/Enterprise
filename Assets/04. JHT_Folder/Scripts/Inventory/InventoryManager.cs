using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public List<ItemObject> itemList;

        public Action<ItemObject> OnAddInventory;
        public Action<ItemObject> OnRemoveInventory;

        public InventoryMode currentMode;
        public void Start()
        {
            if(itemList == null)
                itemList = new();

            OnAddInventory += AddInventroyIndex;
            OnRemoveInventory += RemoveInventroyIndex;
        }

        public void AddItem(ItemObject item)
        {
            OnAddInventory?.Invoke(item);
        }

        public void RemoveItem(ItemObject item)
        {
            OnRemoveInventory?.Invoke(item);
        }

        public void CleanInventroy()
        {
            itemList.Clear();
        }

        public void SelectItem(ItemObject item)
        {

        }

        private void AddInventroyIndex(ItemObject item)
        {
            itemList.Add(item);
        }

        private void RemoveInventroyIndex(ItemObject item)
        {
            itemList.Remove(item);
        }


        public void ItemLevelSort(bool isLevelSort)
        {
            itemList.Sort((a, b) => isLevelSort ? a.itemLevel.CompareTo(b.itemLevel)          //오름
                                            : b.itemLevel.CompareTo(a.itemLevel));            //내림
        }
    }
    public enum InventoryMode
    {
        Search,
        CheckForUpgrade,
        CheckForDelete
    }

}
