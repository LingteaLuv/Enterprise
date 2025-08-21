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

public class CharacterScrollViewUI : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject characterPanelPrefab;
    public TMP_Dropdown sortDropdown;

    [Header("편성 모드")]
    public bool isFormationMode = false;
    public Button formationModeButton;
    public TextMeshProUGUI formationModeButtonText;

    private CharacterSortOption currentSort = CharacterSortOption.Stars;
    private List<CharacterPanelUI> panelPool = new List<CharacterPanelUI>();

    void Start()
    {
        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
        if (formationModeButton != null)
        {
            formationModeButton.onClick.AddListener(ToggleFormationMode);
        }
        UpdateFormationButtonText();
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
        isFormationMode = !isFormationMode;
        Debug.Log("편성 모드 상태: " + isFormationMode);
        UpdateFormationButtonText();
        RefreshDisplay();
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
        switch (currentSort)
        {
            case CharacterSortOption.Stars:
                return charactersQuery.OrderByDescending(c => c.stars)
                                      .ThenBy(c => c.characterdata.characterName)
                                      .ToList();
            case CharacterSortOption.Level:
                return charactersQuery.OrderByDescending(c => c.characterLevel)
                                      .ThenBy(c => c.characterdata.characterName)
                                      .ToList();
            default:
                return charactersQuery.ToList();
        }
    }
}