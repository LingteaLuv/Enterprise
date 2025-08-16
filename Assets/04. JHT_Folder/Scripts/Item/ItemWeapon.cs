using System;
using UnityEngine;

namespace JHT
{
    public class ItemWeapon : ItemSO
    {
        [field: SerializeField] public WeaponType weaponType;
        [field: SerializeField] public CharacterWeponType characterWeaponType;
        AnimatorOverrideController animatorOverride; // Attack애니메이션 변경 character enum에 따라

        public GameObject[] levelParticle;

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
