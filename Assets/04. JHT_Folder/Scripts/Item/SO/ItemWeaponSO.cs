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
        public Rarity rarity; // 1, 2, 3성 등급
        public EquipCategory equipCategory; // 분류 - 무기 방패 갑옷
        public EquipType equipType; // 세부분류 - 무기) 칼 도끼 활 등
        public Stat statType; // 장비가 올려주는 스탯 이름
        [field: SerializeField] public WeaponType weaponType { get; private set; }

        //[field: SerializeField] public CharacterWeponType characterWeaponType { get; private set; } //캐릭터 정보 가져오기
        
        [field: SerializeField] public Sprite[] starImage { get; private set; }
        [field: SerializeField] public float[] upPowerPercent { get; private set; }
        [field: SerializeField] public int[] maxLevelInCurStar { get; private set; }
        [field: SerializeField] public int[] upgradeCost { get; private set; }

        //[field: SerializeField] public WeaponClass[] weaponClasses { get; private set; } = null;

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
    public enum EquipCategory
    {
        Weapon, Shield, Armor
    }
    public enum EquipType
    {
        Sword, Axe, Bow, Hammer, Gun, Spear, Staff, Mace, Shield, Armor
    }
}
