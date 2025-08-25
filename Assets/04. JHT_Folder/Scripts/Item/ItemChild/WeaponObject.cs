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
    
        public Rarity rarity;

        private int itemLevel;
        public int ItemLevel { get { return itemLevel; } set { itemLevel = value; OnChangeLevel?.Invoke(itemLevel); } }
        public Action<int> OnChangeLevel = delegate { };

        public int itemStar = 0;
        public int ItemStar { get { return itemStar; } set { itemStar = value; OnChangeStar?.Invoke(itemStar); } }
        public Action<int> OnChangeStar = delegate { };

        private bool isUpgrade;
        public bool IsUpgrade { get { return isUpgrade; } set { isUpgrade = value; OnUpgrade?.Invoke(isUpgrade); } }
        public event Action<bool> OnUpgrade = delegate { };



        public WeaponObject(ItemWeaponSO sample)
        {
            itemIcon = sample.icon;
            itemName = sample.itemName;
            itemNum = sample.itemNum;
            itemSO = sample;
            rarity = sample.rarity;
        }

        
    }
}
