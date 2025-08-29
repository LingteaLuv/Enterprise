using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    [CreateAssetMenu(menuName = "Scriptable_Relics", fileName = "Scriptable_Relics/Relics")]
    public class ItemRelicsSO : ItemSO
    {
        public PowerType itemPowerType;
        public float[] upPower;
        public float[] cost;
        public float[] startPower;
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

}
