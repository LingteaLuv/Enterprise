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
        public float[] startPower;
        public Sprite[] rarityImage;
        public int[] cost;
    }

    public enum PowerType
    {
        Attack,
        Health,
        Defense,
        CritChance,
        CritDamage,
        AttackSpeed

        //Attack,
        //Shield,
        //HealthUp,
        //Critical,
        //CriticalDamage,
        //CriticalPercent,
        //AttackSpeed,
        //FromtShieldUp,
        //AttackAmountUp,
        //FrontHealthUp,
        //CenterAttack,
        //CenterAttackSpeed,
        //LastHealthRecovery,
        //LastestAttackUp
    }

}
