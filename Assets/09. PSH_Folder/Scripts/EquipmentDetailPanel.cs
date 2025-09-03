using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JHT;
using System;

public class EquipmentDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private GameObject emptyPanel;

    [Header("Buttons")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;

    private WeaponObject currentWeapon;
    private PlayerCharacterData currentCharacter;
    private EquipCategory currentCategory;

    private void OnEnable()
    {
        equipButton.onClick.AddListener(OnEquip);
        unequipButton.onClick.AddListener(OnUnequip);
    }
    private void OnDisable()
    {
        equipButton.onClick?.RemoveListener(OnEquip);
        unequipButton.onClick.RemoveListener(OnUnequip);
    }

    public void ShowItem(WeaponObject weapon, PlayerCharacterData character, EquipCategory category)
    {
        if (weapon == null || character == null)
        {
            ShowEmpty(character, category);
            return;
        }

        this.currentWeapon = weapon;
        this.currentCharacter = character;
        this.currentCategory = category;

        emptyPanel.SetActive(false);
        itemIcon.gameObject.SetActive(true);
        RefreshUI();
    }

    public void ShowEmpty(PlayerCharacterData character, EquipCategory category)
    {
        this.currentWeapon = null;
        this.currentCharacter = character;
        this.currentCategory = category;

        emptyPanel.SetActive(true);
        itemIcon.gameObject.SetActive(false);
        itemNameText.text = "장비 없음";
        itemLevelText.text = "";
        itemStatsText.text = "";
        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if (currentWeapon == null)
        {
            ShowEmpty(currentCharacter, currentCategory);
            return;
        }

        itemIcon.sprite = currentWeapon.itemIcon;
        itemNameText.text = currentWeapon.itemName;
        itemLevelText.text = $"Lv. {currentWeapon.ItemLevel}";

        ItemWeaponSO weaponSO = (ItemWeaponSO)currentWeapon.itemSO;
        itemStatsText.text = $"{weaponSO.statType} {InventoryManager.Instance.GetWeaponStat(currentWeapon.itemNum)}% 증가";

        bool isEquippedByCurrent = currentCharacter.equippedItems.TryGetValue(currentCategory, out var equipped) && equipped == currentWeapon;

        equipButton.gameObject.SetActive(!isEquippedByCurrent);
        unequipButton.gameObject.SetActive(isEquippedByCurrent);
    }

    private void OnEquip()
    {
        if (currentWeapon == null || currentCharacter == null) return;

        // Check if the item is equipped by another character
        if (!string.IsNullOrEmpty(currentWeapon.EquippedByCharacterId))
        {
            if (int.TryParse(currentWeapon.EquippedByCharacterId, out int ownerId) && ownerId != currentCharacter.characterdata.characterID)
            {
                if (PlayerDataManager.Instance.ownedCharacters.TryGetValue(ownerId, out PlayerCharacterData owner))
                {
                    string message = $"{owner.characterdata.characterName}이(가) 사용 중인 장비입니다. 교체하시겠습니까?";
                    PopManager.Instance.ShowOKCancelPopup(message, "예", () => ProceedEquip(), "아니오");
                }
                else
                {   // Owner not found, proceed directly
                    ProceedEquip();
                }
            }
            else
            {   // Equipped by self or invalid ID, proceed directly
                ProceedEquip();
            }
        }
        else
        {
            // Not equipped by anyone
            ProceedEquip();
        }
    }

    private void ProceedEquip()
    {
        bool success = PlayerDataManager.Instance.EquipItem(currentCharacter, currentWeapon);
        if (success)
        {
            Debug.Log($"{currentWeapon.itemName} 장착 성공!");
            RefreshUI();
        }
        else
        {
            Debug.LogWarning($"{currentWeapon.itemName} 장착 실패!");
        }
    }

    private void OnUnequip()
    {
        if (currentWeapon == null || currentCharacter == null) return;

        PlayerDataManager.Instance.UnequipItem(currentCharacter, currentCategory);
        Debug.Log($"{currentWeapon.itemName} 장착 해제 성공!");

        ShowEmpty(currentCharacter, currentCategory);
    }
}
