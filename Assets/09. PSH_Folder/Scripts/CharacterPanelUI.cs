

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelUI : MonoBehaviour
{
    [Header("기본 UI 요소")]
    public Image characterImage;
    public TextMeshProUGUI levelText;
    public Image[] starImages; // 별 이미지 배열
    public Button button; // 캐릭터 인포 여는 버튼

    [Header("영혼 조각 UI")]
    public TextMeshProUGUI soulFragmentText; // 예: "15 / 20"
    public Slider soulFragmentSlider;

    private GameObject characterInfoUIPanel;
    private PlayerCharacterData currentPlayerCharData; // 이 패널이 표시하는 캐릭터 데이터

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPanelButtonClicked);

        // 비활성화된 오브젝트를 포함하여 CharacterInfoUI를 찾습니다.
        characterInfoUIPanel = FindAnyObjectByType<CharacterInfoUI>(FindObjectsInactive.Include)?.gameObject;
    }

    /// <summary>
    /// 캐릭터 데이터로 이 패널의 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data)
    {
        currentPlayerCharData = data;

        // 캐릭터 기본 정보 설정
        characterImage.sprite = data.characterdata.characterSprite;
        levelText.text = $"Lv.{data.level}";

        // 성급(별) UI 업데이트
        UpdateStarUI(data.stars);

        // 영혼 조각 UI 업데이트
        UpdateSoulFragmentUI();
    }

    /// <summary>
    /// 성급(별) UI를 현재 성급에 맞게 갱신합니다.
    /// </summary>
    private void UpdateStarUI(int currentStars)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].color = (i < currentStars) ? Color.yellow : Color.grey;
        }
    }

    /// <summary>
    /// 영혼 조각 텍스트와 슬라이더 UI를 갱신합니다.
    /// </summary>
    private void UpdateSoulFragmentUI()
    {
        int characterId = currentPlayerCharData.characterdata.characterID;

        // 현재 보유량 가져오기
        int currentFragments = 0;
        PlayerDataManager.Instance.characterSoulFragments.TryGetValue(characterId, out currentFragments);

        // 다음 성급 필요량 가져오기
        bool hasNextStar = PlayerDataManager.Instance.TryGetUpgradeCost(currentPlayerCharData.stars, out int requiredFragments);

        if (hasNextStar)
        {
            // 승급 가능할 때
            soulFragmentText.text = $"{currentFragments} / {requiredFragments}";
            if (soulFragmentSlider != null)
            {
                soulFragmentSlider.maxValue = requiredFragments;
                soulFragmentSlider.value = currentFragments;
            }
        }
        else
        {
            // 최대 성급일 때
            soulFragmentText.text = "MAX";
            if (soulFragmentSlider != null)
            {
                soulFragmentSlider.maxValue = 1;
                soulFragmentSlider.value = 1; // 슬라이더를 꽉 채움
            }
        }
    }


    /// <summary>
    /// 패널 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnPanelButtonClicked()
    {
        if (characterInfoUIPanel != null)
        {
            characterInfoUIPanel.SetActive(true);

            CharacterInfoUI infoUI = characterInfoUIPanel.GetComponent<CharacterInfoUI>();
            if (infoUI != null)
            {
                infoUI.Setup(currentPlayerCharData);
            }
            else
            {
                Debug.LogWarning("CharacterInfoUI 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("CharacterInfoUI Panel을 찾을 수 없습니다.");
        }
    }
}

