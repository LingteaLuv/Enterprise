using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshPro 네임스페이스 추가
using System.Numerics; // BigInteger 사용을 위해 추가

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI soulFragmentsText; // 영혼 조각을 표시할 TextMeshPro UI

    [Header("캐릭터 레벨업 비용 설정")]
    public BigInteger baseLevelUpCost = 1000; // 기본 레벨업 비용
    public double levelUpCostIncreaseRatio = 1.07; // 레벨업 비용 증가율

    // 보유한 모든 캐릭터 데이터를 저장하는 딕셔너리 (Key: CharacterSO, Value: 해당 캐릭터의 상태 데이터)
    public Dictionary<CharacterData, PlayerCharacterData> ownedCharacters = new Dictionary<CharacterData, PlayerCharacterData>();

    // 캐릭터별 영혼 조각 (Key: 캐릭터 ID, Value: 조각 개수)
    public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();

    // 성급 업그레이드 비용 (현재 성급 -> 다음 성급에 필요한 영혼 조각)
    private Dictionary<int, int> starUpgradeCosts;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializeUpgradeCosts(); // 성급 업그레이드 비용 초기화
        // UpdateSoulFragmentsUI(); // TODO: UI 로직을 캐릭터별로 수정해야 함
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
            switch (characterdata.rarity) // CharacterData에 rarity(성급) 변수가 있다고 가정
            {
                case Rarity.C: // 1성
                    fragmentsGained = 1;
                    break;
                case Rarity.B: // 2성
                    fragmentsGained = 4;
                    break;
                case Rarity.A: // 3성
                    fragmentsGained = 30;
                    break;
            }

            AddSoulFragments(characterdata.characterID, fragmentsGained);
        }
        else
        {
            // 신규 획득: 새로운 캐릭터 데이터 생성 후 딕셔너리에 추가
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata);
            ownedCharacters.Add(characterdata, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({characterdata.rarity}성) 획득!");
        }
    }

    /// <summary>
    /// 특정 캐릭터의 영혼 조각을 추가합니다.
    /// </summary>
    /// <param name="characterId">캐릭터의 고유 ID</param>
    /// <param name="amount">추가할 조각의 양</param>
    public void AddSoulFragments(int characterId, int amount)
    {
        if (characterSoulFragments.ContainsKey(characterId))
        {
            characterSoulFragments[characterId] += amount;
        }
        else
        {
            characterSoulFragments.Add(characterId, amount);
        }
        Debug.Log($"캐릭터 ID {characterId}의 영혼 조각 +{amount}. 현재: {characterSoulFragments[characterId]}개");
        // TODO: 해당 캐릭터의 영혼 조각 UI 갱신 로직 필요
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

        int characterId = playerCharData.characterdata.characterID;

        // 최대 성급 확인 (예: 5성이 최대라고 가정)
        if (playerCharData.stars >= 5)
        {
            Debug.LogWarning($"{playerCharData.characterdata.characterName}은(는) 이미 최대 성급입니다. (현재 {playerCharData.stars}성)");
            return false;
        }

        // 다음 성급에 필요한 비용 확인
        if (!starUpgradeCosts.TryGetValue(playerCharData.stars, out int cost))
        {
            Debug.LogError($"현재 성급 {playerCharData.stars}에서 다음 성급으로의 업그레이드 비용이 정의되지 않았습니다.");
            return false;
        }

        // 해당 캐릭터의 영혼 조각이 충분한지 확인
        if (!characterSoulFragments.ContainsKey(characterId) || characterSoulFragments[characterId] < cost)
        {
            int currentFragments = characterSoulFragments.ContainsKey(characterId) ? characterSoulFragments[characterId] : 0;
            Debug.LogWarning($"{playerCharData.characterdata.characterName}의 영혼 조각이 부족합니다! (필요: {cost}, 현재: {currentFragments})");
            return false;
        }

        // 업그레이드 진행
        characterSoulFragments[characterId] -= cost;
        playerCharData.stars++;
        Debug.Log($"{playerCharData.characterdata.characterName}이(가) {playerCharData.stars}성으로 승급했습니다! 영혼 조각 {cost}개 소모.");

        // TODO: UI 갱신 로직 필요
        return true;
    }

    /// <summary>
    /// 영혼 조각 UI 텍스트를 현재 값으로 갱신합니다. (TODO: 캐릭터별 UI 로직으로 수정 필요)
    /// </summary>
    private void UpdateSoulFragmentsUI()
    {
        // if (soulFragmentsText != null)
        // {
        //     // 이 함수는 이제 특정 캐릭터의 영혼 조각을 표시하도록 수정되어야 합니다.
        //     // 예를 들어, 현재 선택된 캐릭터의 영혼 조각을 표시하는 방식으로 변경해야 합니다.
        //     // soulFragmentsText.text = characterSoulFragments[selectedCharacterId].ToString();
        // }
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

    /// <summary>
    /// 특정 캐릭터의 레벨업을 시도합니다.
    /// </summary>
    /// <param name="character">레벨업할 대상 캐릭터</param>
    /// <returns>성공 여부</returns>
    public bool TryLevelUpCharacter(PlayerCharacterData character)
    {
        // 현재 레벨에 따른 레벨업 비용 계산
        // BigInteger와 double의 곱셈 오류를 해결하기 위해 double로 캐스팅 후 계산
        BigInteger levelUpCost = (BigInteger)((double)baseLevelUpCost * System.Math.Pow(levelUpCostIncreaseRatio, character.level - 1));
        CurrencyType costType = CurrencyType.Gold; // 비용 재화 타입

        // 재화 확인 및 소모
        if (!InventoryManager.Instance.SpendCurrency(costType, levelUpCost))
        {
            Debug.LogWarning($"캐릭터 레벨업 실패: {costType} 재화 부족. 필요: {levelUpCost}");
            return false;
        }

        // 레벨업 진행
        character.level++;
        Debug.Log($"{character.characterdata.characterName} 레벨업! (Lv.{character.level - 1} -> Lv.{character.level})");

        // TODO: 레벨업에 따른 추가 보상 로직 (스탯 증가 외)
        InventoryManager.Instance.UpdateCurrencyUI();
        return true;
    }
}