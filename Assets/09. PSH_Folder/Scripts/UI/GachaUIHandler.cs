using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 각종 뽑기 버튼의 OnClick 이벤트를 받아 적절한 Gacha Manager에게 작업을 요청하는 클래스입니다.
/// </summary>
public class GachaUIHandler : MonoBehaviour
{
    [Header("패널 목록")]
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
    // public EquipmentGachaManager equipmentGachaManager;

    [Header("유물 뽑기")]


    [Header("연결할 UI")]
    [Tooltip("뽑기 결과 화면 UI를 연결하세요.")]
    public GachaListUI gachaListPanel;


    private void Start()
    {
        // 패널 연결
        charButton.onClick.AddListener(() => { charPanel.SetActive(true); equipPanel.SetActive(false); relicPanel.SetActive(false); });
        equipButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(true); relicPanel.SetActive(false); });
        relicButton.onClick.AddListener(() => { charPanel.SetActive(false); equipPanel.SetActive(false); relicPanel.SetActive(true); });


        // 캐릭터 뽑기
        charSingleBtn.onClick.AddListener(OnClick_CharacterGacha_Single);
        charMultipleBtn.onClick.AddListener(OnClick_CharacterGacha_Multiple);
    }

    #region 캐릭터 뽑기 함수
    /// <summary>
    /// 캐릭터 1회 뽑기 버튼에 연결할 함수
    /// </summary>
    public void OnClick_CharacterGacha_Single()
    {
        HandleGacha(characterGachaManager, 1);
    }

    /// <summary>
    /// 캐릭터 10회 뽑기 버튼에 연결할 함수
    /// </summary>
    public void OnClick_CharacterGacha_Multiple()
    {
        HandleGacha(characterGachaManager, 10);
    }
    #endregion


    #region 장비 뽑기 함수 (구현 예시)
    /*
    /// <summary>
    /// 장비 1회 뽑기 버튼에 연결할 함수
    /// </summary>
    public void OnClick_EquipmentGacha_Single()
    {
        HandleGacha(equipmentGachaManager, 1);
    }

    /// <summary>
    /// 장비 10회 뽑기 버튼에 연결할 함수
    /// </summary>
    public void OnClick_EquipmentGacha_Multiple()
    {
        HandleGacha(equipmentGachaManager, 10);
    }
    */
    #endregion


    /// <summary>
    /// 실제 뽑기를 처리하는 범용 함수
    /// </summary>
    /// <typeparam name="T">PlayerCharacterData 또는 EquipmentData 등</typeparam>
    /// <param name="manager">사용할 매니저</param>
    /// <param name="count">뽑을 횟수</param>
    private void HandleGacha<T>(BaseGachaManager<T> manager, int count) where T : class
    {
        if (manager == null)
        {
            Debug.LogError($"{typeof(T).Name} Gacha Manager가 GachaUIHandler에 연결되지 않았습니다!");
            return;
        }

        int totalCost = manager.singleGachaCost * count;
        string itemType = "아이템"; // 기본 표시 이름
        if (typeof(T) == typeof(PlayerCharacterData)) itemType = "캐릭터";
        // else if (typeof(T) == typeof(EquipmentData)) itemType = "장비"; // 나중에 추가

        // 확인창 호출
        UIManager.Instance.ShowConfirm(
            $"{totalCost}{manager.currencyType}을(를) 소비하여 {itemType} {count}회 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                if (manager.PerformMultipleGacha(count))
                {
                    Debug.Log($"UI 버튼 클릭으로 {itemType} {count}회 뽑기를 실행했습니다.");

                    // 뽑기 결과 타입에 따라 적절한 UI 표시 함수를 호출합니다.
                    if (manager.LastGachaResults is List<PlayerCharacterData> characterResults)
                    {
                        gachaListPanel.DisplayCharacterResults(characterResults);
                        gachaListPanel.gameObject.SetActive(true);
                    }
                    /*
                    else if (manager.LastGachaResults is List<EquipmentData> equipmentResults)
                    {
                        // 나중에 장비 결과 UI를 만들면 여기에 연결
                        // gachaListUI.DisplayEquipmentResults(equipmentResults);
                        // gachaListUI.gameObject.SetActive(true);
                    }
                    */
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
