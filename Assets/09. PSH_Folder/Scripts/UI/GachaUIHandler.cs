using JHT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 각종 뽑기 버튼의 OnClick 이벤트를 받아 적절한 Gacha Manager에게 작업을 요청하는 클래스입니다.
/// </summary>
public class GachaUIHandler : MonoBehaviour
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

    [Header("장비 뽑기")]
    public EquipmentGachaManager equipmentGachaManager;
    public Button equipSingleBtn;
    public Button equipMultipleBtn;

    [Header("유물 뽑기")]
    // public RelicGachaManager relicGachaManager;

    [Header("결과 UI")]
    [Tooltip("뽑기 결과 화면 UI를 연결하세요.")]
    public GachaListUI gachaListPanel;


    private void Start()
    {
        // 탭 버튼 이벤트 연결
        charButton.onClick.AddListener(() => { charPanel.SetActive(true); equipPanel.SetActive(false); relicPanel.SetActive(false); });
        equipButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(true); relicPanel.SetActive(false); });
        relicButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(false); relicPanel.SetActive(true); });

        // 캐릭터 뽑기 버튼 이벤트 연결
        charSingleBtn.onClick.AddListener(OnClick_CharacterGacha_Single);
        charMultipleBtn.onClick.AddListener(OnClick_CharacterGacha_Multiple);

        // 장비 뽑기 버튼 이벤트 연결
        equipSingleBtn.onClick.AddListener(OnClick_EquipmentGacha_Single);
        equipMultipleBtn.onClick.AddListener(OnClick_EquipmentGacha_Multiple);
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
        if (typeof(T) == typeof(PlayerCharacterData)) itemType = "캐릭터";
        else if (typeof(T) == typeof(ItemObject)) itemType = "장비";

        UIManager.Instance.ShowConfirm(
            $"{totalCost}{manager.currencyType}을(를) 소비하여 {itemType} {count}회 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                if (manager.PerformMultipleGacha(count))
                {
                    Debug.Log($"UI 버튼 클릭으로 {itemType} {count}회 뽑기를 실행했습니다.");

                    // [디버그 로그 추가]
                    if (manager.LastGachaResults == null)
                    {
                        Debug.LogError("[GachaUIHandler] LastGachaResults 리스트가 null입니다!");
                        return;
                    }
                    Debug.Log($"[GachaUIHandler] 결과 리스트 확인. 아이템 개수: {manager.LastGachaResults.Count}, 리스트 타입: {manager.LastGachaResults.GetType().Name}");

                    // THE CRITICAL PART
                    if (manager.LastGachaResults is List<PlayerCharacterData> characterResults)
                    {
                        Debug.Log("[GachaUIHandler] 캐릭터 결과 UI를 호출합니다.");
                        gachaListPanel.DisplayCharacterResults(characterResults);
                        gachaListPanel.gameObject.SetActive(true);
                    }
                    else if (manager.LastGachaResults is List<ItemObject> equipmentResults)
                    {
                        Debug.Log("[GachaUIHandler] 장비 결과 UI를 호출합니다.");
                        gachaListPanel.DisplayEquipmentResults(equipmentResults);
                        gachaListPanel.gameObject.SetActive(true);
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
}