using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 각종 뽑기 버튼의 OnClick 이벤트를 받아 적절한 Gacha Manager에게 작업을 요청하는 클래스입니다.
/// </summary>
public class GachaUIHandler : UIBase
{
    [Header("패널 및 탭 버튼")]
    public GameObject charPanel;
    public GameObject equipPanel;
    public GameObject relicPanel;
    public Button charButton;
    public Button equipButton;
    public Button relicButton;

    [Header("캐릭터 뽑기")]
    public CharacterGachaManager characterGachaManager;
    public Button charSingleBtn;
    public Button charMultipleBtn;
    public TextMeshProUGUI gachaPityCountText;

    [Header("장비 뽑기")]
    public EquipmentGachaManager equipmentGachaManager;
    public Button equipSingleBtn;
    public Button equipMultipleBtn;

    [Header("유물 뽑기")]
    public RelicsGachaManager relicsGachaManager;
    public Button relicsSingleBtn;
    public Button specialRelicsSingleBtn;
    public Button relicsProbabilityBtn;
    public RelicsProbability relicsUpgradePanel;

    [Header("가챠 연출")]
    [Tooltip("캐릭터 가챠 시 재생할 연출 프리팹")]
    public GameObject characterGachaDirectorPrefab;

    [Header("결과 UI")]
    [Tooltip("뽑기 결과 화면 UI를 연결하세요.")]
    public GachaListUI gachaListPanel;
    public RelicsGachaListUI relicsGachaListPanel;
    [SerializeField] private RelicsPoints relicsPoints;

    public override void ResetPanel()
    {
        base.ResetPanel();

        // 열려있을 수 있는 결과창들을 모두 닫습니다.
        if (gachaListPanel != null) gachaListPanel.gameObject.SetActive(false);
        if (relicsGachaListPanel != null) relicsGachaListPanel.gameObject.SetActive(false);

        // 기본 탭인 캐릭터 뽑기 탭으로 되돌립니다.
        if (charPanel != null) charPanel.SetActive(true);
        if (equipPanel != null) equipPanel.SetActive(false);
        if (relicPanel != null) relicPanel.SetActive(false);

        Debug.Log("GachaUIHandler가 리셋되어, 모든 결과창을 닫고 기본 탭으로 돌립니다.");
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        // UI가 활성화될 때마다 천장 텍스트를 갱신합니다.
        UpdateCharacterPityText();
    }

    // 캐릭터 뽑기 버튼 이벤트 연결
    private void Start()
    {
        // 탭 버튼 이벤트 연결
        charButton.onClick.AddListener(() => { charPanel.SetActive(true); equipPanel.SetActive(false); relicPanel.SetActive(false); UpdateCharacterPityText(); });
        equipButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(true); relicPanel.SetActive(false); });
        relicButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(false); relicPanel.SetActive(true); });

        // 캐릭터 뽑기 버튼 이벤트 연결
        charSingleBtn.onClick.AddListener(OnClick_CharacterGacha_Single);
        charMultipleBtn.onClick.AddListener(OnClick_CharacterGacha_Multiple);

        // 장비 뽑기 버튼 이벤트 연결
        equipSingleBtn.onClick.AddListener(OnClick_EquipmentGacha_Single);
        equipMultipleBtn.onClick.AddListener(OnClick_EquipmentGacha_Multiple);

        // 유물 뽑기 버튼 이벤트 연결 및 인벤토리 액션연결
        relicsSingleBtn.onClick.AddListener(OnClick_RelicsGacha_Single);
        specialRelicsSingleBtn.onClick.AddListener(OnClick_RelicsGacha_Special);
        relicsPoints.Init(relicsGachaManager);
        relicsProbabilityBtn.onClick.AddListener(ShowUpgradeGachaPanel);

        // 천장 카운트 UI 갱신 이벤트 구독
        CharacterGachaManager.OnGachaPityChanged += UpdateCharacterPityText;
    }

    private void OnDestroy()
    {
        // 천장 카운트 UI 갱신 이벤트 구독 해제
        CharacterGachaManager.OnGachaPityChanged -= UpdateCharacterPityText;
    }


    #region 캐릭터 뽑기 함수
    public void OnClick_CharacterGacha_Single()
    {
        HandleGacha(characterGachaManager, 1);
    }

    public void OnClick_CharacterGacha_Multiple()
    {
        HandleGacha(characterGachaManager, 10);
    }

    public void UpdateCharacterPityText()
    {
        if (gachaPityCountText != null && characterGachaManager != null)
        {
            int remaining = CharacterGachaManager.GACHA_CEILING_COUNT - characterGachaManager.gachaPityCounter;
            gachaPityCountText.text = "3성 확정까지\n<color=yellow>" + remaining + "회</color> 남음";
        }
    }
    #endregion


    #region 장비 뽑기 함수
    public void OnClick_EquipmentGacha_Single()
    {
        HandleGacha(equipmentGachaManager, 1);
    }

    public void OnClick_EquipmentGacha_Multiple()
    {
        HandleGacha(equipmentGachaManager, 10);
    }
    #endregion

    #region 유물 뽑기 함수
    public void OnClick_RelicsGacha_Single()
    {
        if (InventoryManager.Instance.RelicsCoupon >= relicsGachaManager.relicsCouponCost)
        {
            ShowSelectPopUp(relicsGachaManager, relicsGachaManager.relicsTablelevel);
        }
        else
        {
            UIManager.Instance.ShowToast("유물 쿠폰이 부족합니다!", 2f);
        }
    }

    public void OnClick_RelicsGacha_Special()
    {
        if (InventoryManager.Instance.RelicsPoints >= relicsGachaManager.relicsSpecialCost)
        {
            ShowSelectPopUp(relicsGachaManager, -1);
        }
        else
        {
            UIManager.Instance.ShowToast($"{relicsGachaManager.relicsSpecialCost - InventoryManager.Instance.RelicsPoints}만큼 유물잔해가 부족합니다!", 2f);
        }
    }
    #endregion

    /// <summary>
    /// 실제 뽑기를 처리하는 범용 함수
    /// </summary>
    private void HandleGacha<T>(BaseGachaManager<T> manager, int count) where T : class
    {
        if (manager == null)
        {
            Debug.LogError($"GachaUIHandler에 {typeof(T).Name}에 해당하는 Gacha Manager가 연결되지 않았습니다!");
            return;
        }

        int totalCost = manager.singleGachaCost * count;
        string itemType = "아이템";
        bool isCharacterGacha = typeof(T) == typeof(PlayerCharacterData);
        if (isCharacterGacha) itemType = "캐릭터";
        else if (typeof(T) == typeof(ItemObject)) itemType = "장비";

        string currencyName = GetCurrencyNameInKorean(manager.currencyType);

        UIManager.Instance.ShowConfirm(
            $"{totalCost}{currencyName}을(를) 소비하여 {itemType} {count}회 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                // [수정] 연출 재생과 결과 표시를 동시에 진행합니다.
                if (isCharacterGacha && characterGachaDirectorPrefab != null)
                {
                    CharacterGachaDirector director = EffectPoolManager.Instance.SpawnObject<CharacterGachaDirector>(characterGachaDirectorPrefab);
                    if (director != null)
                    {
                        // 연출이 끝나면 스스로 풀에 반납되도록 콜백만 설정하고 바로 다음 로직으로 넘어갑니다.
                        director.Play(onComplete: () =>
                        {
                            EffectPoolManager.Instance.ReturnToPool(director.gameObject);
                        });
                    }
                }

                // 연출 유무와 관계없이, 실제 뽑기 로직과 결과 표시는 즉시 실행합니다.
                if (manager.PerformMultipleGacha(count))
                {
                    Debug.Log($"UI 버튼 클릭으로 {itemType} {count}회 뽑기를 실행했습니다.");
                    if (isCharacterGacha)
                    {
                        QuestSignalManager.Instance.GachaPull(ItemType.Character, count);
                    }
                    else if (itemType == "장비")
                    {
                        QuestSignalManager.Instance.GachaPull(ItemType.Equipment, count);
                    }
                }
            },
            onCancel: () =>
            {
                Debug.Log($"{itemType} {count}회 뽑기 취소됨");
            }
        );
    }

    private string GetCurrencyNameInKorean(CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.CrewDrawTicket:
                return "캐릭터 뽑기권";
            case CurrencyType.EquipDrawTicket:
                return "장비 뽑기권";
            case CurrencyType.Gem:
                return "보석";
            default:
                return currencyType.ToString();
        }
    }

    private void ShowSelectPopUp(RelicsGachaManager manager, int curTableLevel)
    {

        if (curTableLevel >= 0)
        {
            UIManager.Instance.ShowConfirm("유물 뽑기를 진행 하시겠습니까?",
            onConfirm: () =>
            {
                manager.GetGachaOneRelicsData(curTableLevel);
                relicsGachaListPanel.gameObject.SetActive(true);
                relicsGachaListPanel.Init(manager);

                InventoryManager.Instance.RelicsCoupon -= manager.relicsCouponCost;
            },
            onCancel: () =>
            {
                Debug.Log($"유물 뽑기 취소됨");
            });
        }
        else
        {
            UIManager.Instance.ShowConfirm("스페셜 유물 뽑기를 진행 하시겠습니까?",
            onConfirm: () =>
            {
                manager.GetGachaOneRelicsData(-1);
                relicsGachaListPanel.gameObject.SetActive(true);
                relicsGachaListPanel.Init(manager);

                InventoryManager.Instance.RelicsPoints -= manager.relicsSpecialCost;
            },
            onCancel: () =>
            {
                Debug.Log($"스페셜 유물 뽑기 취소됨");
            });
        }

    }

    private void ShowUpgradeGachaPanel()
    {
        relicsUpgradePanel.gameObject.SetActive(true);
        relicsUpgradePanel.Init(relicsGachaManager);
    }

    //private void OnDestroy()
    //{
    //    InventoryManager.Instance.OnChangeRelicsPoints -= ShowRelicsPoint;
    //} 
}