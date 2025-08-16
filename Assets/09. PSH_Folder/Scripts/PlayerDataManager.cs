
using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshPro 네임스페이스 추가

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI soulFragmentsText; // 영혼 조각을 표시할 TextMeshPro UI

    // 보유한 모든 캐릭터 데이터를 저장하는 딕셔너리 (Key: CharacterSO, Value: 해당 캐릭터의 상태 데이터)
    public Dictionary<CharacterData, PlayerCharacterData> ownedCharacters = new Dictionary<CharacterData, PlayerCharacterData>();

    // 공용 재화
    public int soulFragments = 0; // 영혼 조각

    // 성급 업그레이드 비용 (현재 성급 -> 다음 성급에 필요한 영혼 조각)
    private Dictionary<int, int> starUpgradeCosts;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializeUpgradeCosts(); // 성급 업그레이드 비용 초기화
        UpdateSoulFragmentsUI(); // 게임 시작 시 UI 초기화
    }

    private void InitializeUpgradeCosts()
    {
        starUpgradeCosts = new Dictionary<int, int>()
        {
            { 1, 20 }, // 1성 -> 2성
            { 2, 40 }, // 2성 -> 3성
            { 3, 120 }, // 3성 -> 4성
            { 4, 180 }  // 4성 -> 5성
        };
    }

    /// <summary>
    /// 새로운 캐릭터를 획득했을 때 호출되는 함수
    /// </summary>
    /// <param name="characterdata">획득한 캐릭터의 ScriptableObject</param>
    public void AddCharacter(CharacterData characterdata)
    {
        // 이미 보유한 캐릭터인지 확인
        if (ownedCharacters.ContainsKey(characterdata))
        {
            // 중복 획득: 등급에 따라 영혼 조각으로 변환
            int fragmentsGained = 0;
            switch (characterdata.rarity)
            {
                case Rarity.C:
                    fragmentsGained = 1;
                    break;
                case Rarity.B:
                    fragmentsGained = 4;
                    break;
                case Rarity.A:
                    fragmentsGained = 30;
                    break;
            }
            soulFragments += fragmentsGained;
            Debug.Log($"[중복] {characterdata.characterName}({characterdata.rarity}) 획득! 영혼 조각 +{fragmentsGained}. 현재 영혼 조각: {soulFragments}");

            UpdateSoulFragmentsUI(); // 영혼 조각 UI 갱신
        }
        else
        {
            // 신규 획득: 새로운 캐릭터 데이터 생성 후 딕셔너리에 추가
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata);
            ownedCharacters.Add(characterdata, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({characterdata.rarity}) 획득!");
        }
    }

    /// <summary>
    /// 캐릭터의 성급을 업그레이드 시도합니다.
    /// </summary>
    /// <param name="playerCharData">업그레이드할 플레이어 캐릭터 데이터</param>
    /// <returns>업그레이드 성공 여부</returns>
    public bool TryUpgradeCharacterStar(PlayerCharacterData playerCharData)
    {
        if (playerCharData == null)
        {
            Debug.LogError("업그레이드할 캐릭터 데이터가 null입니다.");
            return false;
        }

        // 최대 성급 확인 (예: 5성이 최대라고 가정)
        if (playerCharData.stars >= 5)
        {
            Debug.LogWarning($"{playerCharData.characterdata.characterName}은(는) 이미 최대 성급입니다. (현재 {playerCharData.stars}성)");
            return false;
        }

        // 다음 성급에 필요한 비용 확인
        int nextStarLevel = playerCharData.stars + 1;
        if (!starUpgradeCosts.TryGetValue(playerCharData.stars, out int cost))
        {
            Debug.LogError($"현재 성급 {playerCharData.stars}에서 다음 성급으로의 업그레이드 비용이 정의되지 않았습니다.");
            return false;
        }

        // 영혼 조각 충분한지 확인
        if (soulFragments < cost)
        {
            Debug.LogWarning($"영혼 조각이 부족합니다! (필요: {cost}, 현재: {soulFragments})");
            return false;
        }

        // 업그레이드 진행
        soulFragments -= cost;
        playerCharData.stars = nextStarLevel;
        Debug.Log($"{playerCharData.characterdata.characterName}이(가) {playerCharData.stars}성으로 승급했습니다! 영혼 조각 {cost}개 소모.");

        UpdateSoulFragmentsUI(); // 영혼 조각 UI 갱신
        return true;
    }

    /// <summary>
    /// 영혼 조각 UI 텍스트를 현재 값으로 갱신합니다.
    /// </summary>
    private void UpdateSoulFragmentsUI()
    {
        if (soulFragmentsText != null)
        {
            soulFragmentsText.text = soulFragments.ToString();
        }
    }

    /// <summary>
    /// 특정 성급에서 다음 성급으로 업그레이드하는 데 필요한 비용을 가져옵니다.
    /// </summary>
    /// <param name="currentStarLevel">현재 성급</param>
    /// <param name="cost">필요한 영혼 조각 비용 (out 파라미터)</param>
    /// <returns>비용을 성공적으로 가져왔는지 여부</returns>
    public bool TryGetUpgradeCost(int currentStarLevel, out int cost)
    {
        return starUpgradeCosts.TryGetValue(currentStarLevel, out cost);
    }
}
