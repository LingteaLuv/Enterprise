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
        public ItemRarity curRarity;

        public float itemPower;
        public int itemLevel;
        public float itemCost;
        public Sprite itemRarityImage;
        
        public RelicsObject(ItemSO _so, ItemRarity _rarity, int level)
        {
            itemSO = _so;
            ItemRelicsSO so = (ItemRelicsSO)itemSO;

            itemPower = so.GetPower(_rarity, level);
            itemCost = so.GetCost(_rarity, level);
            itemRarityImage = so.raityBackImage[(int)curRarity];
        }
    }
}
