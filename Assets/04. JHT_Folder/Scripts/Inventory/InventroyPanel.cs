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

        [SerializeField] private WeaponStatPanel weaponStatPanel;

        private InventoryManager inventoryManager;
        private bool isLevelSort;

        //private List<ItemPanelPrefab> items;

        private void OnEnable()
        {
            inventoryManager = InventoryManager.Instance;
            inventoryManager.OnAddInventory += ReSetItemPanel;
            inventoryManager.OnRemoveInventory += ReSetItemPanel;
            levelSort.onClick.AddListener(SortByLevel);

            weaponInventoryButton.onClick.AddListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.AddListener(ChangeRelicsMode);
            ItemEventManager.Instance.OnClickItem += ShowWeaponStat;
        }

        private void OnDisable()
        {
            inventoryManager.OnAddInventory -= ReSetItemPanel;
            inventoryManager.OnRemoveInventory -= ReSetItemPanel;
            levelSort.onClick.RemoveListener(SortByLevel);

            weaponInventoryButton.onClick.RemoveListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.RemoveListener(ChangeRelicsMode);
            ItemEventManager.Instance.OnClickItem -= ShowWeaponStat;
        }
        
        private void SortByLevel()
        {
            isLevelSort = !isLevelSort;
            inventoryManager.WeaponLevelSort(isLevelSort);
            ReSetItemPanel(null);
        }

        private void ShowWeaponStat(ItemObject obj)
        {
            if (obj is WeaponObject)
            {
                WeaponObject inst = (WeaponObject)InventoryManager.Instance.GetItemData(obj);

                if(!weaponStatPanel.gameObject.activeSelf)
                    weaponStatPanel.gameObject.SetActive(true);

                weaponStatPanel.Init(inst);
            }
            
        }

        private void AddItem(ItemObject item)
        {
            ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
            obj.transform.SetParent(itemPanelParent);
            obj.Init(item);
        }


        //유물 or 무기 구분해서 sort
        private void ReSetItemPanel(ItemObject changedItem)
        {
            foreach (Transform child in itemPanelParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < inventoryManager.weaponList.Count; i++)
            {
                ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
                obj.transform.SetParent(itemPanelParent);
                obj.Init(inventoryManager.weaponList[i]);
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
