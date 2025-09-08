using JHT;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentListUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform weaponPanelParent;
    [SerializeField] private ItemEquipPanel itemEquipPanelPrefab;
    [SerializeField] private TMP_Dropdown weaponDropDown;
    [SerializeField] private EquipmentDetailPanel detailPanel;
    [SerializeField] private Button closeButton;

    private List<ItemEquipPanel> itemPanelPool = new List<ItemEquipPanel>();
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
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterDataUpdated;
        }

        if (weaponDropDown != null)
        {
            weaponDropDown.onValueChanged.AddListener(HandleSortChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnAddInventory -= HandleInventoryChanged;
            inventoryManager.OnRemoveInventory -= HandleInventoryChanged;
        }
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterDataUpdated;
        }

        if (weaponDropDown != null)
        {
            weaponDropDown.onValueChanged.RemoveListener(HandleSortChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
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
        RefreshDisplay();
    }

    public void ResetPanel()
    {
        currentCharacter = null;
        if (weaponDropDown != null) weaponDropDown.value = 0;
        ClearDisplay();
        if (detailPanel != null) detailPanel.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowForCharacter(PlayerCharacterData character, EquipCategory category)
    {
        this.currentCharacter = character;
        this.currentCategory = category;

        detailPanel.gameObject.SetActive(true);

        // Show details of the currently equipped item for this category, or show an empty panel.
        if (character.equippedItems.TryGetValue(category, out WeaponObject equippedItem))
        {
            detailPanel.ShowItem(equippedItem, character, category);
        }
        else
        {
            detailPanel.ShowEmpty(character, category);
        }

        RefreshDisplay();
    }

    private void HandleInventoryChanged(ItemObject changedItem)
    {
        if (currentCharacter != null)
        {
            RefreshDisplay();
        }
    }

    private void HandleCharacterDataUpdated(PlayerCharacterData updatedCharacter)
    {
        // If the currently displayed character's data changes, or if any character's equipment changes,
        // refresh the list to update equipped statuses.
        if (gameObject.activeInHierarchy)
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
        if (currentCharacter == null) return;

        var filteredList = FilterEquipment();
        displayedWeaponList = SortWeapons(filteredList);
        UpdateWeaponPanels();
    }

    private List<WeaponObject> FilterEquipment()
    {
        return inventoryManager.weaponList.Where(weapon =>
        {
            bool isCorrectCategory = weapon.equipCategory == this.currentCategory;
            // PlayerDataManager의 메서드를 호출하도록 수정
            bool isEquippableByRole = PlayerDataManager.Instance.IsEquippableByCharacter(weapon, currentCharacter);
            return isCorrectCategory && isEquippableByRole;
        }).ToList();
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

            panel.OnPanelClicked -= OnItemPanelClicked;
            panel.OnPanelClicked += OnItemPanelClicked;

            panel.gameObject.SetActive(true);
        }

        for (int i = displayedWeaponList.Count; i < itemPanelPool.Count; i++)
        {
            itemPanelPool[i].gameObject.SetActive(false);
        }
    }

    private void OnItemPanelClicked(WeaponObject weapon)
    {
        detailPanel.ShowItem(weapon, currentCharacter, currentCategory);
    }

    private void ClearDisplay()
    {
        foreach (var panel in itemPanelPool)
        {
            panel.gameObject.SetActive(false);
        }
    }
}
