using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProbabilityItemUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI probabilityText;
    // public Image itemIcon; // 아이콘이 있다면 연결

    /// <summary>
    /// 일반 아이템 정보 설정
    /// </summary>
    public void Setup(string itemName, string rarity, float probability)
    {
        if (itemNameText != null) itemNameText.text = itemName;
        if (rarityText != null) rarityText.text = rarity;
        if (probabilityText != null) probabilityText.text = $"{probability:P4}"; // 소수점 4자리까지 %로 표시
    }

    /// <summary>
    /// 등급 구분을 위한 헤더로 설정
    /// </summary>
    public void SetupAsHeader(string headerText)
    {
        if (itemNameText != null) itemNameText.text = headerText;
        if (rarityText != null) rarityText.text = "";
        if (probabilityText != null) probabilityText.text = "";

        // 헤더는 다른 폰트나 색상을 사용할 수 있음
        // itemNameText.fontStyle = FontStyles.Bold;
        // itemNameText.color = Color.yellow;
    }
}
