using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class InventroyPanel : MonoBehaviour
    {
        [Header("Spawn_ItemPanel")]
        [SerializeField] private Transform weaponPanelParent;
        [SerializeField] private Transform relicsPanelParent;
        [SerializeField] private ItemPanelPrefab itemPanelPrefab;
        [SerializeField] private Button levelSort;

        [Header("Choose_Inventory")]
        [SerializeField] private GameObject weaponInventory;
        [SerializeField] private GameObject relicsInventory;

        [SerializeField] private Button weaponInventoryButton;
        [SerializeField] private Button relicsInventoryButton;

        [SerializeField] private WeaponStatPanel weaponStatPanel;
        [SerializeField] private RelicsStatPanel relicsStatPanel;

        [Header("PopUp")]
        [SerializeField] private ChoosePopUp choosePopup;

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
            InventoryManager.Instance.OnChooseItem += ShowChooseItem;
            ItemEventManager.Instance.OnClickItem += ShowItemStat;
        }

        private void OnDisable()
        {
            inventoryManager.OnAddInventory -= ReSetItemPanel;
            inventoryManager.OnRemoveInventory -= ReSetItemPanel;
            levelSort.onClick.RemoveListener(SortByLevel);

            weaponInventoryButton.onClick.RemoveListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.RemoveListener(ChangeRelicsMode);
            InventoryManager.Instance.OnChooseItem -= ShowChooseItem;
            ItemEventManager.Instance.OnClickItem -= ShowItemStat;
        }
    
        private void SortByLevel()
        {
            isLevelSort = !isLevelSort;
            inventoryManager.WeaponLevelSort(isLevelSort);
            ReSetItemPanel(null);
        }

        private RelicsObject ShowChooseItem(RelicsObject obj1, RelicsObject obj2)
        {
            choosePopup.gameObject.SetActive(true);
            return choosePopup.Init(obj1, obj2);
        }

        private void ShowItemStat(ItemObject obj)
        {
            if (obj.itemSO.itemType == ItemType.Weapon)
            {
                WeaponObject inst = (WeaponObject)InventoryManager.Instance.GetItemData(obj);

                if(!weaponStatPanel.gameObject.activeSelf)
                    weaponStatPanel.gameObject.SetActive(true);

                weaponStatPanel.Init(inst);
            }
            else
            {
                RelicsObject inst = (RelicsObject)InventoryManager.Instance.GetItemData(obj);

                if (!relicsStatPanel.gameObject.activeSelf)
                    relicsStatPanel.gameObject.SetActive(true);

                relicsStatPanel.Init(inst);
            }
        }

        private void AddItem(ItemObject item)
        {
            ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
            obj.transform.SetParent(weaponPanelParent);
            obj.Init(item);
        }


        //유물 or 무기 구분해서 sort
        private void ReSetItemPanel(ItemObject changedItem)
        {
            if (changedItem is WeaponObject)
            {
                foreach (Transform child in weaponPanelParent)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < inventoryManager.weaponList.Count; i++)
                {
                    ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
                    obj.transform.SetParent(weaponPanelParent);
                    obj.Init(inventoryManager.weaponList[i]);
                }
            }
            else
            {
                foreach (Transform child in relicsPanelParent)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < inventoryManager.relicsList.Count; i++)
                {
                    ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
                    obj.transform.SetParent(relicsPanelParent);
                    obj.Init(inventoryManager.relicsList[i]);
                }
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
