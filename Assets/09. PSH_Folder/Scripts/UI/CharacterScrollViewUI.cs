using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    Navy,
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

    [Header("편성 모드")]
    public bool isFormationMode = false;
    public Button charInfoButton;
    public Button formationButton;
    public GameObject formationPanel;

    [Header("캐릭터 정보")]
    public CharacterInfoUI characterInfoPanel; // 캐릭터 정보 패널 참조

    public TextMeshProUGUI characterCountText;

    [Header("버튼 색상")]
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    private CharacterSortOption currentSort = CharacterSortOption.Stars;
    private CrewRoleFilterOption currentCrewRoleFilter = CrewRoleFilterOption.All;
    private FactionFilterOption currentFactionFilter = FactionFilterOption.All;
    private List<CharacterPanelUI> panelPool = new List<CharacterPanelUI>();
    private List<PlayerCharacterData> sortedCharacterList = new List<PlayerCharacterData>(); // 정렬된 리스트 저장

    // 2. ResetPanel 메서드를 오버라이드하여 초기화 로직 구현
    public override void ResetPanel()
    {
        base.ResetPanel(); // 기본 ResetPanel 호출 (로그 출력 등)

        // 만약 캐릭터 상세 정보 패널이 열려있다면, 리셋하고 닫아줍니다.
        if (characterInfoPanel != null && characterInfoPanel.gameObject.activeSelf)
        {
            characterInfoPanel.ResetPanel();
            Debug.Log("CharacterScrollViewUI가 리셋되어, 열려있던 상세 정보창을 리셋하고 닫습니다.");
        }

        // 추가로, 편성 모드도 비활성화 상태로 되돌릴 수 있습니다.
        if (isFormationMode)
        {
            isFormationMode = false;
            UpdateFormationButtonVisuals();
            if (formationPanel != null) formationPanel.SetActive(false);
        }
    }

    void Start()
    {
        // 정렬, 필터 드롭다운
        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
        crewRoleFilterDropdown.onValueChanged.AddListener(OnCrewRoleFilterDropdownChanged);
        factionFilterDropdown.onValueChanged.AddListener(OnFactionFilterDropdownChanged);

        if (charInfoButton != null)
        {
            charInfoButton.onClick.AddListener(() => TryDisableFormationMode());
        }
        if (formationButton != null)
        {
            formationButton.onClick.AddListener(EnableFormationMode);
        }
        UpdateFormationButtonVisuals();

        // 드롭다운 옵션 초기화
        PopulateSortDropdown();
        PopulateCrewRoleFilterDropdown();
        PopulateFactionFilterDropdown();
    }

    private void UpdateFormationButtonVisuals()
    {
        if (charInfoButton == null || formationButton == null) return;

        var charInfoButtonColors = charInfoButton.colors;
        var formationButtonColors = formationButton.colors;

        if (isFormationMode)
        {
            charInfoButtonColors.normalColor = normalColor;
            formationButtonColors.normalColor = selectedColor;
        }
        else
        {
            charInfoButtonColors.normalColor = selectedColor;
            formationButtonColors.normalColor = normalColor;
        }

        charInfoButton.colors = charInfoButtonColors;
        formationButton.colors = formationButtonColors;
    }

    // SortDropdown 옵션을 채우는 함수
    private void PopulateSortDropdown()
    {
        sortDropdown.ClearOptions(); // 기존 옵션 제거
        List<string> options = new List<string>();
        foreach (CharacterSortOption option in System.Enum.GetValues(typeof(CharacterSortOption)))
        {
            options.Add(GetLocalizedEnumName(option)); // 헬퍼 함수 사용
        }
        sortDropdown.AddOptions(options);
    }

    // 새로운 드롭다운 값 변경 시 호출될 함수
    void OnFactionFilterDropdownChanged(int index)
    {
        currentFactionFilter = (FactionFilterOption)index;
        RefreshUI();
    }

    // FactionFilterDropdown 옵션을 채우는 함수
    private void PopulateFactionFilterDropdown()
    {
        factionFilterDropdown.ClearOptions(); // 기존 옵션 제거
        List<string> options = new List<string>();
        foreach (FactionFilterOption option in System.Enum.GetValues(typeof(FactionFilterOption)))
        {
            options.Add(GetLocalizedEnumName(option)); // 헬퍼 함수 사용
        }
        factionFilterDropdown.AddOptions(options);
    }

    // Enum 값을 한글 문자열로 변환하는 헬퍼 함수
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
            case FactionFilterOption.Navy: return "해군";
            case FactionFilterOption.Monster: return "괴물";

            // CrewRole enum 값도 필요하다면 여기에 추가
            case CrewRole.Deckhand: return "갑판원";
            case CrewRole.Sailor: return "선원";
            case CrewRole.Cook: return "요리사";
            case CrewRole.Captain: return "선장";

            // Faction enum 값도 필요하다면 여기에 추가
            case Faction.Pirate: return "해적";
            case Faction.Marine: return "해군";
            case Faction.Monster: return "괴물";

            // --- ADD THIS ---
            case CharacterSortOption.Stars: return "성급별";
            case CharacterSortOption.Level: return "레벨별";
            // --- END ADDITION ---

            default: return enumValue.ToString(); // 매핑되지 않은 값은 기본 영어 이름 사용
        }
    }

    // 새로운 드롭다운 값 변경 시 호출될 함수
    void OnCrewRoleFilterDropdownChanged(int index)
    {
        currentCrewRoleFilter = (CrewRoleFilterOption)index;
        RefreshUI();
    }

    // CrewRoleFilterDropdown 옵션을 채우는 함수
    private void PopulateCrewRoleFilterDropdown()
    {
        crewRoleFilterDropdown.ClearOptions(); // 기존 옵션 제거
        List<string> options = new List<string>();
        foreach (CrewRoleFilterOption option in System.Enum.GetValues(typeof(CrewRoleFilterOption)))
        {
            options.Add(GetLocalizedEnumName(option)); // 헬퍼 함수 사용
        }
        crewRoleFilterDropdown.AddOptions(options);
    }

    // 이 UI 오브젝트가 활성화될 때마다 목록을 새로고침하고, 이벤트 리스너를 등록합니다.
    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnOwnedCharactersChanged += RefreshUI;
            PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;

            // PlayerDataManager가 준비되었을 때만 RefreshDisplay를 호출합니다.
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("CharacterScrollViewUI: PlayerDataManager.Instance가 OnEnable 시점에 null입니다. 이벤트 구독 및 초기 RefreshDisplay가 지연됩니다.");
        }
    }

    // 비활성화될 때 이벤트 리스너를 해제하여 메모리 누수를 방지합니다.
    private void OnDisable()
    {
        PlayerDataManager.Instance.OnOwnedCharactersChanged -= RefreshUI;
        PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
    }

    // OnCharacterDataUpdated 이벤트는 PlayerCharacterData를 전달하므로,
    // 이를 처리하기 위한 별도의 핸들러가 필요합니다.
    private void HandleCharacterUpdate(PlayerCharacterData data)
    {
        // 캐릭터 데이터가 업데이트되면 스크롤 뷰를 새로고침합니다.
        RefreshUI();
    }

    void OnSortDropdownChanged(int index)
    {
        currentSort = (CharacterSortOption)index;
        RefreshUI();
    }

    public void EnableFormationMode()
    {
        isFormationMode = true;
        Debug.Log("편성 모드 활성화");
        UpdateFormationButtonVisuals();
        RefreshUI();
        formationPanel.SetActive(true);
    }

    public bool TryDisableFormationMode()
    {
        if (PlayerDataManager.Instance != null)
        {
            int a = PlayerDataManager.Instance.IsValidFormation();
            if (a == 1)
            {
                UIManager.Instance.ShowWarning("모든 포지션에 최소 1명을 배치해야합니다.");
                return false;
            }
            else if (a == 2)
            {
                UIManager.Instance.ShowWarning("배치 인원이 5명이 아닙니다.");
                return false;
            }
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance가 null입니다. 편성 유효성 검사를 수행할 수 없습니다.");
            return false;
        }

        isFormationMode = false;
        Debug.Log("편성 모드 비활성화");
        UpdateFormationButtonVisuals();
        RefreshUI();
        if (formationPanel != null) formationPanel.SetActive(false);
        return true;
    }

    public override void RefreshUI()
    {
        // 1. 정렬된 캐릭터 목록 가져오기 및 저장
        sortedCharacterList = GetSortedCharacters();

        // 보유 캐릭터 수 텍스트
        if (characterCountText != null)
        {
            // PlayerDataManager에서 전체 보유 캐릭터 수를 가져옵니다.
            int totalOwnedCount = PlayerDataManager.Instance.ownedCharacters.Count;
            // 필터링으로 현재 표시되는 캐릭터 수와 전체 보유 수를 텍스트로 보여줍니다.
            characterCountText.text = $"{totalOwnedCount} / 55";
        }

        // 2. 필요한 만큼만 패널을 생성하여 풀(Pool)을 채웁니다.
        while (panelPool.Count < sortedCharacterList.Count)
        {
            GameObject panelGO = Instantiate(characterPanelPrefab, contentPanel);
            panelPool.Add(panelGO.GetComponent<CharacterPanelUI>());
        }

        // 3. 풀에 있는 패널들에 데이터를 설정하고 활성화합니다.
        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            panelPool[i].ownerScrollView = this;
            panelPool[i].Setup(sortedCharacterList[i]);
            panelPool[i].gameObject.SetActive(true);
        }

        // 4. 사용하지 않는 나머지 패널들은 비활성화합니다.
        for (int i = sortedCharacterList.Count; i < panelPool.Count; i++)
        {
            panelPool[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 캐릭터 정보 패널을 열고 데이터를 설정합니다.
    /// </summary>
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
        var charactersQuery = PlayerDataManager.Instance.ownedCharacters.Values.AsEnumerable();

        // --- FACTION 필터링 로직 추가 ---
        if (currentFactionFilter != FactionFilterOption.All)
        {
            // FactionFilterOption과 Faction enum의 값이 일치한다고 가정
            Faction targetFaction = (Faction)System.Enum.Parse(typeof(Faction), currentFactionFilter.ToString());
            charactersQuery = charactersQuery.Where(c => c.characterdata.faction == targetFaction);
        }
        // --- FACTION 필터링 로직 끝 ---

        // --- CrewRole 필터링 로직 (기존) ---
        if (currentCrewRoleFilter != CrewRoleFilterOption.All)
        {
            // CrewRoleFilterOption과 CrewRole enum의 값이 일치한다고 가정
            CrewRole targetRole = (CrewRole)System.Enum.Parse(typeof(CrewRole), currentCrewRoleFilter.ToString());
            charactersQuery = charactersQuery.Where(c => c.characterdata.crewRole == targetRole);
        }
        // --- CrewRole 필터링 로직 끝 ---

        // 1. 편성에 포함된 캐릭터를 먼저 정렬 (IsInFormation이 true인 캐릭터가 먼저 오도록)
        var sortedCharacters = charactersQuery.OrderByDescending(c => PlayerDataManager.Instance.IsInFormation(c));

        // 2. 기존 정렬 기준 (성급, 레벨)을 그 다음으로 적용
        switch (currentSort)
        {
            case CharacterSortOption.Stars:
                sortedCharacters = sortedCharacters.ThenByDescending(c => c.stars)
                                                   .ThenBy(c => c.characterdata.characterName); // 이름으로 2차 정렬
                break;
            case CharacterSortOption.Level:
                sortedCharacters = sortedCharacters.ThenByDescending(c => c.characterLevel)
                                                   .ThenBy(c => c.characterdata.characterName); // 이름으로 2차 정렬
                break;
            default:
                // 기본 정렬 (편성 여부만)
                break;
        }

        return sortedCharacters.ToList();
    }
}