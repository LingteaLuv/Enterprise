using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace JHT
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemObject[] items;

        public Action<ItemObject,int> OnAddInventory; 
        public void Init()
        {
            items = new ItemObject[30];

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = null;
            }

            OnAddInventory += AddInventroyIndex;
        }

        public void AddItem(ItemObject item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    OnAddInventory?.Invoke(item,i);
                    break;
                }
            }
        }

        public void RemoveItem(ItemObject item)
        {

        }

        public void CleanItem(ItemObject item)
        {

        }

        public void SelectItem(ItemObject item)
        {

        }

        private void AddInventroyIndex(ItemObject item, int index)
        {
            items[index] = item;
        }
    }
}
