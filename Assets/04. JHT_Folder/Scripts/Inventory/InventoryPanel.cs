using JHT;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class InventoryPanel : UIBase
    {
        [Header("Spawn_ItemPanel")]
        [SerializeField] private Transform weaponPanelParent;
        [SerializeField] private Transform relicsPanelParent;
        [SerializeField] private ItemPanelPrefab itemPanelPrefab;
        [SerializeField] private ItemRelicsPanelItem itemRelicsPanelItem;

        [Header("Choose_Inventory")]
        [SerializeField] private GameObject weaponInventory;
        [SerializeField] private GameObject relicsInventory;
        [SerializeField] private SoulFragmentScrollViewUI soulInventory;
        [SerializeField] private Button equipModeButton;
        [SerializeField] private Button relicsModeButton;
        [SerializeField] private Button soulButton;

        [SerializeField] private WeaponStatPanel weaponStatPanel;
        [SerializeField] private RelicsStatPanel relicsStatPanel;

        [Header("DropDown")] 
        [SerializeField] private TMP_Dropdown weaponDropDown;
        [SerializeField] private TMP_Dropdown weaponNameDropdown;
        [SerializeField] private TMP_Dropdown relicsDropDown;
        [SerializeField] private TMP_Dropdown relicsNameDropdown;
        [SerializeField] private TMP_Dropdown soulDropDown;
        [SerializeField] private TMP_Dropdown soulNameDropdown;
        private List<string> weaponDropDownList;
        private List<string> relicsDropDownList;
        private List<string> weaponNameDropdownList;
        private List<string> relicsNameDropdownList;
        private List<string> soulDropdownList;
        private List<string> soulNameDropdownList;
        private InventoryMode curMode;

        private InventoryManager inventoryManager;
        private ItemEventManager itemEventManager;

        private List<ItemPanelPrefab> itemPanelPool = new List<ItemPanelPrefab>();

        private JHT_ObjectPool relicsPool;
        public override void ResetPanel()
        {
            base.ResetPanel();

            // 열려있을 수 있는 상세 정보 패널들을 모두 닫습니다.
            //if (weaponStatPanel != null) weaponStatPanel.gameObject.SetActive(false);
            //if (relicsStatPanel != null) relicsStatPanel.gameObject.SetActive(false);

            // 기본 탭인 장비 인벤토리로 되돌립니다.
            ChangeWeaponMode();

            Debug.Log("InventoryPanel이 리셋되어, 모든 상세창을 닫고 기본 탭으로 돌립니다.");
        }

        private void Awake()
        {
            if (relicsPool == null)
                relicsPool = new(itemRelicsPanelItem, 10, relicsPanelParent);

        }

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

            itemEventManager.OnClickItem += ShowItemStat;

            InventoryManager.Instance.isSortingDone += ReSetItemPanel;

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
            SetWeaponDropdown();
            SetRelicsDropdown();
            SetSoulDropdown();
        }
        
        private void SetWeaponDropdown()
        {
            weaponDropDownList = new();
            weaponNameDropdownList = new();

            weaponDropDown.ClearOptions();
            weaponNameDropdown.ClearOptions();

            weaponDropDownList.Add("전투력");
            weaponDropDownList.Add("별 레벨");
            weaponDropDownList.Add("레벨");
            weaponDropDownList.Add("희귀도");

            weaponNameDropdownList.Add("검");
            weaponNameDropdownList.Add("도끼");
            weaponNameDropdownList.Add("활");
            weaponNameDropdownList.Add("해머");
            weaponNameDropdownList.Add("총");
            weaponNameDropdownList.Add("창");
            weaponNameDropdownList.Add("스태프");
            weaponNameDropdownList.Add("메이스");
            weaponNameDropdownList.Add("방패");
            weaponNameDropdownList.Add("갑옷");

            weaponDropDown.AddOptions(weaponDropDownList);
            weaponNameDropdown.AddOptions(weaponNameDropdownList);
            weaponDropDown.onValueChanged.AddListener(delegate { ChangeWeaponMode(weaponDropDown.value); });
            weaponNameDropdown.onValueChanged.AddListener(delegate { InventoryManager.Instance.WeaponNameSort(weaponNameDropdown.value); });
        }

        private void SetSoulDropdown()
        {
            soulDropdownList = new();
            soulNameDropdownList = new();

            soulDropDown.ClearOptions();
            soulNameDropdown.ClearOptions();

            soulDropdownList.Add("전투력");
            soulDropdownList.Add("별레벨");
            soulDropdownList.Add("레벨");

            soulDropDown.AddOptions(soulDropdownList);
            soulDropDown.onValueChanged.AddListener(delegate { soulInventory.RefreshDisplayForValue(soulDropDown.value); });
        }

        private void SetRelicsDropdown()
        {
            relicsDropDownList = new();
            relicsNameDropdownList = new();

            relicsDropDown.ClearOptions();
            relicsNameDropdown.ClearOptions();

            relicsDropDownList.Add("레벨");
            relicsDropDownList.Add("희귀도");

            relicsDropDown.AddOptions(relicsDropDownList);

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

        #region weaponPanel spawn
        

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
        #endregion

        #region relisc Spawn

        private void ReSetRelicsPanel(ItemObject changedItem)
        {

            for (int i =0; i < inventoryManager.relicsList.Count; i++)
            {
                ItemRelicsPanelItem item = (ItemRelicsPanelItem)relicsPool.list[i];
                item.Outit(inventoryManager.relicsList[i]);
                relicsPool.list[i].Release();
            }

            for (int i = 0; i < inventoryManager.relicsList.Count; i++)
            {
                ItemRelicsPanelItem panel = relicsPool.GetPooled() as ItemRelicsPanelItem;
                panel.Init(inventoryManager.relicsList[i]);
            }
        }
        #endregion

        #region Soul Spawn


        #endregion


        #region InventoryMode
        private void ChangeWeaponMode()
        {
            inventoryManager.currentMode = InventoryMode.Weapon;

            if (!weaponInventory.activeSelf)
            {
                weaponInventory.SetActive(true);
            }

            if (relicsInventory.activeSelf)
                relicsInventory.SetActive(false);

            if (soulInventory.gameObject.activeSelf)
                soulInventory.gameObject.SetActive(false);
        }

        private void ChangeRelicsMode()
        {
            inventoryManager.currentMode = InventoryMode.Relics;

            if (weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (!relicsInventory.activeSelf)
                relicsInventory.SetActive(true);

            if(soulInventory.gameObject.activeSelf)
                soulInventory.gameObject.SetActive(false); 
        }

        private void ChangeSoulMode()
        {
            inventoryManager.currentMode = InventoryMode.Soul;

            if(weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (relicsInventory.activeSelf)
                relicsInventory.SetActive(false);

            if (!soulInventory.gameObject.activeSelf)
                soulInventory.gameObject.SetActive(true);
        }
        #endregion
    }
}
