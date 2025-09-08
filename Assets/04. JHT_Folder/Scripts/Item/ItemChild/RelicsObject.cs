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
        Legend,
        None
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

            itemPower = SetPower(so,level); 



            itemCost = so.cost[(int)curRarity - 1] * level;
            itemRarityImage = so.rarityImage[(int)curRarity - 1];
        }

        private float SetPower(ItemRelicsSO so,int level)
        {
            float amount = 0;
            if (itemPowerType == PowerType.Attack)
            {
                amount = so.startPower[(int)curRarity - 1] + so.upPower[(int)curRarity - 1] * level;
            }
            else
            {
                amount = 1 + so.startPower[(int)curRarity - 1] + so.upPower[(int)curRarity - 1] * level;
            }
            return amount;

            //switch (itemPowerType)
            //{
            //    case PowerType.Attack:
            //        amount = so.startPower[(int)curRarity - 1] + so.upPower[(int)curRarity - 1] * level;
            //        break;
            //    case PowerType.Health:
            //        amount = 1 + so.startPower[(int)curRarity - 1] + so.upPower[(int)curRarity - 1] * level;
            //        break;
            //    case PowerType.Defense:
            //
            //        break;
            //    case PowerType.CritChance:
            //
            //        break;
            //    case PowerType.CritDamage:
            //
            //        break;
            //    case PowerType.AttackSpeed:
            //
            //        break;
            //}
        }
    }
}
