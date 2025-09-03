using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JHT; // WeaponObject가 있는 네임스페이스

public class EquipmentDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemStatsText;

    [Header("Buttons")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;
    [SerializeField] private Button closeButton;

    private WeaponObject currentWeapon;
    private PlayerCharacterData currentCharacter;
    private EquipCategory currentCategory;

    private void OnEnable()
    {
        equipButton.onClick.AddListener(OnEquip);
        unequipButton.onClick.AddListener(OnUnequip);
        closeButton.onClick.AddListener(ClosePanel);
    }
    private void OnDisable()
    {
        equipButton.onClick?.RemoveListener(OnEquip);
        unequipButton.onClick.RemoveListener(OnUnequip);
        closeButton?.onClick?.RemoveListener(ClosePanel);
    }

    public void ShowPanel(WeaponObject weapon, PlayerCharacterData character, EquipCategory category)
    {
        if (weapon == null || character == null)
        {
            Debug.LogError("Weapon or Character data is null.");
            return;
        }

        this.currentWeapon = weapon;
        this.currentCharacter = character;
        this.currentCategory = category;

        gameObject.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        // 1. 기본 정보 표시
        // WeaponObject에 itemIcon, itemName, ItemLevel 프로퍼티가 있다고 가정합니다.
        // 만약 프로퍼티 이름이 다르거나 없다면, 실제 데이터에 맞게 수정해야 합니다.
        itemIcon.sprite = currentWeapon.itemIcon;
        itemNameText.text = currentWeapon.itemName;
        itemLevelText.text = $"Lv. {currentWeapon.ItemLevel}";

        // 2. 스탯 정보 표시 (임시)
        ItemWeaponSO weaponSO = (ItemWeaponSO)currentWeapon.itemSO;
        itemStatsText.text = $"{weaponSO.statType} {InventoryManager.Instance.GetWeaponStat(currentWeapon.itemNum)}% 증가";

        // 3. 버튼 상태 갱신
        bool isThisWeaponEquippedByAnyone = !string.IsNullOrEmpty(currentWeapon.EquippedByCharacterId);
        bool isThisWeaponEquippedByCurrentCharacter = false;
        if (currentCharacter.equippedItems.TryGetValue(currentWeapon.equipCategory, out WeaponObject equippedItem))
        {
            isThisWeaponEquippedByCurrentCharacter = equippedItem == currentWeapon;
        }

        unequipButton.gameObject.SetActive(isThisWeaponEquippedByCurrentCharacter);
        equipButton.gameObject.SetActive(!isThisWeaponEquippedByAnyone);
    }

    private void OnEquip()
    {
        bool success = PlayerDataManager.Instance.EquipItem(currentCharacter, currentWeapon);
        if (success)
        {
            Debug.Log($"{currentWeapon.itemName} 장착 성공!");
            ClosePanel();
        }
        else
        {
            Debug.LogWarning($"{currentWeapon.itemName} 장착 실패!");
        }
    }

    private void OnUnequip()
    {
        PlayerDataManager.Instance.UnequipItem(currentCharacter, currentWeapon.equipCategory);
        Debug.Log($"{currentWeapon.itemName} 장착 해제 성공!");

        // 장착 해제 후 UI를 갱신하여 '장착' 버튼이 다시 보이도록 함
        RefreshUI();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
