using System;
using UnityEngine;

namespace JHT
{
    public class ItemEventManager : Singleton<ItemEventManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public Action<ItemObject> OnClickItem;

        public void ClickItem(ItemObject obj)
        {
            OnClickItem?.Invoke(obj);
        }
        
        public float ItemLevelPower(ItemObject obj)
        {
            WeaponObject inst = (WeaponObject)obj;
            ItemWeaponSO so = (ItemWeaponSO)inst.itemSO;
            int rarity = 0;
            float power = 0;

            ItemRarity(inst);

            power = rarity * so.upPowerPercent[(int)inst.curRarity];
            return power;
        }


        public int ItemRarity(ItemObject obj)
        {
            WeaponObject inst = (WeaponObject)obj;
            int rarity = 0;

            switch ((int)inst.curRarity)
            {
                case 0:
                    rarity = 100;
                    break;
                case 1:
                    rarity = 200;
                    break;
                case 2:
                    rarity = 300;
                    break;
                case 3:
                    rarity = 400;
                    break;
                case 4:
                    rarity = 500;
                    break;
            }

            return rarity;
        }
    }
}
