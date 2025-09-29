using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SynergyUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject synergyUIPanel; // 시너지 UI 전체를 감싸는 패널
    public TextMeshProUGUI mainSynergyText; // 메인으로 표시될 시너지 이름
    public Button showAllSynergiesButton; // 여러 시너지일 때 활성화될 버튼

    [Header("Optional: For displaying multiple synergies")]
    public GameObject allSynergiesPanel; // 모든 시너지를 표시할 패널 (팝업 등)
    public TextMeshProUGUI allSynergiesText; // 모든 시너지 이름을 표시할 텍스트

    void Awake()
    {
        // 초기에는 비활성화 상태로 시작
        synergyUIPanel.SetActive(false);
        if (allSynergiesPanel != null)
        {
            allSynergiesPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        // FormationManager의 임시 편성 변경 이벤트 구독
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged += UpdateSynergyUI;
        }
        // 초기 UI 업데이트 (혹시 OnEnable 시점에 이미 시너지가 있을 경우)
        UpdateSynergyUI();
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged -= UpdateSynergyUI;
        }
    }

    void Start()
    {
        // 버튼 리스너 추가
        if (showAllSynergiesButton != null)
        {
            showAllSynergiesButton.onClick.AddListener(OnShowAllSynergiesButtonClicked);
        }
    }

    private void UpdateSynergyUI()
    {
        if (SynergyManager.Instance == null)
        {
            Debug.LogWarning("SynergyManager.Instance가 초기화되지 않았습니다.");
            synergyUIPanel.SetActive(false);
            return;
        }

        IReadOnlyList<SynergySO> currentSynergies = SynergyManager.Instance.PreviewSynergies;

        if (currentSynergies == null || currentSynergies.Count == 0)
        {
            // 시너지가 하나도 없으면 UI 비활성화
            synergyUIPanel.SetActive(false);
            if (allSynergiesPanel != null) allSynergiesPanel.SetActive(false);
            Debug.Log("현재 활성화된 시너지가 없습니다. Synergy UI 비활성화.");
        }
        else if (currentSynergies.Count == 1)
        {
            // 시너지가 하나면 해당 시너지 이름만 표시하고 버튼 비활성화
            synergyUIPanel.SetActive(true);
            mainSynergyText.text = currentSynergies[0].synergyName;
            if (showAllSynergiesButton != null) showAllSynergiesButton.gameObject.SetActive(false);
            if (allSynergiesPanel != null) allSynergiesPanel.SetActive(false);
            Debug.Log($"단일 시너지 활성화: {currentSynergies[0].synergyName}");
        }
        else
        {
            // 시너지가 여러 개면 첫 번째 시너지 이름 표시하고 버튼 활성화
            synergyUIPanel.SetActive(true);
            mainSynergyText.text = $"{currentSynergies[0].synergyName} 외 {currentSynergies.Count - 1}개"; // 예시: "불의 맹세 외 2개"
            if (showAllSynergiesButton != null) showAllSynergiesButton.gameObject.SetActive(true);
            if (allSynergiesPanel != null) allSynergiesPanel.SetActive(false); // 여러 시너지 패널은 일단 닫아둠
            Debug.Log($"여러 시너지 활성화. 첫 번째: {currentSynergies[0].synergyName}, 총 {currentSynergies.Count}개.");
        }
    }

    private void OnShowAllSynergiesButtonClicked()
    {
        if (allSynergiesPanel != null)
        {
            // 모든 시너지 패널 토글
            allSynergiesPanel.SetActive(!allSynergiesPanel.activeSelf);

            if (allSynergiesPanel.activeSelf && allSynergiesText != null)
            {
                // 패널이 활성화되면 모든 시너지 이름 표시
                List<string> synergyNames = new List<string>();
                foreach (var synergy in SynergyManager.Instance.PreviewSynergies)
                {
                    synergyNames.Add(synergy.synergyName);
                }
                allSynergiesText.text = "활성화된 시너지:" + string.Join("", synergyNames);
                Debug.Log("모든 시너지 보기 버튼 클릭됨. 패널 활성화.");
            }
            else
            {
                Debug.Log("모든 시너지 보기 버튼 클릭됨. 패널 비활성화.");
            }
        }
        else
        {
            // 모든 시너지 패널이 없으면 로그로 출력
            Debug.Log("모든 시너지 패널이 설정되지 않았습니다. 시너지 목록:");
            foreach (var synergy in SynergyManager.Instance.PreviewSynergies)
            {
                Debug.Log($"- {synergy.synergyName}");
            }
        }
    }
}