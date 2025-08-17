using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public TextMeshProUGUI characterNameText;
    public Image characterImage;
    public TextMeshProUGUI levelText;

    [Header("닫기 버튼")]
    public Button closeButton;

    [Header("승급 UI")]
    public TextMeshProUGUI upgradeCostText; // 승급 비용을 표시할 TextMeshPro UI
    public Button upgradeButton; // 승급 버튼

    private PlayerCharacterData currentCharacterData; // 현재 표시 중인 캐릭터 데이터

    private CharacterScrollViewUI scrollView;

    void Awake()
    {
        // 초기에는 비활성화
        gameObject.SetActive(false);

        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        scrollView = FindFirstObjectByType<CharacterScrollViewUI>();
    }

    /// <summary>
    /// 캐릭터 정보를 설정하고 패널을 활성화합니다.
    /// </summary>
    /// <param name="data">표시할 플레이어 캐릭터 데이터</param>
    public void Setup(PlayerCharacterData data)
    {
        currentCharacterData = data;

        if (data != null)
        {
            characterNameText.text = data.characterdata.characterName;
            characterImage.sprite = data.characterdata.characterSprite;
            levelText.text = $"Lv.{data.level}";

            // 승급 UI 업데이트
            UpdateUpgradeUI();

            // 패널 활성화
            gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("CharacterInfoUI: 전달된 캐릭터 데이터가 null입니다.");
            ClosePanel(); // 데이터가 없으면 패널 닫기
        }
    }

    /// <summary>
    /// 승급 UI (비용, 버튼 활성화/비활성화)를 갱신합니다.
    /// </summary>
    private void UpdateUpgradeUI()
    {
        if (currentCharacterData == null) return;

        // 최대 성급인지 확인
        if (currentCharacterData.stars >= 5) // 5성이 최대 성급이라고 가정
        {
            upgradeCostText.text = "MAX";
            upgradeButton.interactable = false;
            return;
        }

        // 다음 성급에 필요한 비용 가져오기
        int nextStarLevel = currentCharacterData.stars + 1;
        int cost = 0;
        if (PlayerDataManager.Instance.TryGetUpgradeCost(currentCharacterData.stars, out cost))
        {
            upgradeCostText.text = $"need {cost}";

            // 영혼 조각이 충분한지 확인하여 버튼 활성화/비활성화
            if (PlayerDataManager.Instance.soulFragments >= cost)
            {
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.interactable = false;
            }
        }
        else
        {
            upgradeCostText.text = "mollu"; // 정의되지 않은 성급
            upgradeButton.interactable = false;
        }

        // 승급 버튼 클릭 이벤트 연결 (중복 연결 방지)
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        //레벨 스텟 등 갱신
        levelText.text = $"Lv.{currentCharacterData.level}";
    }

    /// <summary>
    /// 승급 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (currentCharacterData == null) return;

        // PlayerDataManager를 통해 승급 시도
        bool success = PlayerDataManager.Instance.TryUpgradeCharacterStar(currentCharacterData);

        if (success)
        {
            // 승급 성공 시 UI 갱신
            // 현재 정보 패널의 UI를 갱신
            Setup(currentCharacterData); // 현재 캐릭터 데이터로 다시 Setup 호출하여 UI 갱신

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
    /// 캐릭터 정보 패널을 비활성화합니다.
    /// </summary>
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // 테스트용 레벨 업 진짜 레벨만 업함
    public void LevelUp()
    {
        currentCharacterData.level++;
        scrollView.RefreshDisplay();
        UpdateUpgradeUI();
    }
}