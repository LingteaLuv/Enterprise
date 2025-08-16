
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Image characterImage;
    public TextMeshProUGUI levelText;
    public Image[] starImages; // 별 이미지 배열 추가
    
    public TextMeshProUGUI upgradeCostText; // 승급 비용을 표시할 TextMeshPro UI
    public Button button; // 캐릭터 인포 여는 버튼
    public GameObject characterInfoUIPanel;

    private PlayerCharacterData currentPlayerCharData; // 이 패널이 표시하는 캐릭터 데이터

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPanelButtonClicked);

        characterInfoUIPanel = FindAnyObjectByType<CharacterInfoUI>(FindObjectsInactive.Include)?.gameObject;

    }

    /// <summary>
    /// 캐릭터 데이터로 이 패널의 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data)
    {
        currentPlayerCharData = data; // 현재 캐릭터 데이터 저장

        // 캐릭터 기본 정보 설정
        characterImage.sprite = data.characterdata.characterSprite;
        levelText.text = $"Lv.{data.level}";

        // 성급(별) 표시 (색상 변경)
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < data.stars)
            {
                starImages[i].color = Color.yellow; // 활성화된 별은 노란색
            }
            else
            {
                starImages[i].color = Color.grey; // 비활성화된 별은 회색
            }
        }
    }

    /// <summary>
    /// 승급 UI (비용, 버튼 활성화/비활성화)를 갱신합니다.
    /// </summary>
    private void UpdateUpgradeUI()
    {
        if (currentPlayerCharData == null) return;

        // 최대 성급인지 확인
        if (currentPlayerCharData.stars >= 5) // 5성이 최대 성급이라고 가정
        {
            upgradeCostText.text = "MAX";
            button.interactable = false;
            return;
        }

        // 다음 성급에 필요한 비용 가져오기
        int nextStarLevel = currentPlayerCharData.stars + 1;
        int cost = 0;
        if (PlayerDataManager.Instance.TryGetUpgradeCost(currentPlayerCharData.stars, out cost))
        {
            upgradeCostText.text = $"0 / {cost}";

            // 영혼 조각이 충분한지 확인하여 버튼 활성화/비활성화
            if (PlayerDataManager.Instance.soulFragments >= cost)
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        else
        {
            upgradeCostText.text = "mollu?"; // 정의되지 않은 성급
            button.interactable = false;
        }
    }

    /// <summary>
    /// 승급 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (currentPlayerCharData == null) return;

        // PlayerDataManager를 통해 승급 시도
        bool success = PlayerDataManager.Instance.TryUpgradeCharacterStar(currentPlayerCharData);

        if (success)
        {
            CharacterScrollViewUI scrollView = FindFirstObjectByType<CharacterScrollViewUI>();
            if (scrollView != null)
            {
                scrollView.RefreshDisplay();
            }
            else
            {
                Debug.LogWarning("CharacterScrollViewUI를 찾을 수 없습니다. UI 갱신에 문제가 있을 수 있습니다.");
            }
        }
        // 실패 시 PlayerDataManager에서 이미 로그를 출력하므로 별도 처리 없음
    }

    /// <summary>
    /// 패널 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnPanelButtonClicked()
    {
        if (characterInfoUIPanel != null)
        {
            characterInfoUIPanel.SetActive(true); // 정보 패널 활성화

            // CharacterInfoUI 컴포넌트를 찾아서 데이터 설정
            CharacterInfoUI infoUI = characterInfoUIPanel.GetComponent<CharacterInfoUI>();
            if (infoUI != null)
            {
                infoUI.Setup(currentPlayerCharData); // 캐릭터 데이터 전달
            }
            else
            {
                Debug.LogWarning("CharacterInfoUI 컴포넌트를 찾을 수 없습니다. 캐릭터 정보 패널에 스크립트가 연결되어 있는지 확인하세요.");
            }
        }
        else
        {
            Debug.LogWarning("CharacterInfoUI Panel이 CharacterPanelUI에 연결되지 않았습니다.");
        }
        Debug.Log($"캐릭터 패널 클릭됨: {currentPlayerCharData.characterdata.characterName}");
    }
}
