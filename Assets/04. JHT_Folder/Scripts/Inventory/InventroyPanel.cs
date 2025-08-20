using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class InventroyPanel : MonoBehaviour
    {
        [SerializeField] private Transform itemPanelParent;
        [SerializeField] private ItemPanelPrefab itemPanelPrefab;
        [SerializeField] private Button levelSort;

        [SerializeField] private GameObject weaponInventory;
        [SerializeField] private GameObject relicsInventory;

        [SerializeField] private Button weaponInventoryButton;
        [SerializeField] private Button relicsInventoryButton;

        private InventoryManager inventoryManager;
        private bool isLevelSort;

        private List<ItemPanelPrefab> items;
        private void OnEnable()
        {
            inventoryManager = InventoryManager.Instance;
            inventoryManager.OnAddInventory += AddItem;
            inventoryManager.OnRemoveInventory += RemoveItem;
            levelSort.onClick.AddListener(() => { isLevelSort = !isLevelSort; inventoryManager.WeaponLevelSort(isLevelSort); ReSetItemPanel(); });

            weaponInventoryButton.onClick.AddListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.AddListener(ChangeRelicsMode);
        }

        private void OnDisable()
        {
            inventoryManager.OnAddInventory -= AddItem;
            inventoryManager.OnRemoveInventory -= RemoveItem;
            levelSort.onClick.RemoveListener(() => { isLevelSort = !isLevelSort; inventoryManager.WeaponLevelSort(isLevelSort); ReSetItemPanel(); });

            weaponInventoryButton.onClick.RemoveListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.RemoveListener(ChangeRelicsMode);
        }

        private void Start()
        {
            items = new();
        }

        private void AddItem(ItemObject item)
        {
            ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
            obj.transform.SetParent(itemPanelParent);
            obj.Init(item);
            items.Add(obj);
        }

        private void RemoveItem(ItemObject item)
        {
            int idx = items.FindIndex(p => p != null && p.itemObject == item);
            if (idx < 0) return;

            var panel = items[idx];
            items.RemoveAt(idx);
            if (panel != null) Destroy(panel.gameObject);
        }

        //유물 or 무기 구분해서 sort
        private void ReSetItemPanel()
        {
            foreach (Transform child in itemPanelParent)
            {
                Destroy(child.gameObject);
            }
            items.Clear();

            for (int i = 0; i < inventoryManager.weaponList.Count; i++)
            {
                ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
                obj.transform.SetParent(itemPanelParent);
                obj.Init(inventoryManager.weaponList[i]);
                items.Add(obj);
            }
        }

        private void ChangeWeaponMode()
        {
            InventoryManager.Instance.currentMode = InventoryMode.Weapon;
            if (!weaponInventory.activeSelf)
                weaponInventory.SetActive(true);

            if (weaponInventory.activeSelf)
                relicsInventory.SetActive(false);
        }

        private void ChangeRelicsMode()
        {
            InventoryManager.Instance.currentMode = InventoryMode.Relics;
            if (weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (!weaponInventory.activeSelf)
                relicsInventory.SetActive(true);
        }
    }
}
