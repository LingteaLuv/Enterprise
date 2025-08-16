using JHT;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    //구조 바꿔야함 상속으로 전체 아이템 담을거임
    public class ItemObject : MonoBehaviour
    {
        [field: SerializeField] public ItemSO weapon { get; private set; }
        private Image icon;

        public float attackRange;
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
            icon.sprite = weapon.icon;
            itemLevel = 1;
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
