using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    [CreateAssetMenu(menuName = "Scriptable_Relics", fileName = "Scriptable_Relics/Relics")]
    public class ItemRelicsSO : ItemSO
    {
        public ItemRarity relicsRarity;
        public PowerType itemPowerType;
        public float[] upPower;
        public float[] cost;
        public Sprite[] rarityImage;
    }

    public enum PowerType
    {
        Attack,
        Shield,
        HealthUp,
        Critical,
        CriticalDamage,
        CriticalPercent,
        AttackSpeed,
        FromtShieldUp,
        AttackAmountUp,
        FrontHealthUp,
        CenterAttack,
        CenterAttackSpeed,
        LastHealthRecovery,
        LastestAttackUp
    }

    public enum ItemRarity
    {
        Normal = 1,
        Rare,
        Epic,
        Unique,
        Legend
    }
}
