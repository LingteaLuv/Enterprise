using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using System.Collections.Generic; // Dictionary 사용
using System.Numerics; // BigInteger 사용

public class BasicStatUI : MonoBehaviour
{
    [Header("스탯 UI 그룹 (공격력, 방어력, 체력 순서)")]
    public BasicStatUIGroup[] statUIGroups; // 각 스탯별 UI 요소를 담을 배열

    [Header("레벨업 배수 버튼")]
    public Button levelUpMultiplierButton; // x1 / x10 토글 버튼
    public TextMeshProUGUI levelUpMultiplierText; // 버튼 텍스트 (x1, x10)

    private int _levelsToGain = 1; // 한 번에 올릴 레벨 수 (1 또는 10)

    // UI 그룹을 인스펙터에서 설정하기 위한 Serializable 클래스
    [System.Serializable]
    public class BasicStatUIGroup
    {
        public BasicStatType statType; // 이 그룹이 담당하는 스탯 종류
        public TextMeshProUGUI statNameText; // 스탯 이름 (예: 공격력)
        public TextMeshProUGUI levelText; // 현재 레벨 (예: Lv. 1)
        public TextMeshProUGUI valueText; // 현재 스탯 값 (예: 100)
        public TextMeshProUGUI costText; // 레벨업 비용 (예: 1000 Gold)
        public HoldButton levelUpButton; // 레벨업 버튼
    }

    void Awake()
    {
        // 각 스탯 그룹의 레벨업 버튼에 리스너 연결
        foreach (var group in statUIGroups)
        {
            if (group.levelUpButton != null)
            {
                // 람다식을 사용하여 어떤 스탯의 버튼인지 전달
                group.levelUpButton.onHoldAction.AddListener(() => OnLevelUpButtonClicked(group.statType));
            }
        }

        // 레벨업 배수 버튼에 리스너 연결
        if (levelUpMultiplierButton != null)
        {
            levelUpMultiplierButton.onClick.AddListener(ToggleLevelsToGain);
        }

        // 초기 UI 갱신
        UpdateMultiplierButtonText();
    }

    void OnEnable()
    {
        // 패널이 활성화될 때마다 UI 갱신
        RefreshUI();
    }

    /// <summary>
    /// 모든 기본 스탯 UI를 갱신합니다.
    /// </summary>
    public void RefreshUI()
    {
        if (BasicStatManager.Instance == null)
        {
            Debug.LogError("BasicStatManager가 초기화되지 않았습니다.");
            return;
        }
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager가 초기화되지 않았습니다.");
            return;
        }

        foreach (var group in statUIGroups)
        {
            // 스탯 이름 표시 (선택 사항, 인스펙터에서 설정해도 됨)
            if (group.statNameText != null)
            {
                group.statNameText.text = GetStatDisplayName(group.statType);
            }

            // 현재 레벨 표시
            int currentLevel = BasicStatManager.Instance.GetStatLevel(group.statType);
            if (group.levelText != null)
            {
                group.levelText.text = $"Lv. {currentLevel}";
            }

            // 현재 스탯 값 표시
            float currentValue = BasicStatManager.Instance.GetStatValue(group.statType);
            if (group.valueText != null)
            {
                group.valueText.text = currentValue.ToString("F0"); // 소수점 없이 표시
            }

            // 레벨업 비용 표시
            BigInteger cost = BasicStatManager.Instance.GetLevelUpCost(group.statType, _levelsToGain);
            CurrencyType costType = CurrencyType.Gold; // 기본 스탯 레벨업 비용은 골드라고 가정

            if (group.costText != null)
            {
                group.costText.text = $"Cost: {DataUtility.FormatNumber(cost)} {costType}"; 
            }

            // 레벨업 버튼 활성화/비활성화
            if (group.levelUpButton != null)
            {
                BigInteger currentGold = InventoryManager.Instance.GetCurrency(costType);
                group.levelUpButton.interactable = (currentGold >= cost);
            }
        }
    }

    /// <summary>
    /// 레벨업 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnLevelUpButtonClicked(BasicStatType type)
    {
        if (BasicStatManager.Instance == null) return;

        bool success = BasicStatManager.Instance.TryLevelUpStat(type, _levelsToGain);
        if (success)
        {
            RefreshUI(); // 레벨업 성공 시 UI 갱신
            InventoryManager.Instance.UpdateCurrencyUI();
        }
    }

    /// <summary>
    /// 레벨업 배수 (x1 / x10)를 토글합니다.
    /// </summary>
    private void ToggleLevelsToGain()
    {
        _levelsToGain = (_levelsToGain == 1) ? 10 : 1;
        UpdateMultiplierButtonText();
        RefreshUI(); // 배수 변경 시 비용 갱신을 위해 UI 전체 갱신
    }

    /// <summary>
    /// 레벨업 배수 버튼의 텍스트를 갱신합니다.
    /// </summary>
    private void UpdateMultiplierButtonText()
    {
        if (levelUpMultiplierText != null)
        {
            levelUpMultiplierText.text = $"x{_levelsToGain}";
        }
    }

    // 스탯 타입에 따른 표시 이름 반환 (선택 사항)
    private string GetStatDisplayName(BasicStatType type)
    {
        switch (type)
        {
            case BasicStatType.Attack: return "ATK";
            case BasicStatType.Defense: return "DEF";
            case BasicStatType.Health: return "HP";
            default: return type.ToString();
        }
    }
}