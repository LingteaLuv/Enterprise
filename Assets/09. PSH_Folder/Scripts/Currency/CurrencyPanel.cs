
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 보유한 모든 재화를 표시하는 UI 패널을 관리합니다.
/// </summary>
public class CurrencyPanel : MonoBehaviour
{
    [Header("UI 설정")]
    // [SerializeField] private GameObject currencyDisplayPrefab; // 재화 하나를 표시할 프리팹
    // [SerializeField] private Transform contentParent;      // 프리팹들이 생성될 부모 객체 (Vertical Layout Group이 있는 곳)
    [SerializeField] private TextMeshProUGUI t1;
    [SerializeField] private TextMeshProUGUI t2;
    [SerializeField] private TextMeshProUGUI t3;
    [SerializeField] private TextMeshProUGUI t4;
    [SerializeField] private TextMeshProUGUI t5;
    [SerializeField] private Button blocker;

    // private List<CurrencyDisplay> currencyDisplays = new List<CurrencyDisplay>();

    private void Start()
    {
        UpdatePanel();
    }
    private void OnEnable()
    {
        // 재화 정보가 변경될 때마다 패널을 업데이트하도록 이벤트에 등록합니다.
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdatePanel;
        }

        if (blocker != null)
            blocker.onClick.AddListener(()=>gameObject.SetActive(false));
    }

    private void OnDestroy()
    {
        // 메모리 누수를 방지하기 위해 이벤트 구독을 해제합니다.
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdatePanel;
        }

        if (blocker != null)
            blocker.onClick.RemoveListener(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// 패널을 초기화하고 모든 재화 UI를 생성합니다.
    /// </summary>
    private void InitializePanel()
    {/*
        if (currencyDisplayPrefab == null || contentParent == null)
        {
            Debug.LogError("[CurrencyPanel] 프리팹 또는 부모 Transform이 설정되지 않았습니다!");
            return;
        }

        // 기존에 생성된 UI가 있다면 모두 삭제합니다.
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        currencyDisplays.Clear();

        // CurrencyType enum의 모든 멤버를 순회합니다.
        foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
        {
            // 프리팹을 생성합니다.
            GameObject displayObject = Instantiate(currencyDisplayPrefab, contentParent);
            CurrencyDisplay display = displayObject.GetComponent<CurrencyDisplay>();

            if (display != null)
            {
                // 재화 정보를 가져와서 UI를 업데이트합니다.
                var amount = CurrencyManager.Instance.GetCurrency(currencyType);
                display.UpdateDisplay(currencyType, amount);
                currencyDisplays.Add(display);
            }
            else
            {
                Debug.LogWarning($"[CurrencyPanel] {currencyDisplayPrefab.name} 프리팹에 CurrencyDisplay 스크립트가 없습니다.");
            }
        }*/
    }

    /// <summary>
    /// 패널의 모든 재화 정보를 최신 상태로 업데이트합니다.
    /// </summary>
    public void UpdatePanel()
    {
        t1.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.EnhancementStone));
        t2.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.RelicsPoint));
        t3.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.CrewDrawTicket));
        t4.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.EquipDrawTicket)); 
        t5.text = DataUtility.FormatNumber(CurrencyManager.Instance.GetCurrency(CurrencyType.RelicsCoupon));

        /*Debug.Log("재화 패널 업데이트를 시작합니다.");
        foreach (var display in currencyDisplays)
        {
            var amount = CurrencyManager.Instance.GetCurrency(display.CurrencyType);
            display.UpdateDisplay(display.CurrencyType, amount);
        }*/
    }
}
