using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 캐릭터 영혼 조각 보유 현황을 보여주는 스크롤 뷰를 관리합니다.
/// CharacterScrollViewUI의 패턴을 따라 오브젝트 풀링 및 이벤트 기반 업데이트를 사용합니다.
/// </summary>
public class SoulFragmentScrollViewUI : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject soulFragmentPanelPrefab;

    public SoulStatPanel characterStatPanel;

    // UI 패널들을 재사용하기 위한 오브젝트 풀
    private List<SoulFragmentPanel> panelPool = new List<SoulFragmentPanel>();

    private void OnEnable()
    {
        // 데이터 매니저가 준비되었을 때 이벤트 리스너를 등록하고 UI를 새로고칩니다.
        if (PlayerDataManager.Instance != null)
        {
            // 영혼 조각 데이터가 변경될 때마다 UI를 새로고침하도록 이벤트를 구독합니다.
            // 참고: PlayerDataManager에 영혼조각 전용 이벤트가 있다면 그것을 사용하는 것이 더 좋습니다.
            // 여기서는 기존 CharacterScrollViewUI처럼 OnOwnedCharactersChanged 이벤트를 사용합니다.
            PlayerDataManager.Instance.OnOwnedCharactersChanged += RefreshDisplay;
            RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("SoulFragmentScrollViewController: PlayerDataManager.Instance가 null입니다.");
        }
        ItemEventManager.Instance.OnClickPlayerItem += ShowPlayerStat;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 리스너를 해제하여 메모리 누수를 방지합니다.
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnOwnedCharactersChanged -= RefreshDisplay;
            ItemEventManager.Instance.OnClickPlayerItem -= ShowPlayerStat;
        }
    }

    /// <summary>
    /// 영혼 조각을 보유한 캐릭터 목록을 가져와 UI를 새로고칩니다.
    /// </summary>
    public void RefreshDisplay()
    {
        // 1. 영혼 조각을 1개 이상 가진 캐릭터 목록을 가져옵니다.
        List<PlayerCharacterData> charactersWithFragments = GetCharactersWithFragments();

        // 2. 필요한 만큼만 패널을 생성하여 풀(Pool)을 채웁니다.
        while (panelPool.Count < charactersWithFragments.Count)
        {
            GameObject panelGO = Instantiate(soulFragmentPanelPrefab, contentPanel);
            panelPool.Add(panelGO.GetComponent<SoulFragmentPanel>());
        }

        // 3. 풀에 있는 패널들에 데이터를 설정하고 활성화합니다.
        for (int i = 0; i < charactersWithFragments.Count; i++)
        {
            panelPool[i].SetUp(charactersWithFragments[i]);
            panelPool[i].gameObject.SetActive(true);
        }

        // 4. 사용하지 않는 나머지 패널들은 비활성화합니다.
        for (int i = charactersWithFragments.Count; i < panelPool.Count; i++)
        {
            panelPool[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// PlayerDataManager로부터 영혼 조각을 1개 이상 보유한 캐릭터의 데이터 리스트를 가져옵니다.
    /// </summary>
    /// <returns>영혼 조각을 보유한 캐릭터 데이터 리스트</returns>
    private List<PlayerCharacterData> GetCharactersWithFragments()
    {
        if (PlayerDataManager.Instance == null)
        {
            return new List<PlayerCharacterData>(); // 빈 리스트 반환
        }

        // ownedCharacters의 모든 값을 가져와서 Enumerable로 처리합니다.
        var charactersQuery = PlayerDataManager.Instance.ownedCharacters.Values.AsEnumerable();

        // 영혼 조각이 0보다 큰 캐릭터만 필터링합니다.
        charactersQuery = charactersQuery.Where(c =>
        {
            int fragmentCount;
            // characterSoulFragments 딕셔너리에서 해당 캐릭터의 조각 수를 찾아, 0보다 큰지 확인합니다.
            bool hasFragments = PlayerDataManager.Instance.characterSoulFragments.TryGetValue(c.characterdata.characterID, out fragmentCount);
            return hasFragments && fragmentCount > 0;
        });

        // 캐릭터 이름순으로 정렬합니다. (다른 정렬 기준을 원하면 여기를 수정)
        var sortedCharacters = charactersQuery.OrderBy(c => c.characterdata.characterName);

        return sortedCharacters.ToList();
    }


    private void ShowPlayerStat(PlayerCharacterData data)
    {
        if (data == null)
            return;

        characterStatPanel.gameObject.SetActive(true);
        characterStatPanel.Init(data);
    }
}
