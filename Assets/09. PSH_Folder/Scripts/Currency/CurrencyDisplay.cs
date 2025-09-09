
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;

/// <summary>
/// 단일 재화의 아이콘과 수량을 표시하는 UI 요소입니다.
/// </summary>
public class CurrencyDisplay : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image currencyIcon; 
    [SerializeField] private TextMeshProUGUI amountText;

    public CurrencyType CurrencyType { get; private set; }

    /// <summary>
    /// 재화 정보를 받아 UI를 업데이트합니다.
    /// </summary>
    /// <param name="type">표시할 재화 종류</param>
    /// <param name="amount">표시할 재화 수량</param>
    public void UpdateDisplay(CurrencyType type, BigInteger amount)
    {
        CurrencyType = type;

        // CurrencyIconManager를 통해 아이콘을 가져와 설정합니다.
        // 이 매니저는 직접 구현해야 합니다.
        Sprite icon = CurrencyIconManager.Instance.GetIcon(type);
        if (currencyIcon != null && icon != null)
        { 
            currencyIcon.sprite = icon;
        }
        else
        {
            Debug.LogWarning($"[CurrencyDisplay] {type}에 대한 아이콘을 찾을 수 없거나 Image 컴포넌트가 없습니다.");
        }

        // 재화 수량을 텍스트로 변환하여 표시합니다.
        if (amountText != null)
        {
            amountText.text = amount.ToString();
            //amountText.text = DataUtility.FormatNumber(amount);
        }
    }
}
