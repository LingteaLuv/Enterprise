using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    [CreateAssetMenu(menuName = "Scriptable_Relics", fileName = "Scriptable_Relics/Relics")]
    public class ItemRelicsSO : ItemSO
    {
        public RelicsPowerClasses[] relicsPower;
        public RelicsCostClasses[] relicsCost;
        public Sprite[] raityBackImage;



        public float GetPower(ItemRarity rarity, int level)
        {
            RelicsPowerClasses target = System.Array.Find(relicsPower, r => r.relicsRarity == rarity);

            if (target == null)
            {
                Debug.LogError($"{this.itemName}에 해당하는 값 없음");
                return 0;
            }

            if (level < 0 || level >= target.levels.Length)
            {
                return target.levels.Length - 1;
            }

            return target.levels[level];
        }

        public float GetCost(ItemRarity rarity, int level)
        {
            RelicsCostClasses target = System.Array.Find(relicsCost, r => r.relicsRarity == rarity);

            if (target == null)
            {
                Debug.LogError($"{this.itemName}에 해당하는 값 없음");
                return 0;
            }

            if (level < 0 || level >= target.levels.Length)
            {
                return target.levels.Length - 1;
            }

            return target.levels[level];
        }
    }

    [System.Serializable]
    public class RelicsPowerClasses
    {
        public ItemRarity relicsRarity;
        public float[] levels;
    }

    [System.Serializable]
    public class RelicsCostClasses
    {
        public ItemRarity relicsRarity;
        public float[] levels;
    }

}
