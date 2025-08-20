using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro 네임스페이스 추가
using System.Numerics; // BigInteger 사용을 위해 추가

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("캐릭터 편성")]
    public List<PlayerCharacterData> formationCharacters = new List<PlayerCharacterData>();
    public const int MAX_FORMATION_SIZE = 5;
    public BigInteger teamBattlePower;

    [Header("캐릭터 레벨업 비용 설정")]
    public BigInteger baseLevelUpCost = 1000; // 기본 레벨업 비용
    public double levelUpCostIncreaseRatio = 1.07; // 레벨업 비용 증가율

    public Dictionary<CharacterData, PlayerCharacterData> ownedCharacters = new Dictionary<CharacterData, PlayerCharacterData>();
    public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();
    private Dictionary<int, int> starUpgradeCosts;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializeUpgradeCosts();
    }

    private void Start()
    {
        // 게임 시작 시 모든 캐릭터의 스탯을 한 번 계산해줍니다.
        // 다른 모든 Start() 함수가 실행된 후를 보장하기 위해, 첫 프레임의 끝에서 실행합니다.
        StartCoroutine(InitialCalculationCoroutine());
    }

    private IEnumerator InitialCalculationCoroutine()
    {
        // 모든 Start() 함수가 실행된 후인 첫 프레임의 끝까지 기다립니다.
        yield return new WaitForEndOfFrame();

        Debug.Log("초기 스탯 계산을 시작합니다.");
        RecalculateAllCharacterStats();
    }

    private void OnEnable()
    {
        StatEvents.OnCharacterBattlePowerChanged += HandleCharacterBattlePowerChange;
        BasicStatManager.OnBaseStatsChanged += RecalculateAllCharacterStats; // 기본 스탯 변경 시 모든 캐릭터 스탯 재계산
    }

    private void OnDisable()
    {
        StatEvents.OnCharacterBattlePowerChanged -= HandleCharacterBattlePowerChange;
        BasicStatManager.OnBaseStatsChanged -= RecalculateAllCharacterStats;
    }

    private void InitializeUpgradeCosts()
    {
        starUpgradeCosts = new Dictionary<int, int>()
        {
            { 1, 20 }, { 2, 40 }, { 3, 120 }, { 4, 180 }
        };
    }

    public PlayerCharacterData AddCharacter(CharacterData characterdata)
    {
        if (ownedCharacters.TryGetValue(characterdata, out PlayerCharacterData existingCharData))
        {
            int fragmentsGained = 0;
            switch (characterdata.rarity)
            {
                case Rarity.C: fragmentsGained = 1; break;
                case Rarity.B: fragmentsGained = 4; break;
                case Rarity.A: fragmentsGained = 30; break;
            }
            AddSoulFragments(characterdata.characterID, fragmentsGained);
            Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata);

            // 추가한 캐릭터의 전투력을 계산해놓고 저장
            newCharData.RecaculateStats();
            ownedCharacters.Add(characterdata, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({characterdata.rarity}성) 획득!");
            return newCharData;
        }
    }

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
    }

    public bool TryUpgradeCharacterStar(PlayerCharacterData playerCharData)
    {
        if (playerCharData == null) { Debug.LogError("업그레이드할 캐릭터 데이터가 null입니다."); return false; }
        int characterId = playerCharData.characterdata.characterID;
        if (playerCharData.stars >= 5) { Debug.LogWarning($"{playerCharData.characterdata.characterName}은(는) 이미 최대 성급입니다."); return false; }
        if (!starUpgradeCosts.TryGetValue(playerCharData.stars, out int cost)) { Debug.LogError($"현재 성급 {playerCharData.stars} 업그레이드 비용이 정의되지 않았습니다."); return false; }
        if (!characterSoulFragments.ContainsKey(characterId) || characterSoulFragments[characterId] < cost)
        {
            Debug.LogWarning($"{playerCharData.characterdata.characterName}의 영혼 조각이 부족합니다!");
            return false;
        }
        characterSoulFragments[characterId] -= cost;
        playerCharData.stars++;
        Debug.Log($"{playerCharData.characterdata.characterName}이(가) {playerCharData.stars}성으로 승급했습니다!");
        return true;
    }

    public bool TryGetUpgradeCost(int currentStarLevel, out int cost)
    {
        return starUpgradeCosts.TryGetValue(currentStarLevel, out cost);
    }

    public bool TryLevelUpCharacter(PlayerCharacterData character)
    {
        BigInteger levelUpCost = (BigInteger)((double)baseLevelUpCost * System.Math.Pow(levelUpCostIncreaseRatio, character.characterLevel - 1));
        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.EnhancementStone, levelUpCost)) { Debug.LogWarning("캐릭터 레벨업 실패: 재화 부족"); return false; }
        character.characterLevel++;
        Debug.Log($"{character.characterdata.characterName} 레벨업! (Lv.{character.characterLevel})");
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }

    public int AddCharacterToFormation(PlayerCharacterData characterData)
    {
        if (formationCharacters.Contains(characterData)) { Debug.Log($"{characterData.characterdata.characterName}은(는) 이미 편성에 포함되어 있습니다."); return 1; }
        if (formationCharacters.Count >= MAX_FORMATION_SIZE) { Debug.Log($"편성이 가득 찼습니다."); return 2; }
        formationCharacters.Add(characterData);
        Debug.Log($"{characterData.characterdata.characterName}을(를) 편성에 추가했습니다.");
        RecalculateTeamBattlePower();
        return 0;
    }

    public bool RemoveCharacterFromFormation(PlayerCharacterData characterData)
    {
        if (formationCharacters.Remove(characterData))
        {
            Debug.Log($"{characterData.characterdata.characterName}을(를) 편성에서 제거했습니다.");
            RecalculateTeamBattlePower();
            return true;
        }
        return false;
    }

    public bool IsInFormation(PlayerCharacterData characterData)
    {
        return formationCharacters.Contains(characterData);
    }

    private void HandleCharacterBattlePowerChange(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        if (IsInFormation(character))
        {
            Debug.Log($"{character.characterdata.characterName}의 전투력 변경으로 팀 전투력을 재계산합니다.");
            RecalculateTeamBattlePower();
        }
    }

    public void RecalculateTeamBattlePower()
    {
        BigInteger oldTeamPower = teamBattlePower;
        BigInteger newTeamPower = 0;
        foreach (var character in formationCharacters)
        {
            newTeamPower += character.battlePower;
        }
        teamBattlePower = newTeamPower;
        if (oldTeamPower != teamBattlePower)
        {
            StatEvents.RaiseTeamBattlePowerChanged(oldTeamPower, teamBattlePower);
        }
    }

    /// <summary>
    /// 보유한 모든 캐릭터의 스탯을 재계산합니다.
    /// </summary>
    public void RecalculateAllCharacterStats()
    {
        Debug.Log("모든 캐릭터의 스탯을 재계산합니다...");
        foreach (var character in ownedCharacters.Values)
        {
            character.RecaculateStats();
        }
    }
}

