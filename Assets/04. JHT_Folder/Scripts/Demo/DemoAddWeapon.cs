using JHT;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    public class DemoAddWeapon : MonoBehaviour
    {

        [SerializeField] private ItemWeaponSO item1;
        [SerializeField] private ItemWeaponSO item2;
        [SerializeField] private ItemWeaponSO item3;

        [SerializeField] private ItemRelicsSO relics1;
        [SerializeField] private ItemRelicsSO relics2;
        [SerializeField] private ItemRelicsSO relics3;

        public Button oneAddButton;
        public Button twoAddButton;
        public Button threeAddButton;

        public Button oneAddRButton;
        public Button twoAddRButton;
        public Button threeAddRButton;


        private void Start()
        {
            oneAddButton.onClick.AddListener(OnAdd1);
            twoAddButton.onClick.AddListener(OnAdd2);
            threeAddButton.onClick.AddListener(OnAdd3);

            oneAddRButton.onClick.AddListener(OnRAdd1);
            twoAddRButton.onClick.AddListener(OnRAdd2);
            threeAddRButton.onClick.AddListener(OnRAdd3);
        }

        public void AddWeapon(ItemWeaponSO item)
        {
           // InventoryManager.Instance.AddItem(item,ItemRarity.Normal);
        }
        public void AddRelics1(ItemRelicsSO item)
        {
            InventoryManager.Instance.AddItem(item,ItemRarity.Normal,3);
        }

        public void AddRelics2(ItemRelicsSO item)
        {
            InventoryManager.Instance.AddItem(item, ItemRarity.Unique, 3);
        }
        public void AddRelics3(ItemRelicsSO item)
        {
            InventoryManager.Instance.AddItem(item, ItemRarity.Normal, 1);
        }

        private void OnAdd1() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item1.itemNum]);
        private void OnAdd2() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item2.itemNum]);
        private void OnAdd3() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item3.itemNum]);
        private void OnRAdd1() => AddRelics1(ItemDataManager.Instance.GetAllRelicsData()[relics1.itemNum]);
        private void OnRAdd2() => AddRelics2(ItemDataManager.Instance.GetAllRelicsData()[relics2.itemNum]);
        private void OnRAdd3() => AddRelics3(ItemDataManager.Instance.GetAllRelicsData()[relics3.itemNum]);
    }
}
