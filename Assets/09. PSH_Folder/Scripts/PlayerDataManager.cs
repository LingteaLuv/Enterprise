using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro 네임스페이스 추가
using System.Numerics; // BigInteger 사용을 위해 추가

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("캐릭터 편성")]
    public Dictionary<CrewRole, List<PlayerCharacterData>> formation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
    public const int MAX_FORMATION_SIZE = 5;
    public BigInteger teamBattlePower;

    [Header("캐릭터 레벨업 비용 설정")]
    public BigInteger baseLevelUpCost = 1000; // 기본 레벨업 비용
    public double levelUpCostIncreaseRatio = 1.07; // 레벨업 비용 증가율

    public Dictionary<CharacterData, PlayerCharacterData> ownedCharacters = new Dictionary<CharacterData, PlayerCharacterData>();
    public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();
    private Dictionary<int, int> starUpgradeCosts;

    private bool isBatchUpdating = false; // 일괄 업데이트 상태 플래그

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        formation = new Dictionary<CrewRole, List<PlayerCharacterData>>
        {
            { CrewRole.Deckhand, new List<PlayerCharacterData>() }, // 전
            { CrewRole.Sailor, new List<PlayerCharacterData>() },   // 중
            { CrewRole.Cook, new List<PlayerCharacterData>() },     // 후
            { CrewRole.Captain, new List<PlayerCharacterData>() }   // 최후
        };

        InitializeUpgradeCosts();
    }

    private void Start()
    {
        StartCoroutine(InitialCalculationCoroutine());
    }

    private IEnumerator InitialCalculationCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("초기 스탯 계산을 시작합니다.");
        RecalculateAllCharacterStats();
    }

    private void OnEnable()
    {
        StatEvents.OnCharacterBattlePowerChanged += HandleCharacterBattlePowerChange;
        BasicStatManager.OnBaseStatsChanged += HandleBaseStatsChanged;
    }

    private void OnDisable()
    {
        StatEvents.OnCharacterBattlePowerChanged -= HandleCharacterBattlePowerChange;
        BasicStatManager.OnBaseStatsChanged -= HandleBaseStatsChanged;
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
            ownedCharacters.Add(characterdata, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({characterdata.rarity}성) 획득!");
            // 새로 추가된 캐릭터의 스탯을 즉시 계산
            newCharData.RecaculateStats();
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

    public int GetFormationCharacterCount()
    {
        int count = 0;
        foreach (var list in formation.Values)
        {
            count += list.Count;
        }
        return count;
    }

    public int AddCharacterToFormation(PlayerCharacterData characterData)
    {
        CrewRole position = characterData.characterdata.crewrole;

        // 편성이 가득 찼는지 확인
        if (GetFormationCharacterCount() >= MAX_FORMATION_SIZE)
        {
            Debug.Log("편성이 가득 찼습니다.");
            return 2;
        }

        // 캐릭터가 이미 다른 곳에 편성되어 있는지 확인
        if (IsInFormation(characterData))
        {
            Debug.Log($"{characterData.characterdata.characterName}은(는) 이미 편성에 포함되어 있습니다.");
            return 1;
        }

        // 포지션별 규칙 확인
        List<PlayerCharacterData> positionList = formation[position];
        if (position == CrewRole.Captain && positionList.Count >= 1)
        {
            Debug.Log("최후(Captain) 포지션에는 한 명만 배치할 수 있습니다.");
            return 4; // 포지션 가득 참
        }
        if (position != CrewRole.Captain && positionList.Count >= 2)
        {
            Debug.Log($"{position} 포지션에는 두 명까지만 배치할 수 있습니다.");
            return 4;
        }

        // 캐릭터 추가
        positionList.Add(characterData);
        Debug.Log($"{characterData.characterdata.characterName}을(를) {position} 포지션에 추가했습니다.");
        RecalculateTeamBattlePower();
        return 0; // 성공
    }

    public bool RemoveCharacterFromFormation(PlayerCharacterData characterData)
    {
        CrewRole position = characterData.characterdata.crewrole;
        if (formation.ContainsKey(position) && formation[position].Remove(characterData))
        {
            Debug.Log($"{characterData.characterdata.characterName}을(를) {position} 포지션에서 제거했습니다.");
            RecalculateTeamBattlePower();
            return true;
        }
        Debug.LogWarning($"{characterData.characterdata.characterName}을(를) 편성에서 찾을 수 없습니다.");
        return false;
    }

    public bool IsInFormation(PlayerCharacterData characterData)
    {
        CrewRole position = characterData.characterdata.crewrole;
        if (formation.ContainsKey(position))
        {
            return formation[position].Contains(characterData);
        }
        return false;
    }

    private void HandleCharacterBattlePowerChange(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        if (isBatchUpdating) return; // 일괄 업데이트 중에는 개별 처리를 건너뜀

        if (IsInFormation(character))
        {
            Debug.Log($"{character.characterdata.characterName}의 전투력 변경으로 팀 전투력을 재계산합니다.");
            RecalculateTeamBattlePower();
        }
    }

    private void HandleBaseStatsChanged()
    {
        Debug.Log("기본 스탯 변경 감지. 일괄 업데이트를 시작합니다.");
        isBatchUpdating = true;

        RecalculateAllCharacterStats();
        RecalculateTeamBattlePower(); // 모든 캐릭터 업데이트 후, 팀 전투력 최종 계산

        isBatchUpdating = false;
        Debug.Log("일괄 업데이트 완료.");
    }

    public void RecalculateTeamBattlePower()
    {
        BigInteger oldTeamPower = teamBattlePower;
        BigInteger newTeamPower = 0;
        foreach (var list in formation.Values)
        {
            foreach (var character in list)
            {
                newTeamPower += character.battlePower;
            }
        }
        teamBattlePower = newTeamPower;
        if (oldTeamPower != teamBattlePower)
        {
            StatEvents.RaiseTeamBattlePowerChanged(oldTeamPower, teamBattlePower);
        }
    }

    public void RecalculateAllCharacterStats()
    {
        Debug.Log("모든 캐릭터의 스탯을 재계산합니다...");
        foreach (var character in ownedCharacters.Values)
        {
            character.RecaculateStats();
        }
    }
}
