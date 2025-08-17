
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterScrollViewUI : MonoBehaviour
{
    public Transform contentPanel; // UI 패널들이 자식으로 추가될 Content Transform
    public GameObject characterPanelPrefab; // 캐릭터 패널 UI 프리팹

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

        // 2. 정렬된 캐릭터 목록 가져오기 (성급 내림차순 → 이름순)
        var sortedCharacters = PlayerDataManager.Instance.ownedCharacters.Values
            .OrderByDescending(c => c.stars)
            .ThenBy(c => c.characterdata.characterName)
            .ToList();

        // 3. 정렬된 캐릭터 목록으로 UI 생성
        foreach (PlayerCharacterData charData in sortedCharacters)
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
