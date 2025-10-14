using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicStatUI : UIBase
{
    [Header("스탯 UI 그룹 (공격력, 방어력, 체력 순서)")]
    public BasicStatUIGroup[] statUIGroups; // 각 스탯별 UI 요소를 담을 배열

    [Header("레벨업 배수 버튼")]
    public Button enhanceOnceButton; // 1회 강화 버튼
    public Button enhanceTenTimesButton; // 10회 강화 버튼

    [Header("닫기 버튼")]
    public Button closeButton;
    public Button blocker;

    [Header("보유 골드 및 단계 표시")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI stageText; // 현재 단계를 표시할 텍스트

    private int _levelsToGain = 1; // 한 번에 올릴 레벨 수 (1 또는 10)

    [System.Serializable]
    public class BasicStatUIGroup
    {
        public BasicStatType statType; // 이 그룹이 담당하는 스탯 종류
        public TextMeshProUGUI levelText; // 현재 레벨 (예: Lv. 1)
        public TextMeshProUGUI valueText; // 현재 스탯 값 (예: 100)
        public TextMeshProUGUI costText; // 레벨업 비용 (예: 1000 Gold)
        public HoldButton levelUpButton; // 레벨업 버튼
    }

    private void Awake()
    {
        foreach (var group in statUIGroups)
        {
            if (group.levelUpButton != null)
            {
                group.levelUpButton.onHoldAction.AddListener(() => OnLevelUpButtonClicked(group.statType));
            }
        }

        if (enhanceOnceButton != null)
        {
            enhanceOnceButton.onClick.AddListener(() => SelectEnhancementAmount(1));
        }
        if (enhanceTenTimesButton != null)
        {
            enhanceTenTimesButton.onClick.AddListener(() => SelectEnhancementAmount(10));
        }
        SelectEnhancementAmount(1); // 기본값으로 1회 강화 선택
    }

    private void OnEnable()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        blocker.onClick.AddListener(() => gameObject.SetActive(false));
        // BasicStatManager의 이벤트에 구독하여 스탯 변경 시 UI를 자동으로 갱신
        BasicStatManager.OnBaseStatsChanged += RefreshUI;
        RefreshUI(); // 활성화될 때 항상 최신 정보로 갱신
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveAllListeners();
        blocker.onClick.RemoveAllListeners();
        // 비활성화될 때 이벤트 구독 해제
        BasicStatManager.OnBaseStatsChanged -= RefreshUI;
    }

    public override void RefreshUI()
    {
        if (BasicStatManager.Instance == null || CurrencyManager.Instance == null)
        {
            return; // 매니저가 준비되지 않았으면 UI 갱신 중단
        }

        // 단계 텍스트 갱신
        if (stageText != null)
        {
            stageText.text = $"Lv {BasicStatManager.Instance.GetCurrentStage()}";
        }

        foreach (var group in statUIGroups)
        {
            int currentLevel = BasicStatManager.Instance.GetStatLevel(group.statType);
            if (group.levelText != null)
            {
                group.levelText.text = $"{currentLevel}";
            }

            float currentValue = BasicStatManager.Instance.GetStatValue(group.statType);
            if (group.valueText != null)
            {
                group.valueText.text = DataUtility.FormatNumber(currentValue);
            }

            BigInteger cost = BasicStatManager.Instance.GetLevelUpCost(group.statType, _levelsToGain);

            if (group.costText != null)
            {
                // GetLevelUpCost가 -1을 반환하면 최대 레벨에 도달한 것으로 간주
                if (cost < 0)
                {
                    group.costText.text = "MAX";
                }
                else
                {
                    group.costText.text = $"{DataUtility.FormatNumber(cost)}";
                }
            }

            if (group.levelUpButton != null)
            {
                bool canAfford = (cost >= 0) && CurrencyManager.Instance.CanSpendCurrency(CurrencyType.Gold, cost);
                group.levelUpButton.interactable = canAfford;
            }
        }

        if (goldText != null)
        {
            goldText.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.Gold));
            // goldText.text = CurrencyManager.Instance.GetCurrency(CurrencyType.Gold).ToString();
        }
    }

    private void OnLevelUpButtonClicked(BasicStatType type)
    {
        if (BasicStatManager.Instance == null) return;

        bool success = BasicStatManager.Instance.TryLevelUpStat(type, _levelsToGain);
        if (success)
        {
            // OnBaseStatsChanged 이벤트가 RefreshUI를 호출하므로 여기서 직접 호출할 필요 없음
            // RefreshUI(); 

            UpgradeType upType;
            switch (type)
            {
                case BasicStatType.Attack: upType = UpgradeType.Atk; break;
                case BasicStatType.Defense: upType = UpgradeType.Def; break;
                case BasicStatType.Health: upType = UpgradeType.Hp; break;
                default: upType = UpgradeType.Atk; break;
            }
            QuestSignalManager.Instance.Upgrade(upType, _levelsToGain);
        }
    }

    private void SelectEnhancementAmount(int amount)
    {
        _levelsToGain = amount;

        // 선택된 버튼은 비활성화해서 어둡게, 나머지는 활성화
        if (enhanceOnceButton != null)
        {
            enhanceOnceButton.interactable = (amount != 1);
        }
        if (enhanceTenTimesButton != null)
        {
            enhanceTenTimesButton.interactable = (amount != 10);
        }

        RefreshUI();
    }
}