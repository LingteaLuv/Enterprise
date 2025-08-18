using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// using System.Numerics; // BigInteger를 사용하지 않으므로 제거

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
    public TextMeshProUGUI defDisplay;
    public TextMeshProUGUI criChanDisplay;
    public TextMeshProUGUI criDmgDisplay;
    public TextMeshProUGUI atkSpdDisplay;

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

        // PlayerDataManager.Instance.characterSoulFragments.TryGetValue(characterId, out currentFragments);
        // PlayerDataManager.Instance가 BigInteger를 사용한다면, currentFragments도 BigInteger여야 합니다.
        // 현재는 int로 가정합니다.
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.characterSoulFragments.TryGetValue(characterId, out currentFragments))
        {
            soulFragmentsText.text = $"Soul Fragments: {currentFragments}";
        }
        else
        {
            soulFragmentsText.text = "Soul Fragments: 0"; // 데이터를 찾을 수 없을 때 기본값
        }
    }

    /// <summary>
    /// 캐릭터 레벨업 UI (비용, 버튼 활성화/비활성화)를 갱신합니다.
    /// </summary>
    private void UpdateLevelUpUI()
    {
        if (currentCharacterData == null || levelUpCostText == null || levelUpButton == null) return;

        // 실제 레벨에 따른 레벨업 비용 계산 (PlayerDataManager에서 가져옴)
        // PlayerDataManager.Instance.baseLevelUpCost가 BigInteger라면 cost도 BigInteger
        // PlayerDataManager.Instance.baseLevelUpCost가 float/double이라면 cost도 float/double
        // 여기서는 BigInteger를 가정하고 double로 계산 후 BigInteger로 캐스팅
        double costDouble = (double)PlayerDataManager.Instance.baseLevelUpCost * System.Math.Pow(PlayerDataManager.Instance.levelUpCostIncreaseRatio, currentCharacterData.level - 1);
        System.Numerics.BigInteger cost = (System.Numerics.BigInteger)costDouble; // BigInteger로 캐스팅

        CurrencyType costType = CurrencyType.EnhancementStone; // 비용 재화 타입 (현재는 골드로 고정)

        // DataUtility.FormatNumber가 BigInteger를 받는지 확인 필요. float을 받는다면 cost를 float으로 변경
        levelUpCostText.text = $"Cost: {DataUtility.FormatNumber(cost)} {costType}";

        // 재화가 충분한지 확인하여 버튼 활성화/비활성화
        // InventoryManager.Instance.GetCurrency가 BigInteger를 반환한다고 가정
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.GetCurrency(costType) >= cost)
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
        // PlayerDataManager.Instance.TryGetUpgradeCost가 int cost를 반환한다고 가정
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.TryGetUpgradeCost(currentCharacterData.stars, out cost))
        {
            starUpgradeCostText.text = $"Cost: {cost} Soul Fragments";

            // 해당 캐릭터의 영혼 조각이 충분한지 확인하여 버튼 활성화/비활성화
            int currentFragments = 0;
            // PlayerDataManager.Instance.characterSoulFragments가 int를 저장한다고 가정
            if (PlayerDataManager.Instance.characterSoulFragments.TryGetValue(currentCharacterData.characterdata.characterID, out currentFragments))
            {
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
                starUpgradeButton.interactable = false; // 영혼 조각 데이터가 없으면 비활성화
            }
        }
        else
        {
            starUpgradeCostText.text = "-"; // 정의되지 않은 성급
            starUpgradeButton.interactable = false;
        }
    }

    /// <summary>
    /// 모든 캐릭터 스탯 표시를 갱신합니다.
    /// </summary>
    private void UpdateCharacterStatsDisplay()
    {
        if (currentCharacterData == null) return;      

        // ATK 스탯 데이터 찾기 (CSV 헤더에 맞춰 "attackPower"로 변경)
        StatData atkStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "attackPower");
        if (atkStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float atkValue = StatManager.CalculateStatValue(atkStatData, currentCharacterData.level);
            // DataUtility.FormatNumber가 float을 받지 않을 경우를 대비하여 ToString("F0") 사용
            atkDisplay.text = $"ATK Lv{currentCharacterData.level} {atkValue.ToString("F0")}";
        }
        else
        {
            atkDisplay.text = "ATK Stat Not Found"; // ATK 스탯을 찾을 수 없습니다.
        }

        // HP 스탯 데이터 찾기 (CSV 헤더에 맞춰 "health"로 변경)
        StatData hpStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "health");
        if (hpStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float hpValue = StatManager.CalculateStatValue(hpStatData, currentCharacterData.level);
            // DataUtility.FormatNumber가 float을 받지 않을 경우를 대비하여 ToString("F0") 사용
            hpDisplay.text = $"HP Lv{currentCharacterData.level} {hpValue.ToString("F0")}";
        }
        else
        {
            hpDisplay.text = "HP Stat Not Found"; // HP 스탯을 찾을 수 없습니다.
        }

        // DEF 스탯 데이터 찾기 (CSV 헤더에 맞춰 "defensePower"로 변경)
        StatData defStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "defensePower");
        if (defStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float defValue = StatManager.CalculateStatValue(defStatData, currentCharacterData.level);
            // DataUtility.FormatNumber가 float을 받지 않을 경우를 대비하여 ToString("F0") 사용
            defDisplay.text = $"DEF Lv{currentCharacterData.level} {defValue.ToString("F0")}";
        }
        else
        {
            defDisplay.text = "DEF Stat Not Found"; // DEF 스탯을 찾을 수 없습니다.
        }

        // CritChance 데이터 가져오기 레벨업
        StatData criChanStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "critChance");
        if (criChanStatData != null)
        {
            float criChanValue = criChanStatData.value;
            criChanDisplay.text = $"CRI {criChanValue}%";
        }

        // CritDmg 데이터 가져오기 레벨업
        StatData criDmgStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "critDamage");
        if (criDmgStatData != null)
        {
            float criDmgValue = criDmgStatData.value;
            criDmgDisplay.text = $"CRIDmg {criDmgValue}%";
        }

        // ATKSpd 데이터 가져오기 레벨업
        StatData atkSpdStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == "attackSpeed");
        if (atkSpdStatData != null)
        {
            float atkSpdValue = atkSpdStatData.value;
            atkSpdDisplay.text = $"AtkSpd {atkSpdValue}";
        }
        // 치확 치피 공속 얘네는 나중에 다른 업글 수단 생기면 업데이트해야함 statmanager에서 따로 매서드 추가해야할듯
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

}