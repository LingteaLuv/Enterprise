using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    [Serializable]
    public class RelicsObject : ItemObject
    {
        public float itemPower;
        public int itemLevel;
        public float itemCost;
        public Sprite itemRarityImage;
        public ItemRarity curRarity;
        public RelicsObject(ItemSO _so, ItemRarity _rarity, int level)
        {
            itemSO = _so;
            ItemRelicsSO so = (ItemRelicsSO)itemSO;
            curRarity = _rarity;
            itemLevel = level;
            itemPower = so.upPower[(int)curRarity - 1] * level;
            itemCost = so.cost[(int)curRarity - 1] * level;
            itemRarityImage = so.rarityImage[(int)curRarity - 1];
        }
    }
}
