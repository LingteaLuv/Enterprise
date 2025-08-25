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

        public Button oneAddButton;
        public Button twoAddButton;
        public Button threeAddButton;

        public Button oneRemoveButton;
        public Button twoRemoveButton;
        public Button threeRemoveButton;


        private void Start()
        {
            oneAddButton.onClick.AddListener(OnAdd1);
            twoAddButton.onClick.AddListener(OnAdd2);
            threeAddButton.onClick.AddListener(OnAdd3);

           // twoRemoveButton.onClick.AddListener(OnRemove2);
           // threeRemoveButton.onClick.AddListener(OnRemove3);

        }

        public void AddWeapon(ItemWeaponSO item)
        {
            InventoryManager.Instance.AddItem(item,WeaponRarity.E);
        }

        public void RemoveWeapon(ItemWeaponSO item)
        {
            //InventoryManager.Instance.RemoveItem(item);
        }

        private void OnAdd1() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item1.itemNum]);
        private void OnAdd2() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item2.itemNum]);
        private void OnAdd3() => AddWeapon(ItemDataManager.Instance.GetAllWeaponData()[item3.itemNum]);
        private void OnRemove1() => RemoveWeapon(item1);
        private void OnRemove2() => RemoveWeapon(item2);
        private void OnRemove3() => RemoveWeapon(item3);
    }
}
