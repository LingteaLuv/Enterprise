using JHT;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EquipmentListUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform weaponPanelParent;
    [SerializeField] private ItemEquipPanel itemEquipPanelPrefab; // *** 타입 변경
    [SerializeField] private TMP_Dropdown weaponDropDown;
    [SerializeField] private EquipmentDetailPanel detailPanel;

    private List<ItemEquipPanel> itemPanelPool = new List<ItemEquipPanel>(); // *** 타입 변경
    private InventoryManager inventoryManager;

    private PlayerCharacterData currentCharacter;
    private EquipCategory currentCategory;
    private List<WeaponObject> displayedWeaponList = new List<WeaponObject>();

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager.Instance is null! EquipmentListUI를 비활성화합니다.");
            gameObject.SetActive(false);
            return;
        }
        if (detailPanel == null)
        {
            Debug.LogError("Detail Panel이 연결되지 않았습니다! EquipmentListUI를 비활성화합니다.");
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        inventoryManager.OnAddInventory += HandleInventoryChanged;
        inventoryManager.OnRemoveInventory += HandleInventoryChanged;

        if (weaponDropDown != null)
        {
            weaponDropDown.onValueChanged.AddListener(HandleSortChanged);
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnAddInventory -= HandleInventoryChanged;
            inventoryManager.OnRemoveInventory -= HandleInventoryChanged;
        }

        if (weaponDropDown != null)
        {
            weaponDropDown.onValueChanged.RemoveListener(HandleSortChanged);
        }
    }

    private void Start()
    {
        if (weaponDropDown != null)
        {
            var weaponDropDownList = new List<string> { "Power", "Star", "Level", "Rarity" };
            weaponDropDown.ClearOptions();
            weaponDropDown.AddOptions(weaponDropDownList);
        }
        ClearDisplay();
    }

    public void ShowForCharacter(PlayerCharacterData character, EquipCategory category)
    {
        this.currentCharacter = character;
        this.currentCategory = category;
        RefreshDisplay();
        detailPanel.gameObject.SetActive(false);
    }

    private void HandleInventoryChanged(ItemObject changedItem)
    {
        if (currentCharacter != null)
        {
            RefreshDisplay();
        }
    }

    private void HandleSortChanged(int sortIndex)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (currentCharacter == null)
        {
            ClearDisplay();
            return;
        }

        var filteredList = FilterEquipment();
        displayedWeaponList = SortWeapons(filteredList);
        UpdateWeaponPanels();
    }

    private List<WeaponObject> FilterEquipment()
    {
        // 1. 해당 카테고리의 장비만 필터링
        // 2. 다른 캐릭터가 장착하지 않은 장비만 필터링
        // 3. 현재 캐릭터가 장착할 수 있는 장비만 필터링 (직업 제한 등)
        return inventoryManager.weaponList.Where(weapon =>
        {
            bool isCorrectCategory = weapon.equipCategory == this.currentCategory;
            bool isNotEquipped = string.IsNullOrEmpty(weapon.EquippedByCharacterId);
            bool isEquippableByRole = IsEquippableByCharacter(weapon, currentCharacter);

            return isCorrectCategory && isNotEquipped && isEquippableByRole;
        }).ToList();
    }

    private bool IsEquippableByCharacter(WeaponObject weapon, PlayerCharacterData character)
    {
        if (weapon == null || character == null) return false;

        // Armor and Shields can be equipped by any role.
        if (weapon.equipCategory == EquipCategory.Armor || weapon.equipCategory == EquipCategory.Shield)
        {
            return true;
        }

        // For weapons, check the character's role.
        ItemWeaponSO weaponSO = (ItemWeaponSO)weapon.itemSO;
        EquipType type = weaponSO.equipType;
        CrewRole role = character.characterdata.crewRole;

        switch (role)
        {
            case CrewRole.Captain:
                return type == EquipType.Sword || type == EquipType.Gun;
            case CrewRole.Cook:
                return type == EquipType.Mace || type == EquipType.Staff;
            case CrewRole.Sailor:
                return type == EquipType.Bow || type == EquipType.Spear;
            case CrewRole.Deckhand:
                return type == EquipType.Axe || type == EquipType.Hammer;
            default:
                return false;
        }
    }

    private List<WeaponObject> SortWeapons(List<WeaponObject> weapons)
    {
        int sortIndex = weaponDropDown != null ? weaponDropDown.value : 0;
        switch (sortIndex)
        {
            case 0: return weapons.OrderByDescending(w => w.ItemPower).ToList();
            case 1: return weapons.OrderByDescending(w => w.ItemStar).ToList();
            case 2: return weapons.OrderByDescending(w => w.ItemLevel).ToList();
            case 3: return weapons.OrderByDescending(w => w.rarity).ToList();
            default: return weapons;
        }
    }

    private void UpdateWeaponPanels()
    {
        if (weaponPanelParent == null) return;

        while (itemPanelPool.Count < displayedWeaponList.Count)
        {
            ItemEquipPanel newPanel = Instantiate(itemEquipPanelPrefab, weaponPanelParent);
            itemPanelPool.Add(newPanel);
        }

        for (int i = 0; i < displayedWeaponList.Count; i++)
        {
            var panel = itemPanelPool[i];
            panel.SetUp(displayedWeaponList[i]);

            // 클릭 이벤트 구독
            panel.OnPanelClicked -= ShowDetailPanel; // 중복 구독 방지
            panel.OnPanelClicked += ShowDetailPanel;

            panel.gameObject.SetActive(true);
        }

        for (int i = displayedWeaponList.Count; i < itemPanelPool.Count; i++)
        {
            itemPanelPool[i].gameObject.SetActive(false);
        }
    }

    private void ShowDetailPanel(WeaponObject weapon)
    {
        detailPanel.ShowPanel(weapon, currentCharacter, currentCategory);
    }

    private void ClearDisplay()
    {
        foreach (var panel in itemPanelPool)
        {
            panel.gameObject.SetActive(false);
        }
    }
}