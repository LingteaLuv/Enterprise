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

public class CharacterScrollViewUI : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject characterPanelPrefab;
    public TMP_Dropdown sortDropdown;
    public TMP_Dropdown crewRoleFilterDropdown;
    public TMP_Dropdown factionFilterDropdown; // 새로운 드롭다운 필드

    [Header("편성 모드")]
    public bool isFormationMode = false;
    public Button formationModeButton;
    public TextMeshProUGUI formationModeButtonText;
    public GameObject formationPanel;

    private CharacterSortOption currentSort = CharacterSortOption.Stars;
    private CrewRoleFilterOption currentCrewRoleFilter = CrewRoleFilterOption.All;
    private FactionFilterOption currentFactionFilter = FactionFilterOption.All; // 새로운 필터 변수
    private List<CharacterPanelUI> panelPool = new List<CharacterPanelUI>();

    void Start()
    {
        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
        crewRoleFilterDropdown.onValueChanged.AddListener(OnCrewRoleFilterDropdownChanged);
        // 새로운 드롭다운 리스너 추가
        factionFilterDropdown.onValueChanged.AddListener(OnFactionFilterDropdownChanged);

        if (formationModeButton != null)
        {
            formationModeButton.onClick.AddListener(ToggleFormationMode);
        }
        UpdateFormationButtonText();

        // 드롭다운 옵션 초기화
        PopulateSortDropdown(); // 새로 추가
        PopulateCrewRoleFilterDropdown();
        PopulateFactionFilterDropdown();
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
        RefreshDisplay();
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
            case Faction.Navy: return "해군";
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
        RefreshDisplay();
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
            PlayerDataManager.Instance.OnOwnedCharactersChanged += RefreshDisplay;
            PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;

            // PlayerDataManager가 준비되었을 때만 RefreshDisplay를 호출합니다.
            RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("CharacterScrollViewUI: PlayerDataManager.Instance가 OnEnable 시점에 null입니다. 이벤트 구독 및 초기 RefreshDisplay가 지연됩니다.");
        }
    }

    // 비활성화될 때 이벤트 리스너를 해제하여 메모리 누수를 방지합니다.
    private void OnDisable()
    {
        PlayerDataManager.Instance.OnOwnedCharactersChanged -= RefreshDisplay;
        PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
    }

    // OnCharacterDataUpdated 이벤트는 PlayerCharacterData를 전달하므로,
    // 이를 처리하기 위한 별도의 핸들러가 필요합니다.
    private void HandleCharacterUpdate(PlayerCharacterData data)
    {
        // 캐릭터 데이터가 업데이트되면 스크롤 뷰를 새로고침합니다.
        RefreshDisplay();
    }

    void OnSortDropdownChanged(int index)
    {
        currentSort = (CharacterSortOption)index;
        RefreshDisplay();
    }

    public void ToggleFormationMode()
    {
        // 편성 모드를 종료하려는 경우 (isFormationMode가 true에서 false로 바뀌기 직전)
        if (isFormationMode) // 현재 편성 모드 상태가 true라면, 이제 종료하려는 것
        {
            if (PlayerDataManager.Instance != null)
            {
                if (!PlayerDataManager.Instance.IsValidFormation())
                {
                    Debug.LogWarning("편성 오류: 편성이 완료되지 않았습니다! (모든 포지션에 1명씩, 총 5명)");
                    // 여기에 사용자에게 보여줄 UI 경고 메시지 로직 추가 (예: 팝업)
                    // 편성 모드를 종료하지 않고 유지합니다.
                    return; // 함수 종료, isFormationMode는 true로 유지됨
                }
            }
            else
            {
                Debug.LogError("PlayerDataManager.Instance가 null입니다. 편성 유효성 검사를 수행할 수 없습니다.");
                return;
            }
        }

        // 유효성 검사를 통과했거나, 편성 모드로 진입하는 경우 (isFormationMode가 false에서 true로 바뀌는 경우)
        isFormationMode = !isFormationMode;
        Debug.Log("편성 모드 상태: " + isFormationMode);
        UpdateFormationButtonText();
        RefreshDisplay();
        formationPanel.SetActive(isFormationMode);
    }

    private void UpdateFormationButtonText()
    {
        if (formationModeButtonText != null)
        {
            formationModeButtonText.text = isFormationMode ? "캐릭터 편성" : "캐릭터 정보";
        }
    }

    public void RefreshDisplay()
    {
        // 1. 정렬된 캐릭터 목록 가져오기
        List<PlayerCharacterData> characters = GetSortedCharacters();

        // 2. 필요한 만큼만 패널을 생성하여 풀(Pool)을 채웁니다.
        while (panelPool.Count < characters.Count)
        {
            GameObject panelGO = Instantiate(characterPanelPrefab, contentPanel);
            panelPool.Add(panelGO.GetComponent<CharacterPanelUI>());
        }

        // 3. 풀에 있는 패널들에 데이터를 설정하고 활성화합니다.
        for (int i = 0; i < characters.Count; i++)
        {
            panelPool[i].ownerScrollView = this;
            panelPool[i].Setup(characters[i]);
            panelPool[i].gameObject.SetActive(true);
        }

        // 4. 사용하지 않는 나머지 패널들은 비활성화합니다.
        for (int i = characters.Count; i < panelPool.Count; i++)
        {
            panelPool[i].gameObject.SetActive(false);
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