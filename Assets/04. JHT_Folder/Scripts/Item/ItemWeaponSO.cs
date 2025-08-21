using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{

    [System.Serializable]
    public class WeaponClass
    {
        public int star;
        public int[] levels;
    }

    [CreateAssetMenu(menuName = "Scriptable_Weapon", fileName = "Scriptable_Weapon/Weapon")]
    public class ItemWeaponSO : ItemSO
    {
        [field: SerializeField] public WeaponType weaponType { get; private set; }
        [field: SerializeField] public CharacterWeponType characterWeaponType { get; private set; }

        [field: SerializeField] public WeaponClass[] weaponClasses { get; private set; } = null;
        [field: SerializeField] public Image[] stars { get; private set; }
        [field: SerializeField] public float[] upPowerPercent { get; private set; }
        [field: SerializeField] public int[] maxLevelInCurStar { get; private set; }


        public override void UseItem(ItemSO item)
        {
            // 이전 아이템을 인벤토리에 넣고 해당 아이템을 넣음 -> Action으로 능력치 적용해야됨(여기서 말고 monobehaviour클래스)
        }
    }

    public enum WeaponType
    {
        Head = 0,
        Body,
        Weapon,
        Shield
    }

    // 이게 필요할까..? flags가
    [Flags]
    public enum CharacterWeponType //추후에 캐릭터 enum나오면 바꿀거임
    {
        Sword = 1 << 0,
        Bow = 2 << 1,
        Ax = 3 << 2
    }
}
