using JHT;
using System.Collections.Generic;
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

    [Header("결과 UI")]
    [Tooltip("뽑기 결과 화면 UI를 연결하세요.")]
    public GachaListUI gachaListPanel;
    public RelicsGachaListUI relicsGachaListPanel;
    [SerializeField] private TextMeshProUGUI relicsPoints;

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
        InventoryManager.Instance.OnChangeRelicsPoints += ShowRelicsPoint;
        ShowRelicsPoint(InventoryManager.Instance.myRelicsPoints);
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
        if (InventoryManager.Instance.relicsCoupon >= relicsGachaManager.relicsCouponCost)
        {
            ShowSelectPopUp(relicsGachaManager, true);
        }
        else
        {
            UIManager.Instance.ShowToast("유물 쿠폰이 부족합니다!", 2f);
        }
    }

    public void OnClick_RelicsGacha_Special()
    {
        if (InventoryManager.Instance.myRelicsPoints >= relicsGachaManager.relicsSpecialCost)
        {
            ShowSelectPopUp(relicsGachaManager, false);
        }
        else
        {
            UIManager.Instance.ShowToast($"{relicsGachaManager.relicsSpecialCost - InventoryManager.Instance.myRelicsPoints}만큼 유물잔해가 부족합니다!", 2f);
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
        bool isCharacterGacha = false;
        if (typeof(T) == typeof(PlayerCharacterData))
        {
            itemType = "캐릭터";
            isCharacterGacha = true;
        }
        else if (typeof(T) == typeof(ItemObject)) itemType = "장비";

        UIManager.Instance.ShowConfirm(
            $"{totalCost}{manager.currencyType}을(를) 소비하여 {itemType} {count}회 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                if (manager.PerformMultipleGacha(count))
                {
                    Debug.Log($"UI 버튼 클릭으로 {itemType} {count}회 뽑기를 실행했습니다.");

                    if (manager.LastGachaResults == null)
                    {
                        Debug.LogError("[GachaUIHandler] LastGachaResults 리스트가 null입니다!");
                        return;
                    }

                    // 결과창 표시
                    if (manager.LastGachaResults is List<PlayerCharacterData> characterResults)
                    {
                        gachaListPanel.gameObject.SetActive(true);
                        gachaListPanel.DisplayCharacterResults(characterResults);
                        QuestSignalManager.Instance.GachaPull(ItemType.Character, count);
                    }
                    else if (manager.LastGachaResults is List<ItemObject> equipmentResults)
                    {
                        gachaListPanel.gameObject.SetActive(true);
                        gachaListPanel.DisplayEquipmentResults(equipmentResults);
                        QuestSignalManager.Instance.GachaPull(ItemType.Equipment, count);
                    }

                    // *** 올바른 위치 ***
                    // 캐릭터 뽑기였다면, 뽑기가 모두 끝난 이 시점에서 텍스트를 갱신합니다.
                    if (isCharacterGacha)
                    {
                        UpdateCharacterPityText();
                    }
                }
                else
                {
                    UIManager.Instance.ShowWarning("재화가 부족합니다.");
                }
            },
            onCancel: () =>
            {
                Debug.Log($"{itemType} {count}회 뽑기 취소됨");
            }
        );
    }

    private void ShowSelectPopUp(RelicsGachaManager manager, bool isNormal)
    {

        if (isNormal)
        {
            UIManager.Instance.ShowConfirm("유물 뽑기를 진행 하시겠습니까?",
            () =>
            {
                manager.GetGachaOneRelicsData(true);
                relicsGachaListPanel.gameObject.SetActive(true);
                relicsGachaListPanel.Init(manager);

                InventoryManager.Instance.relicsCoupon -= manager.relicsCouponCost;
            });
        }
        else
        {
            UIManager.Instance.ShowConfirm("스페셜 유물 뽑기를 진행 하시겠습니까?",
            () =>
            {
                manager.GetGachaOneRelicsData(false);
                relicsGachaListPanel.gameObject.SetActive(true);
                relicsGachaListPanel.Init(manager);

                InventoryManager.Instance.myRelicsPoints -= manager.relicsSpecialCost;
            });
        }

    }

    private void ShowRelicsPoint(float value)
    {
        relicsPoints.text = value.ToString();
    }

    //private void OnDestroy()
    //{
    //    InventoryManager.Instance.OnChangeRelicsPoints -= ShowRelicsPoint;
    //}
}