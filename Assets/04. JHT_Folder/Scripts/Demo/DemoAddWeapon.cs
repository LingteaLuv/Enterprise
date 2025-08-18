using JHT;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    public class DemoAddWeapon : MonoBehaviour
    {

        [SerializeField] private ItemObject item1;
        [SerializeField] private ItemObject item2;
        [SerializeField] private ItemObject item3;

        public Button oneAddButton;
        public Button twoAddButton;
        public Button threeAddButton;

        public Button oneRemoveButton;
        public Button twoRemoveButton;
        public Button threeRemoveButton;

        private event Action<ItemObject> OnClick;
        private void Start()
        {
            oneAddButton.onClick.AddListener(OnAdd1);
            twoAddButton.onClick.AddListener(OnAdd2);
            threeAddButton.onClick.AddListener(OnAdd3);

            oneRemoveButton.onClick.AddListener(OnRemove1);
            twoRemoveButton.onClick.AddListener(OnRemove2);
            threeRemoveButton.onClick.AddListener(OnRemove3);
        }

        public void AddWeapon(ItemObject item)
        {
            InventoryManager.Instance.AddItem(item);
        }

        public void RemoveWeapon(ItemObject item)
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
