
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Button 사용을 위해 추가

public enum CharacterSortOption
{
    Stars,
    Level
}
public class CharacterScrollViewUI : MonoBehaviour
{
    public Transform contentPanel; // UI 패널들이 자식으로 추가될 Content Transform
    public GameObject characterPanelPrefab; // 캐릭터 패널 UI 프리팹
    public TMP_Dropdown sortDropdown;

    [Header("편성 모드")]
    public bool isFormationMode = false;
    public Button formationModeButton; // 유니티 에디터에서 연결할 편성 모드 버튼
    public TextMeshProUGUI formationModeButtonText; // 편성 모드 버튼의 텍스트 (옵션)

    private CharacterSortOption currentSort = CharacterSortOption.Stars;

    void Start()
    {
        // 드롭다운 값 변경 시 호출
        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);

        // 편성 모드 버튼 리스너 연결
        if (formationModeButton != null)
        {
            formationModeButton.onClick.AddListener(ToggleFormationMode);
        }
        UpdateFormationButtonText(); // 초기 버튼 텍스트 설정
    }

    void OnSortDropdownChanged(int index)
    {
        currentSort = (CharacterSortOption)index;
        RefreshDisplay();
    }

    /// <summary>
    /// 편성 모드를 켜고 끕니다. 버튼에 의해 호출됩니다.
    /// </summary>
    public void ToggleFormationMode()
    {
        isFormationMode = !isFormationMode;
        Debug.Log("편성 모드 상태: " + isFormationMode);
        UpdateFormationButtonText();

        // 패널들의 시각적 상태를 업데이트하기 위해 새로고침
        RefreshDisplay();
    }

    /// <summary>
    /// 편성 모드 버튼의 텍스트를 현재 상태에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateFormationButtonText()
    {
        if (formationModeButtonText != null)
        {
            formationModeButtonText.text = isFormationMode ? "캐릭터 편성" : "캐릭터 정보";
        }
    }


    /// <summary>
    /// 현재 플레이어 데이터 기준으로 스크롤 뷰 전체를 다시 그립니다.
    /// </summary>
    public void RefreshDisplay()
    {
        // 1. 기존 UI 패널 제거
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 정렬된 캐릭터 목록 가져오기
        var characters = PlayerDataManager.Instance.ownedCharacters.Values.AsEnumerable();

        switch (currentSort)
        {
            case CharacterSortOption.Stars:
                characters = characters.OrderByDescending(c => c.stars).ThenBy(c => c.characterdata.characterName)
        .ToList();
                break;
            case CharacterSortOption.Level:
                characters = characters.OrderByDescending(c => c.characterLevel).ThenBy(c => c.characterdata.characterName)
        .ToList();
                break;
        }

        // 3. 정렬된 캐릭터 목록으로 UI 생성
        foreach (PlayerCharacterData charData in characters)
        {
            GameObject panelGO = Instantiate(characterPanelPrefab, contentPanel);
            CharacterPanelUI panelUI = panelGO.GetComponent<CharacterPanelUI>();
            if (panelUI != null)
            {
                panelUI.ownerScrollView = this; // CharacterPanelUI에 이 스크립트의 참조를 넘겨줌
                panelUI.Setup(charData);
            }
        }
    }
}
