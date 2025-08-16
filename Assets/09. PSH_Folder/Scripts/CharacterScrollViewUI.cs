
using UnityEngine;
using System.Collections.Generic;

public class CharacterScrollViewUI : MonoBehaviour
{
    public Transform contentPanel; // UI 패널들이 자식으로 추가될 Content Transform
    public GameObject characterPanelPrefab; // 캐릭터 패널 UI 프리팹

    /// <summary>
    /// 현재 플레이어 데이터 기준으로 스크롤 뷰 전체를 다시 그립니다.
    /// </summary>
    public void RefreshDisplay()
    {
        // 1. 기존에 있던 모든 UI 패널들을 삭제합니다.
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 플레이어 데이터 매니저에서 보유 캐릭터 목록을 가져옵니다.
        Dictionary<CharacterData, PlayerCharacterData> ownedCharacters = PlayerDataManager.Instance.ownedCharacters;

        // 3. 보유한 모든 캐릭터에 대해 UI 패널을 하나씩 생성합니다.
        foreach (PlayerCharacterData charData in ownedCharacters.Values)
        {
            GameObject panelGO = Instantiate(characterPanelPrefab, contentPanel);
            CharacterPanelUI panelUI = panelGO.GetComponent<CharacterPanelUI>();
            if (panelUI != null)
            {
                // 생성된 패널에 캐릭터 데이터를 넘겨주어 UI를 설정합니다.
                panelUI.Setup(charData);
            }
        }
    }
}
