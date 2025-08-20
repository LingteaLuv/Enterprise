using JHT;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    public class DemoAddWeapon : MonoBehaviour
    {

        [SerializeField] private WeaponObject item1;
        [SerializeField] private WeaponObject item2;
        [SerializeField] private WeaponObject item3;

        public Button oneAddButton;
        public Button twoAddButton;
        public Button threeAddButton;

        public Button oneRemoveButton;
        public Button twoRemoveButton;
        public Button threeRemoveButton;

        private event Action<WeaponObject> OnClick;
        private void Start()
        {
            oneAddButton.onClick.AddListener(OnAdd1);
            twoAddButton.onClick.AddListener(OnAdd2);
            threeAddButton.onClick.AddListener(OnAdd3);

            oneRemoveButton.onClick.AddListener(OnRemove1);
            twoRemoveButton.onClick.AddListener(OnRemove2);
            threeRemoveButton.onClick.AddListener(OnRemove3);
        }

        public void AddWeapon(WeaponObject item)
        {
            InventoryManager.Instance.AddItem(item);
        }

        public void RemoveWeapon(WeaponObject item)
        {
            InventoryManager.Instance.RemoveItem(item);
        }

        private void OnAdd1() => AddWeapon(item1);
        private void OnAdd2() => AddWeapon(item2);
        private void OnAdd3() => AddWeapon(item3);
        private void OnRemove1() => RemoveWeapon(item1);
        private void OnRemove2() => RemoveWeapon(item2);
        private void OnRemove3() => RemoveWeapon(item3);
    }
}
