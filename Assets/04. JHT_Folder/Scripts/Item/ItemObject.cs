using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    //구조 바꿔야함 상속으로 전체 아이템 담을거임
    public class ItemObject : MonoBehaviour
    {
        [field: SerializeField] public ItemSO weapon { get; private set; }
        [field: SerializeField] public Sprite itemIcon { get; private set; }
        public float attackDamage;

        public int itemLevel;
        private int upCount;

        public event Action<ItemWeapon> OnUpgrade;
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            itemLevel = 1;
            attackDamage = weapon.attackDamage;
        }

        // 레벨이 오를경우
        public void ItemLevelUp(ItemWeapon weapon)
        {
            upCount++;
            if (upCount >= 100)
            {
                OnUpgrade?.Invoke(weapon);
                itemLevel++;
                upCount = 0;
            }
        }

        // 레벨업시 아이템에 효과 줄 메서드
        public void UpgradeWeapon(ItemWeapon weapon)
        {
            GameObject obj = weapon.levelParticle[itemLevel - 1];
        }
    }
}
