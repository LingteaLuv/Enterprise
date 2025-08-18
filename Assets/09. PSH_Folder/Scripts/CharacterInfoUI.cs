using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Numerics;

public class CharacterInfoUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI characterNameText;
    public Image characterImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI soulFragmentsText; // 캐릭터별 영혼 조각

    [Header("닫기 버튼")]
    public Button closeButton;

    [Header("캐릭터 레벨업 UI")]
    public TextMeshProUGUI levelUpCostText; // 캐릭터 레벨업 비용
    public HoldButton levelUpButton; // 캐릭터 레벨업 버튼 (HoldButton 타입)

    [Header("성급 업그레이드 UI")]
    public TextMeshProUGUI starUpgradeCostText; // 성급 업그레이드 비용 표시
    public Button starUpgradeButton; // 성급 업그레이드 버튼

    [Header("스탯 표시")]
    public TextMeshProUGUI hpDisplay; // HP LvX Value 형식
    public TextMeshProUGUI atkDisplay; // ATK LvX Value 형식

    private PlayerCharacterData currentCharacterData; // 현재 표시 중인 캐릭터 데이터

    private CharacterScrollViewUI scrollView;

    void Awake()
    {
        // 초기에는 비활성화
        gameObject.SetActive(false);

        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        // 캐릭터 레벨업 버튼 이벤트 연결 (HoldButton의 onHoldAction 사용)
        if (levelUpButton != null)
        {
            levelUpButton.onHoldAction.AddListener(OnLevelUpButtonClicked);
        }

        // 성급 업그레이드 버튼 이벤트 연결
        if (starUpgradeButton != null)
        {
            starUpgradeButton.onClick.AddListener(OnStarUpgradeButtonClicked);
        }

        scrollView = FindFirstObjectByType<CharacterScrollViewUI>();
    }

    /// <summary>
    /// 캐릭터 정보를 설정하고 패널을 활성화합니다.
    /// </summary>
    /// <param name="data">표시할 플레이어 캐릭터 데이터</param>
    public void Setup(PlayerCharacterData data)
    {
        currentCharacterData = data;

        if (data != null)
        {
            characterNameText.text = data.characterdata.characterName;
            characterImage.sprite = data.characterdata.characterSprite;
            levelText.text = $"Lv.{data.level}";

            // 캐릭터별 영혼 조각 UI 갱신
            UpdateSoulFragmentsUI();

            // 캐릭터 레벨업 UI (비용, 버튼 활성화/비활성화) 갱신
            UpdateLevelUpUI();

            // 성급 업그레이드 UI 갱신
            UpdateStarUpgradeUI();

            // 모든 캐릭터 스탯 표시 갱신
            UpdateCharacterStatsDisplay();

            // 패널 활성화
            gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("CharacterInfoUI: 전달된 캐릭터 데이터가 null입니다."); // 전달된 캐릭터 데이터가 null입니다.
            ClosePanel(); // 데이터가 없으면 패널 닫기
        }
    }

    /// <summary>
    /// 캐릭터의 영혼 조각 UI를 갱신합니다.
    /// </summary>
    private void UpdateSoulFragmentsUI()
    {
        if (currentCharacterData == null || soulFragmentsText == null) return;

        int characterId = currentCharacterData.characterdata.characterID;
        int currentFragments = 0;

        PlayerDataManager.Instance.characterSoulFragments.TryGetValue(characterId, out currentFragments);

        soulFragmentsText.text = $"Soul Fragments: {currentFragments}";
    }

    /// <summary>
    /// 캐릭터 레벨업 UI (비용, 버튼 활성화/비활성화)를 갱신합니다.
    /// </summary>
    private void UpdateLevelUpUI()
    {
        if (currentCharacterData == null || levelUpCostText == null || levelUpButton == null) return;

        // 실제 레벨에 따른 레벨업 비용 계산 (PlayerDataManager에서 가져옴)
        BigInteger cost = (BigInteger)((double)PlayerDataManager.Instance.baseLevelUpCost * System.Math.Pow(PlayerDataManager.Instance.levelUpCostIncreaseRatio, currentCharacterData.level - 1));
        CurrencyType costType = CurrencyType.Gold; // 비용 재화 타입 (현재는 골드로 고정)

        levelUpCostText.text = $"Cost: {DataUtility.FormatNumber(cost)} {costType}";

        // 재화가 충분한지 확인하여 버튼 활성화/비활성화
        if (InventoryManager.Instance != null && InventoryManager.Instance.GetCurrency(costType) >= cost)
        {
            levelUpButton.interactable = true;
        }
        else
        {
            levelUpButton.interactable = false;
        }
    }

    /// <summary>
    /// 성급 업그레이드 UI (비용, 버튼 활성화/비활성화)를 갱신합니다.
    /// </summary>
    private void UpdateStarUpgradeUI()
    {
        if (currentCharacterData == null || starUpgradeCostText == null || starUpgradeButton == null) return;

        // 최대 성급 확인 (예: 5성이 최대라고 가정)
        if (currentCharacterData.stars >= 5)
        {
            starUpgradeCostText.text = "MAX";
            starUpgradeButton.interactable = false;
            return;
        }

        // 다음 성급에 필요한 비용 가져오기
        int nextStarLevel = currentCharacterData.stars + 1;
        int cost = 0;
        if (PlayerDataManager.Instance.TryGetUpgradeCost(currentCharacterData.stars, out cost))
        {
            starUpgradeCostText.text = $"Cost: {cost} Soul Fragments";

            // 해당 캐릭터의 영혼 조각이 충분한지 확인하여 버튼 활성화/비활성화
            int currentFragments = 0;
            PlayerDataManager.Instance.characterSoulFragments.TryGetValue(currentCharacterData.characterdata.characterID, out currentFragments);

            if (currentFragments >= cost)
            {
                starUpgradeButton.interactable = true;
            }
            else
            {
                starUpgradeButton.interactable = false;
            }
        }
        else
        {
            starUpgradeCostText.text = "-"; // 정의되지 않은 성급
            starUpgradeButton.interactable = false;
        }
    }

    /// <summary>
    /// 모든 캐릭터 스탯 표시를 갱신합니다. (HP, ATK)
    /// </summary>
    private void UpdateCharacterStatsDisplay()
    {
        if (currentCharacterData == null) return;

        // HP 스탯 데이터 찾기
        StatData hpStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "HP");
        if (hpStatData != null)
        {
            BigInteger hpValue = StatManager.CalculateStatValue(hpStatData, currentCharacterData.level);
            hpDisplay.text = $"HP Lv{currentCharacterData.level} {DataUtility.FormatNumber(hpValue)}";
        }
        else
        {
            hpDisplay.text = "HP Stat Not Found"; // HP 스탯을 찾을 수 없습니다.
        }

        // ATK 스탯 데이터 찾기
        StatData atkStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "ATK");
        if (atkStatData != null)
        {
            BigInteger atkValue = StatManager.CalculateStatValue(atkStatData, currentCharacterData.level);
            atkDisplay.text = $"ATK Lv{currentCharacterData.level} {DataUtility.FormatNumber(atkValue)}";
        }
        else
        {
            atkDisplay.text = "ATK Stat Not Found"; // ATK 스탯을 찾을 수 없습니다.
        }
    }

    /// <summary>
    /// 캐릭터 레벨업 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnLevelUpButtonClicked()
    {
        if (currentCharacterData == null) return;

        bool success = PlayerDataManager.Instance.TryLevelUpCharacter(currentCharacterData);

        if (success)
        {
            // 레벨업 성공 시 UI 갱신
            Setup(currentCharacterData); // Setup을 다시 호출하여 모든 UI 요소 갱신

            if (scrollView != null)
            {
                scrollView.RefreshDisplay();
            }
            else
            {
                Debug.LogWarning("CharacterScrollViewUI를 찾을 수 없습니다. UI 갱신이 불완전할 수 있습니다."); // CharacterScrollViewUI를 찾을 수 없습니다. UI 갱신이 불완전할 수 있습니다.
            }
        }
    }

    /// <summary>
    /// 성급 업그레이드 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnStarUpgradeButtonClicked()
    {
        if (currentCharacterData == null) return;

        bool success = PlayerDataManager.Instance.TryUpgradeCharacterStar(currentCharacterData);

        if (success)
        {
            // 성급 업그레이드 성공 시 UI 갱신
            Setup(currentCharacterData); // Setup을 다시 호출하여 모든 UI 요소 갱신

            if (scrollView != null)
            {
                scrollView.RefreshDisplay();
            }
            else
            {
                Debug.LogWarning("CharacterScrollViewUI를 찾을 수 없습니다. UI 갱신이 불완전할 수 있습니다."); // CharacterScrollViewUI를 찾을 수 없습니다. UI 갱신이 불완전할 수 있습니다.
            }
        }
    }

    /// <summary>
    /// 캐릭터 정보 패널을 비활성화합니다.
    /// </summary>
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // 테스트용 레벨업 (비용 없이 레벨만 증가)
    public void LevelUp()
    {
        currentCharacterData.level++;
        scrollView.RefreshDisplay();
        UpdateLevelUpUI(); // 일관성을 위해 UpdateLevelUpUI 사용
    }
}