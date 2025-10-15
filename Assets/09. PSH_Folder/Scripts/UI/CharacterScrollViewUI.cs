using System.Collections.Generic;
using System.Linq;
using _05._CSJ_Folder.Scripts.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum CharacterSortOption
{
    Stars,
    Level
}

// CrewRole 필터링 옵션
public enum CrewRoleFilterOption
{
    All, // 전체
    Deckhand,
    Sailor,
    Cook,
    Captain
}

// Faction 필터링 옵션
public enum FactionFilterOption
{
    All, // 전체
    Pirate,
    Marine,
    Monster
}

// 1. MonoBehaviour 대신 UIBase를 상속받도록 변경
public class CharacterScrollViewUI : UIBase
{
    public Transform contentPanel;
    public GameObject characterPanelPrefab;
    public TMP_Dropdown sortDropdown;
    public TMP_Dropdown crewRoleFilterDropdown;
    public TMP_Dropdown factionFilterDropdown;

    [Header("UI 애니메이션")]
    [SerializeField] private RectTransform scrollViewRectTransform; // 높이를 변경할 UI
    [SerializeField] private float heightDecreaseAmount = 700f; // 편성 모드에서 줄어들 높이
    [SerializeField] private float heightAnimationDuration = 0.3f; // 애니메이션 시간

    private float initialHeight; // 초기 높이를 저장할 변수

    [Header("편성 모드")]
    public bool isFormationMode = false;
    public Button charInfoButton;
    public Button formationButton;
    public GameObject formationPanel;
    public Button saveButton;


    [Header("캐릭터 정보")]
    public CharacterInfoUI characterInfoPanel;

    public TextMeshProUGUI characterCountText;

    [Header("버튼 색상")]
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    private CharacterSortOption currentSort = CharacterSortOption.Stars;
    private CrewRoleFilterOption currentCrewRoleFilter = CrewRoleFilterOption.All;
    private FactionFilterOption currentFactionFilter = FactionFilterOption.All;
    private List<CharacterPanelUI> panelPool = new List<CharacterPanelUI>();
    private List<PlayerCharacterData> sortedCharacterList = new List<PlayerCharacterData>();

    public override void ResetPanel()
    {
        base.ResetPanel();

        if (characterInfoPanel != null && characterInfoPanel.gameObject.activeSelf)
        {
            characterInfoPanel.ResetPanel();
        }

        if (isFormationMode)
        {
            if (FormationManager.Instance.HasUnsavedChanges)
            {
                FormationManager.Instance.RevertFormationChanges();
            }
            SetFormationMode(false); // 상태를 확실히 되돌립니다.
        }
    }

    void Start()
    {
        if (scrollViewRectTransform != null)
        {
            initialHeight = scrollViewRectTransform.sizeDelta.y;
        }

        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
        crewRoleFilterDropdown.onValueChanged.AddListener(OnCrewRoleFilterDropdownChanged);
        factionFilterDropdown.onValueChanged.AddListener(OnFactionFilterDropdownChanged);

        charInfoButton?.onClick.AddListener(() => TrySetFormationMode(false));
        formationButton?.onClick.AddListener(() => TrySetFormationMode(true));
        saveButton?.onClick.AddListener(OnSaveFormation);

        UpdateFormationButtonVisuals();
        PopulateSortDropdown();
        PopulateCrewRoleFilterDropdown();
        PopulateFactionFilterDropdown();
        
        TutorialTargets.Register("FormationButton", formationButton?.transform as RectTransform);
        TutorialTargets.Register("SaveButton", saveButton?.transform as RectTransform);
    }

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnOwnedCharactersChanged += RefreshUI;
        PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;
        FormationManager.Instance.OnTempFormationChanged += RefreshUI; // 임시 편성 변경 시 UI 갱신
        //RefreshUI();
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.OnOwnedCharactersChanged -= RefreshUI;
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
        if (FormationManager.Instance != null) FormationManager.Instance.OnTempFormationChanged -= RefreshUI;
    }

    private void HandleCharacterUpdate(PlayerCharacterData data)
    {
        RefreshUI();
    }

    /// <summary>
    /// 편성 모드를 켜거나, 끄기를 '시도'합니다.
    /// </summary>
    public void TrySetFormationMode(bool enable)
    {
        if (isFormationMode == enable) return;

        if (enable)
        {
            SetFormationMode(true);
        }
        else
        {
            TryDisableFormationMode();
        }
    }

    /// <summary>
    /// 편성 모드를 안전하게 비활성화하려고 시도합니다. 탭 이동 전에 호출됩니다.
    /// </summary>
    /// <returns>모드 전환에 성공하면 true, 팝업 등으로 인해 중단되면 false</returns>
    public bool TryDisableFormationMode()
    {
        if (!isFormationMode) return true; // 이미 비활성 상태면 항상 성공

        if (!FormationManager.Instance.HasUnsavedChanges)
        {
            SetFormationMode(false);
            return true; // 변경사항 없으면 바로 끔
        }

        // 변경사항이 있을 경우, 유효성 검사부터 실행
        if (!FormationManager.Instance.ValidateFormation(out string errorMessage))
        {
            // 유효하지 않은 편성일 경우: 경고 팝업만 띄움
            PopManager.Instance.ShowOKPopup(errorMessage, "확인");
            return false; // 모드 전환 실패
        }
        else
        {
            // 유효하지만 저장되지 않은 변경사항이 있을 경우: 저장 여부 묻는 팝업
            PopManager.Instance.ShowOKCancelPopup(
                "저장되지 않은 변경사항이 있습니다. 저장하시겠습니까?",
                "저장",
                () => {
                    OnSaveFormation(); // 저장하고
                    SetFormationMode(false); // 모드 끄기
                },
                "저장 안함",
                () => {
                    OnRevertFormation(); // 변경사항 되돌리고
                    SetFormationMode(false); // 모드 끄기
                }
            );
            return false; // 팝업이 떴으므로, 유저의 선택을 기다려야 함. 일단 모드 전환은 보류.
        }
    }

    private void SetFormationMode(bool enable)
    {
        isFormationMode = enable;

        if (isFormationMode)
        {
            FormationManager.Instance.InitializeTempFormation();
        }

        // 편성 모드에 따라 ButtonDragHandler 활성화/비활성화
        foreach (var panel in panelPool)
        {
            var dragHandler = panel.GetComponent<ButtonDragHandler>();
            if (dragHandler != null)
            {
                // 편성 모드일 때는 비활성화, 아닐 때는 활성화
                dragHandler.enabled = !isFormationMode;
            }
        }

        UpdateFormationButtonVisuals();
        RefreshUI();
        // formationPanel.SetActive(isFormationMode);
        if (saveButton) saveButton.gameObject.SetActive(isFormationMode);

        // UI 높이 애니메이션
        if (scrollViewRectTransform != null)
        {
            // 편성 모드(enable=true)일 때는 줄어든 높이로, 일반 모드(enable=false)일 때는 초기 높이로 설정
            float targetHeight = enable ? initialHeight - heightDecreaseAmount : initialHeight;
            float currentHeight = scrollViewRectTransform.sizeDelta.y;

            // 높이 변화량의 절반만큼 Y위치를 보정해줘야 아래를 기준으로 늘어남/줄어듦
            float heightDifference = targetHeight - currentHeight;
            Vector2 targetPosition = scrollViewRectTransform.anchoredPosition + new Vector2(0, heightDifference / 2f);

            // 시퀀스를 사용해 높이와 위치를 동시에 애니메이션
            Sequence sequence = DOTween.Sequence();
            sequence.Append(scrollViewRectTransform.DOSizeDelta(new Vector2(scrollViewRectTransform.sizeDelta.x, targetHeight), heightAnimationDuration));
            sequence.Join(scrollViewRectTransform.DOAnchorPos(targetPosition, heightAnimationDuration));
            sequence.SetUpdate(true);
        }
    }

    private void OnSaveFormation()
    {
        if (FormationManager.Instance.SaveChanges(out string errorMessage))
        {
            PopManager.Instance.ShowOKPopup("편성이 저장되었습니다!", "확인");
        }
        else
        {
            PopManager.Instance.ShowOKPopup($"편성 저장 실패:\n{errorMessage}", "확인");
        }
        RefreshUI();
    }

    private void OnRevertFormation()
    {
        FormationManager.Instance.RevertFormationChanges();
        PopManager.Instance.ShowOKPopup("변경사항을 되돌렸습니다.", "확인");
        RefreshUI();
    }

    public override void RefreshUI()
    {
        sortedCharacterList = GetSortedCharacters();

        if (characterCountText != null)
        {
            int totalOwnedCount = PlayerDataManager.Instance.OwnedCharacters.Count;
            characterCountText.text = $"{totalOwnedCount} / 55";
        }

        while (panelPool.Count < sortedCharacterList.Count)
        {
            GameObject panelGO = Instantiate(characterPanelPrefab, contentPanel);
            panelPool.Add(panelGO.GetComponent<CharacterPanelUI>());
        }

        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            panelPool[i].ownerScrollView = this;
            panelPool[i].Setup(sortedCharacterList[i]);
            panelPool[i].gameObject.SetActive(true);
        }

        for (int i = sortedCharacterList.Count; i < panelPool.Count; i++)
        {
            panelPool[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 캐릭터 패널이 클릭되었을 때 호출될 핸들러
    /// </summary>
    private void OnCharacterPanelClicked(PlayerCharacterData character)
    {
        if (isFormationMode)
        {
            FormationManager.Instance.ToggleCharacterInTempFormation(character);
        }
        else
        {
            ShowCharacterInfo(character);
        }
    }

    public void ShowCharacterInfo(PlayerCharacterData character)
    {
        if (characterInfoPanel != null)
        {
            characterInfoPanel.Setup(character, sortedCharacterList);
        }
        else
        {
            Debug.LogError("CharacterInfoUI 패널이 CharacterScrollViewUI에 연결되지 않았습니다!");
        }
    }

    private List<PlayerCharacterData> GetSortedCharacters()
    {
        var charactersQuery = PlayerDataManager.Instance.OwnedCharacters.Values.AsEnumerable();

        if (currentFactionFilter != FactionFilterOption.All)
        {
            Faction targetFaction = (Faction)System.Enum.Parse(typeof(Faction), currentFactionFilter.ToString());
            charactersQuery = charactersQuery.Where(c => c.characterdata.faction == targetFaction);
        }

        if (currentCrewRoleFilter != CrewRoleFilterOption.All)
        {
            CrewRole targetRole = (CrewRole)System.Enum.Parse(typeof(CrewRole), currentCrewRoleFilter.ToString());
            charactersQuery = charactersQuery.Where(c => c.characterdata.crewRole == targetRole);
        }

        var sortedCharacters = charactersQuery.OrderByDescending(c =>
            isFormationMode ? FormationManager.Instance.IsInTempFormation(c) : PlayerDataManager.Instance.IsInFormation(c)
        );

        switch (currentSort)
        {
            case CharacterSortOption.Stars:
                sortedCharacters = sortedCharacters.ThenByDescending(c => c.Star.Value)
                                                   .ThenBy(c => c.characterdata.characterName);
                break;
            case CharacterSortOption.Level:
                sortedCharacters = sortedCharacters.ThenByDescending(c => c.Level.Value)
                                                   .ThenBy(c => c.characterdata.characterName);
                break;
        }

        return sortedCharacters.ToList();
    }

    private void UpdateFormationButtonVisuals()
    {
        if (charInfoButton == null || formationButton == null) return;

        // 버튼의 Image 컴포넌트를 직접 가져와서 색상을 변경해요.
        Image charInfoImage = charInfoButton.image;
        Image formationImage = formationButton.image;

        if (charInfoImage == null || formationImage == null)
        {
            Debug.LogError("버튼에 Image 컴포넌트가 없어");
            return;
        }

        if (isFormationMode)
        {
            charInfoImage.color = normalColor;
            formationImage.color = selectedColor;
        }
        else
        {
            charInfoImage.color = selectedColor;
            formationImage.color = normalColor;
        }
    }

    #region 드롭다운 UI 관련 (코드가 길어서 생략)
    private void PopulateSortDropdown()
    {
        sortDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (CharacterSortOption option in System.Enum.GetValues(typeof(CharacterSortOption)))
        {
            options.Add(GetLocalizedEnumName(option));
        }
        sortDropdown.AddOptions(options);
    }

    void OnFactionFilterDropdownChanged(int index)
    {
        currentFactionFilter = (FactionFilterOption)index;
        RefreshUI();
    }

    private void PopulateFactionFilterDropdown()
    {
        factionFilterDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (FactionFilterOption option in System.Enum.GetValues(typeof(FactionFilterOption)))
        {
            options.Add(GetLocalizedEnumName(option));
        }
        factionFilterDropdown.AddOptions(options);
    }

    private string GetLocalizedEnumName(System.Enum enumValue)
    {
        switch (enumValue)
        {
            case CrewRoleFilterOption.All: return "전체";
            case CrewRoleFilterOption.Deckhand: return "갑판원";
            case CrewRoleFilterOption.Sailor: return "선원";
            case CrewRoleFilterOption.Cook: return "요리사";
            case CrewRoleFilterOption.Captain: return "선장";

            case FactionFilterOption.All: return "전체";
            case FactionFilterOption.Pirate: return "해적";
            case FactionFilterOption.Marine: return "해군";
            case FactionFilterOption.Monster: return "괴물";

            case CrewRole.Deckhand: return "갑판원";
            case CrewRole.Sailor: return "선원";
            case CrewRole.Cook: return "요리사";
            case CrewRole.Captain: return "선장";

            case Faction.Pirate: return "해적";
            case Faction.Marine: return "해군";
            case Faction.Monster: return "괴물";

            case CharacterSortOption.Stars: return "성급별";
            case CharacterSortOption.Level: return "레벨별";

            default: return enumValue.ToString();
        }
    }

    void OnCrewRoleFilterDropdownChanged(int index)
    {
        currentCrewRoleFilter = (CrewRoleFilterOption)index;
        RefreshUI();
    }

    private void PopulateCrewRoleFilterDropdown()
    {
        crewRoleFilterDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (CrewRoleFilterOption option in System.Enum.GetValues(typeof(CrewRoleFilterOption)))
        {
            options.Add(GetLocalizedEnumName(option));
        }
        crewRoleFilterDropdown.AddOptions(options);
    }
    void OnSortDropdownChanged(int index)
    {
        currentSort = (CharacterSortOption)index;
        RefreshUI();
    }
    #endregion
}
