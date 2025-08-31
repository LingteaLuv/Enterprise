using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 1. MonoBehaviour 대신 UIBase를 상속받고, 드래그 인터페이스들을 구현합니다.
public class CharacterInfoUI : UIBase, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("UI 요소")]
    public TextMeshProUGUI characterNameText;
    public Image characterImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI soulFragmentsText; // 캐릭터별 영혼 조각

    [Header("닫기 버튼")]
    public Button closeButton;
    public Button blocker;

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

    public TextMeshProUGUI battlePoint;

    [Header("좌우 버튼")]
    public Button prevButton;
    public Button nextButton;

    private List<PlayerCharacterData> characterList;
    private int currentIndex;

    [Header("스와이프 설정")]
    public float swipeThreshold = 50f; // 스와이프로 인식할 최소 픽셀 거리
    private Vector2 dragStartPosition;

    private PlayerCharacterData currentCharacterData; // 현재 표시 중인 캐릭터 데이터

    void Awake()
    {
        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(SetHide);
        }

        if (blocker != null)
        {
            blocker.onClick.AddListener(SetHide);
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

        // 좌우 버튼 이벤트 연결
        if (prevButton != null)
        {
            prevButton.onClick.AddListener(PreviousCharacter);
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextCharacter);
        }
    }

    /// <summary>
    /// 캐릭터 정보 리스트와 현재 인덱스를 기반으로 패널을 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData character, List<PlayerCharacterData> list)
    {
        characterList = list;
        // 전달된 캐릭터가 리스트 내에서 몇 번째인지 찾습니다.
        currentIndex = characterList.FindIndex(c => c == character);

        if (currentIndex == -1)
        {
            Debug.LogError("선택된 캐릭터가 리스트에 없습니다!");
            SetHide(); // ClosePanel() 대신 SetHide() 사용
            return;
        }

        DisplayCharacter();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 인덱스의 캐릭터 정보를 화면에 표시합니다.
    /// </summary>
    private void DisplayCharacter()
    {
        // 현재 인덱스로 캐릭터 데이터 가져오기
        currentCharacterData = characterList[currentIndex];

        if (currentCharacterData != null)
        {
            characterNameText.text = $"이름 : {currentCharacterData.characterdata.characterName}";
            characterImage.sprite = currentCharacterData.characterdata.characterSprite;
            levelText.text = $"캐릭터 레벨 : {currentCharacterData.characterLevel}";

            UpdateSoulFragmentsUI();
            UpdateLevelUpUI();
            UpdateStarUpgradeUI();
            UpdateCharacterStatsDisplay();
        }
        else
        {
            Debug.LogWarning("CharacterInfoUI: 현재 인덱스의 캐릭터 데이터가 null입니다.");
            SetHide(); // ClosePanel() 대신 SetHide() 사용
        }

        // 좌우 버튼 활성화/비활성화 로직
        prevButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < characterList.Count - 1;
    }

    /// <summary>
    /// 이전 캐릭터를 표시합니다.
    /// </summary>
    public void PreviousCharacter()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            DisplayCharacter();
        }
    }

    /// <summary>
    /// 다음 캐릭터를 표시합니다.
    /// </summary>
    public void NextCharacter()
    {
        if (currentIndex < characterList.Count - 1)
        {
            currentIndex++;
            DisplayCharacter();
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
            soulFragmentsText.text = $"영혼 조각 : {currentFragments}";
        }
        else
        {
            soulFragmentsText.text = "영혼 조각 : 0";
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
        double costDouble = (double)PlayerDataManager.Instance.baseLevelUpCost * System.Math.Pow(PlayerDataManager.Instance.levelUpCostIncreaseRatio, currentCharacterData.characterLevel - 1);
        System.Numerics.BigInteger cost = (System.Numerics.BigInteger)costDouble;

        CurrencyType costType = CurrencyType.EnhancementStone;

        levelUpCostText.text = $"비용 : {DataUtility.FormatNumber(cost)} 강화석";

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
            starUpgradeCostText.text = $"비용 : {cost} 영혼 조각";

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
        StatData atkStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.Attack);
        if (atkStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float atkValue = StatManager.CalculateStatValue(atkStatData, currentCharacterData.characterLevel);
            atkDisplay.text = $"공격력 : {DataUtility.FormatNumber(atkValue)}";
            currentCharacterData.characterStats[Stat.Attack] = atkValue;
        }
        else
        {
            atkDisplay.text = "ATK Stat Not Found";
        }

        // HP 스탯 데이터 찾기 (CSV 헤더에 맞춰 "health"로 변경)
        StatData hpStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.Health);
        if (hpStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float hpValue = StatManager.CalculateStatValue(hpStatData, currentCharacterData.characterLevel);
            hpDisplay.text = $"체력 : {DataUtility.FormatNumber(hpValue)}";
            currentCharacterData.characterStats[Stat.Health] = hpValue;
        }
        else
        {
            hpDisplay.text = "HP Stat Not Found";
        }

        // DEF 스탯 데이터 찾기 (CSV 헤더에 맞춰 "defensePower"로 변경)
        StatData defStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.Defense);
        if (defStatData != null)
        {
            // StatManager.CalculateStatValue가 float을 반환하므로 타입 변경
            float defValue = StatManager.CalculateStatValue(defStatData, currentCharacterData.characterLevel);
            defDisplay.text = $"방어력 : {DataUtility.FormatNumber(defValue)}";
            currentCharacterData.characterStats[Stat.Defense] = defValue;
        }
        else
        {
            defDisplay.text = "DEF Stat Not Found";
        }

        // CritChance 데이터 가져오기 레벨업
        StatData criChanStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.CritChance);
        if (criChanStatData != null)
        {
            float criChanValue = criChanStatData.value;
            criChanDisplay.text = $"치명타 확률 : {criChanValue}%";
        }

        // CritDmg 데이터 가져오기 레벨업
        StatData criDmgStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.CritDamage);
        if (criDmgStatData != null)
        {
            float criDmgValue = criDmgStatData.value;
            criDmgDisplay.text = $"치명타 피해 : {criDmgValue}%";
        }

        // ATKSpd 데이터 가져오기 레벨업
        StatData atkSpdStatData = currentCharacterData.characterdata.baseStats.Find(s => s.statName == Stat.AttackSpeed);
        if (atkSpdStatData != null)
        {
            float atkSpdValue = atkSpdStatData.value;
            atkSpdDisplay.text = $"공격 속도 : {atkSpdValue}";
        }
        // 치확 치피 공속 얘네는 나중에 다른 업글 수단 생기면 업데이트해야함 statmanager에서 따로 매서드 추가해야할듯

        // 전투력 계산
        currentCharacterData.RecaculateStats();
        battlePoint.text = $"전투력 : {DataUtility.FormatNumber(currentCharacterData.battlePower)}";
    }

    /// <summary>
    /// 캐릭터 레벨업 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnLevelUpButtonClicked()
    {
        if (currentCharacterData == null) return;

        // PlayerDataManager의 TryLevelUpCharacter는 성공 시 이벤트를 발생시키므로,
        // 이 UI는 그 이벤트를 수신하여 스스로 갱신됩니다.
        // 따라서 이 함수에서는 단순히 레벨업 시도만 하면 됩니다.
        PlayerDataManager.Instance.TryLevelUpCharacter(currentCharacterData);
    }

    /// <summary>
    /// 성급 업그레이드 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnStarUpgradeButtonClicked()
    {
        if (currentCharacterData == null) return;

        // 레벨업과 마찬가지로, 업그레이드 시도만 하고 UI 갱신은
        // PlayerDataManager가 발생시키는 이벤트에 맡깁니다.
        PlayerDataManager.Instance.TryUpgradeCharacterStar(currentCharacterData);
    }

    private void OnEnable()
    {
        // PlayerDataManager의 이벤트에 구독
        PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;
    }

    private void OnDisable()
    {
        // PlayerDataManager의 이벤트 구독 해제
        PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
    }

    /// <summary>
    /// PlayerDataManager로부터 캐릭터 데이터 변경 이벤트를 받았을 때 호출됩니다.
    /// </summary>
    private void HandleCharacterUpdate(PlayerCharacterData updatedData)
    {
        // 현재 이 정보창에 표시된 캐릭터의 데이터가 변경된 경우에만 UI를 새로고침합니다.
        if (currentCharacterData != null && currentCharacterData == updatedData)
        {
            DisplayCharacter();
        }
    }

    // 3. ClosePanel()을 UIBase의 SetHide()를 오버라이드 하도록 변경
    public override void SetHide()
    {
        base.SetHide(); // gameObject.SetActive(false)를 호출
    }

    #region 스와이프 처리
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작!");
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 끝!");
        Vector2 dragEndPosition = eventData.position;
        float dragDistance = dragEndPosition.x - dragStartPosition.x;

        // 오른쪽으로 스와이프 (이전 캐릭터)
        if (dragDistance > swipeThreshold)
        {
            Debug.Log("오른쪽으로 스와이프 감지");
            PreviousCharacter();
        }
        // 왼쪽으로 스와이프 (다음 캐릭터)
        else if (dragDistance < -swipeThreshold)
        {
            Debug.Log("왼쪽으로 스와이프 감지");
            NextCharacter();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
    #endregion
}
