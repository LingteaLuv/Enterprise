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

        private List<ItemPanelPrefab> itemPanelPool = new List<ItemPanelPrefab>();

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

            ReSetItemPanel(null);
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

        private void ShowChooseItem(RelicsObject obj1, RelicsObject obj2)
        {
            choosePopup.gameObject.SetActive(true);
            choosePopup.Init(obj1, obj2);
        }

        private void ShowItemStat(ItemObject obj)
        {
            if (obj.itemSO.itemType == ItemType.Equip)
            {
                WeaponObject inst = (WeaponObject)InventoryManager.Instance.GetItemData(obj);

                if(!weaponStatPanel.gameObject.activeSelf)
                    weaponStatPanel.gameObject.SetActive(true);

                weaponStatPanel.ShowStats(inst);
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


        private void ReSetItemPanel(ItemObject changedItem)
        {
            // 1. 모든 풀링된 패널을 비활성화
            foreach (ItemPanelPrefab panel in itemPanelPool)
            {
                panel.gameObject.SetActive(false);
            }

            // 2. 인벤토리 리스트를 기반으로 패널을 재사용/생성하여 업데이트
            for (int i = 0; i < inventoryManager.weaponList.Count; i++)
            {
                ItemPanelPrefab currentPanel;
                if (i < itemPanelPool.Count)
                {
                    // 풀에 사용 가능한 패널이 있으면 재사용
                    currentPanel = itemPanelPool[i];
                }
                else
                {
                    // 풀이 부족하면 새로 생성하고 풀에 추가
                    currentPanel = Instantiate(itemPanelPrefab, weaponPanelParent);
                    itemPanelPool.Add(currentPanel);
                }

                WeaponObject weapon = inventoryManager.weaponList[i];
                currentPanel.SetUp(weapon);
                currentPanel.gameObject.SetActive(true);
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
