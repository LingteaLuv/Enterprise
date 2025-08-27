using JHT;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        [SerializeField] private ItemRelicsPanelItem itemRelicsPanelItem;

        [Header("Choose_Inventory")]
        [SerializeField] private GameObject weaponInventory;
        [SerializeField] private GameObject relicsInventory;
        [SerializeField] private GameObject soulInventory;
        [SerializeField] private Button equipModeButton;
        [SerializeField] private Button relicsModeButton;
        [SerializeField] private Button soulButton;

        [SerializeField] private WeaponStatPanel weaponStatPanel;
        [SerializeField] private RelicsStatPanel relicsStatPanel;


        [Header("DropDown")] 
        [SerializeField] private TMP_Dropdown weaponDropDown;
        [SerializeField] private TMP_Dropdown relicsDropDown;
        private List<string> weaponDropDownList;
        private List<string> relicsDropDownList;
        private InventoryMode curMode;

        private InventoryManager inventoryManager;
        private ItemEventManager itemEventManager;

        private List<ItemPanelPrefab> itemPanelPool = new List<ItemPanelPrefab>();
        private List<ItemRelicsPanelItem> itemRelicsPanelPool = new List<ItemRelicsPanelItem>();

        
        //private List<ItemPanelPrefab> items;

        private void OnEnable()
        {
            inventoryManager = InventoryManager.Instance;
            itemEventManager = ItemEventManager.Instance;

            inventoryManager.OnAddInventory += ReSetItemPanel;
            inventoryManager.OnRemoveInventory += ReSetItemPanel;

            inventoryManager.OnAddInventory += ReSetRelicsPanel;
            inventoryManager.OnChangePanel += ReSetRelicsPanel;

            equipModeButton.onClick.AddListener(ChangeWeaponMode);
            relicsModeButton.onClick.AddListener(ChangeRelicsMode);
            soulButton.onClick.AddListener(ChangeSoulMode);

            ItemEventManager.Instance.OnClickItem += ShowItemStat;

            ReSetItemPanel(null);
            ReSetRelicsPanel(null);
        }

        private void OnDisable()
        {
            inventoryManager.OnAddInventory -= ReSetItemPanel;
            inventoryManager.OnRemoveInventory -= ReSetItemPanel;

            inventoryManager.OnAddInventory -= ReSetRelicsPanel;
            inventoryManager.OnChangePanel -= ReSetRelicsPanel;

            equipModeButton.onClick.RemoveListener(ChangeWeaponMode);
            relicsModeButton.onClick.RemoveListener(ChangeRelicsMode);
            soulButton.onClick.RemoveListener(ChangeSoulMode);

            itemEventManager.OnClickItem -= ShowItemStat;
        }

        private void Start()
        {
            curMode = InventoryMode.Weapon;
            inventoryManager.currentMode = curMode;

            weaponDropDownList = new();
            relicsDropDownList = new();

            weaponDropDown.ClearOptions();
            relicsDropDown.ClearOptions();

            weaponDropDownList.Add("Power");
            weaponDropDownList.Add("Star");
            weaponDropDownList.Add("Level");
            weaponDropDownList.Add("Rarity");

            relicsDropDownList.Add("Level");
            relicsDropDownList.Add("Rarity");

            weaponDropDown.AddOptions(weaponDropDownList);
            relicsDropDown.AddOptions(relicsDropDownList);

            weaponDropDown.onValueChanged.AddListener(delegate { ChangeWeaponMode(weaponDropDown.value); });
            relicsDropDown.onValueChanged.AddListener(delegate { ChangeRelicsMode(relicsDropDown.value); });
        }
        
        private void ChangeRelicsMode(int value)
        {
            if (value == 0)
                SortByWeaponLevel();
            else if (value == 1)
                SortByWeaponRarity();
        }

        private void ChangeWeaponMode(int value)
        {
            if (value == 0)
                SortByPower();
            else if (value == 1)
                SortByStar();
            else if (value == 2)
                SortByWeaponLevel();
            else if (value == 3)
                SortByWeaponRarity();

        }

        #region SortUI
        private void SortByWeaponRarity()
        {
            inventoryManager.WeaponRarity();
            ReSetItemPanel(null);
        }

        private void SortByPower()
        {
            inventoryManager.WeaponPowerSort();
            ReSetItemPanel(null);
        }

        private void SortByWeaponLevel()
        {
            inventoryManager.WeaponLevelSort();
            ReSetItemPanel(null);
        }

        private void SortByStar()
        {
            inventoryManager.WeaponStarSort();
            ReSetItemPanel(null);
        }

        #endregion


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

        private void ReSetRelicsPanel(ItemObject changedItem)
        {
            // 1. 모든 풀링된 패널을 비활성화
            foreach (ItemRelicsPanelItem panel in itemRelicsPanelPool)
            {
                panel.gameObject.SetActive(false);
            }
            Debug.Log($"{inventoryManager.relicsList.Count}");
            // 2. 인벤토리 리스트를 기반으로 패널을 재사용/생성하여 업데이트
            for (int i = 0; i < inventoryManager.relicsList.Count; i++)
            {
                ItemRelicsPanelItem currentPanel;
                if (i < itemRelicsPanelPool.Count)
                {
                    // 풀에 사용 가능한 패널이 있으면 재사용
                    currentPanel = itemRelicsPanelPool[i];
                }
                else
                {
                    // 풀이 부족하면 새로 생성하고 풀에 추가
                    currentPanel = Instantiate(itemRelicsPanelItem);
                    currentPanel.transform.SetParent(relicsPanelParent);
                    itemRelicsPanelPool.Add(currentPanel);
                }

                RelicsObject relics = inventoryManager.relicsList[i];
                currentPanel.Init(relics);
                currentPanel.gameObject.SetActive(true);
            }
        }

        private void ChangeWeaponMode()
        {
            inventoryManager.currentMode = InventoryMode.Weapon;

            if (!weaponInventory.activeSelf)
                weaponInventory.SetActive(true);

            if (relicsInventory.activeSelf)
                relicsInventory.SetActive(false);

            if (soulInventory.activeSelf)
                soulInventory.SetActive(false);
        }

        private void ChangeRelicsMode()
        {
            inventoryManager.currentMode = InventoryMode.Relics;

            if (weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (!relicsInventory.activeSelf)
                relicsInventory.SetActive(true);

            if(soulInventory.activeSelf)
                soulInventory.SetActive(false); 
        }

        private void ChangeSoulMode()
        {
            inventoryManager.currentMode = InventoryMode.Soul;

            if(weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (relicsInventory.activeSelf)
                relicsInventory.SetActive(false);

            if (!soulInventory.activeSelf)
                soulInventory.SetActive(true);
        }
    }
}
