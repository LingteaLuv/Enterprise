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
        public int itemCost;
        public Sprite itemRarityImage;
        public ItemRarity curRarity;
        public PowerType itemPowerType;
        public RelicsObject(ItemSO so, ItemRarity rarity, int level)
        {
            itemSO = so;
            ItemRelicsSO data = (ItemRelicsSO)itemSO;

            itemPowerType = data.itemPowerType;
            curRarity = rarity;
            itemLevel = level;
            itemIcon = data.icon;
            itemNum = data.itemNum;
            itemName = data.itemName;

            itemPower = data.startPower[(int)curRarity - 1] + data.upPower[(int)curRarity - 1] * level;
            itemCost = data.cost[(int)curRarity - 1];
            itemRarityImage = data.rarityImage[(int)curRarity - 1];
        }
        
        public RelicsObject(int id, ItemRarity rarity, int level)
        {
            itemSO = DataManager.Instance.AllRelics.Find(r => r.itemNum == id);
            ItemRelicsSO data = (ItemRelicsSO)itemSO;

            itemPowerType = data.itemPowerType;
            curRarity = rarity;
            itemLevel = level;
            itemIcon = data.icon;
            itemNum = data.itemNum;
            itemName = data.itemName;

            //itemPower = SetPower(data,level);
            itemPower = data.startPower[(int)curRarity - 1] + data.upPower[(int)curRarity - 1] * level;
            itemRarityImage = data.rarityImage[(int)curRarity - 1];
        }

        private float SetPower(ItemRelicsSO so, int level)
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
        }
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
