using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public enum ItemRarity
    {
        Normal = 1,
        Rare,
        Epic,
        Unique,
        Legend
    }
    [Serializable]
    public class RelicsObject : ItemObject
    {
        public float itemPower;
        public int itemLevel;
        public float itemCost;
        public Sprite itemRarityImage;
        public ItemRarity curRarity;
        public PowerType itemPowerType;
        public RelicsObject(ItemSO _so, ItemRarity _rarity, int level)
        {
            itemSO = _so;
            ItemRelicsSO so = (ItemRelicsSO)itemSO;

            itemPowerType = so.itemPowerType;
            curRarity = _rarity;
            itemLevel = level;
            itemIcon = so.icon;
            itemNum = so.itemNum;
            itemName = so.itemName;

            itemPower = so.upPower[(int)curRarity - 1] * level;
            itemCost = so.cost[(int)curRarity - 1] * level;
            itemRarityImage = so.rarityImage[(int)curRarity - 1];
        }
    }
}
