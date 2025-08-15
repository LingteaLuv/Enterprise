using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using TMPro;

public class StatDisplayPanel : MonoBehaviour
{
    [Header("--- 이 패널의 UI 요소 ---")]
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statValueText;
    public TextMeshProUGUI upgradeCostText;
    public HoldButton holdButton;

    // 로직을 처리할 매니저와 자신의 인덱스
    private StatManager manager;
    private int statIndex;

    // StatManager가 호출해주는 초기화 함수
    public void Initialize(StatManager statManager, int index)
    {
        manager = statManager;
        statIndex = index;
        holdButton.onHoldAction.AddListener(OnUpgradeButtonClick);
    }

    // 업그레이드 버튼이 눌리면 매니저에게 요청을 전달
    private void OnUpgradeButtonClick()
    {
        manager.AttemptUpgrade(statIndex);
    }

    // 매니저로부터 받은 정보로 UI를 갱신
    public void UpdateDisplay(string name, int level, BigInteger statValue, BigInteger cost, int amount)
    {
        statNameText.text = name;
        levelText.text = $"level {level}";
        statValueText.text = $"stat: {DataUtility.FormatNumber(statValue)}";
        upgradeCostText.text = $"cost (x{amount}): {DataUtility.FormatNumber(cost)}";
    }
}