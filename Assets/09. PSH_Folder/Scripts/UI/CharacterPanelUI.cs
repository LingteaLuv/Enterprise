

using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("편성 UI")]
    public GameObject formationIndicator; // 편성에 포함되었는지 표시하는 UI 오브젝트 (예: 체크마크 이미지)

    [HideInInspector]
    public CharacterScrollViewUI ownerScrollView; // 부모 스크롤 뷰

    [Header("직업, 속성 아이콘")]
    public Image crewRoleIcon;
    public Image factionIcon;

    private RedDotController redDotController; // 레드닷 컨트롤러 참조
    public PlayerCharacterData currentPlayerCharData { get; private set; } // 이 패널이 표시하는 캐릭터 데이터

    private void Awake()
    {
        // 자식 오브젝트에서 RedDotController를 자동으로 찾아 할당합니다.
        redDotController = GetComponentInChildren<RedDotController>(true);
    }

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPanelButtonClicked);
    }

    /// <summary>
    /// 캐릭터 데이터로 이 패널의 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data)
    {
        currentPlayerCharData = data;

        // 캐릭터 기본 정보 설정
        characterImage.sprite = data.characterdata.characterSprite;
        levelText.text = $"Lv.{data.characterLevel}";

        // 성급(별) UI 업데이트
        UpdateStarUI(data.stars);

        // 영혼 조각 UI 업데이트
        UpdateSoulFragmentUI();

        // 편성 상태 시각화 업데이트
        UpdateFormationVisuals();

        // 직업, 속성 아이콘 업데이트
        UpdateIcon();

        // 레드닷 컨트롤러를 통해 알림 상태를 업데이트합니다.
        if (redDotController != null)
        {
            redDotController.CheckNotifications(currentPlayerCharData);
        }
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
    /// PlayerCharacterData에서 아이콘을 가져옵니다
    /// </summary>
    public void UpdateIcon()
    {
        crewRoleIcon.sprite = currentPlayerCharData.crewRoleIcon;
        factionIcon.sprite = currentPlayerCharData.factionIcon;
    }

    /// <summary>
    /// 현재 편성 상태에 따라 시각적 표시를 업데이트합니다.
    /// </summary>
    public void UpdateFormationVisuals()
    {
        if (formationIndicator == null || ownerScrollView == null) return;

        bool isInFormation;
        // 편성 모드인지 아닌지에 따라 다른 데이터를 확인합니다.
        if (ownerScrollView.isFormationMode)
        {
            // 편성 모드일 때: FormationManager의 임시 편성 정보를 확인
            isInFormation = FormationManager.Instance.IsInTempFormation(currentPlayerCharData);
        }
        else
        {
            // 일반 모드일 때: PlayerDataManager의 실제 저장된 편성 정보를 확인
            isInFormation = PlayerDataManager.Instance.IsInFormation(currentPlayerCharData);
        }

        formationIndicator.SetActive(isInFormation);
    }


    /// <summary>
    /// 패널 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnPanelButtonClicked()
    {
        // 편성 모드일 때는 DraggableCharacter 스크립트가 입력을 처리하므로,
        // 여기서는 캐릭터 정보창을 여는 로직만 남겨둡니다.
        if (ownerScrollView != null)
        {
            // 편성 모드가 아닐 때만 정보창을 엽니다.
            if (!ownerScrollView.isFormationMode)
            {
                ownerScrollView.ShowCharacterInfo(currentPlayerCharData);
            }
        }
        else
        {
            Debug.LogError("CharacterPanelUI에 ownerScrollView가 연결되지 않았습니다!");
        }
    }
}


