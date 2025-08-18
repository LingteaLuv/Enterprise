
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
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

    private CharacterSortOption currentSort = CharacterSortOption.Stars;

    void Start()
    {
        // 드롭다운 값 변경 시 호출
        sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
    }
    void OnSortDropdownChanged(int index)
    {
        currentSort = (CharacterSortOption)index;
        RefreshDisplay();
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
                characters = characters.OrderByDescending(c => c.level).ThenBy(c => c.characterdata.characterName)
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
                panelUI.Setup(charData);
            }
        }
    }
}
