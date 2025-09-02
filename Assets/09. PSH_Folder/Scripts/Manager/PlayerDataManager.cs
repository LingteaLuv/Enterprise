using JHT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    [Header("시작 캐릭터 설정")]
    [Tooltip("게임 시작 시 기본으로 지급할 캐릭터 목록")]
    public List<CharacterData> startingCharacters = new List<CharacterData>();

    [Header("캐릭터 편성")]
    public Dictionary<CrewRole, List<PlayerCharacterData>> formation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
    public const int MAX_FORMATION_SIZE = 5;
    public BigInteger teamBattlePower;

    [Header("캐릭터 레벨업 비용 설정")]
    public BigInteger baseLevelUpCost = 1000; // 기본 레벨업 비용
    public double levelUpCostIncreaseRatio = 1.07; // 레벨업 비용 증가율

    // 보유 캐릭터 ID, 데이터
    public Dictionary<int, PlayerCharacterData> ownedCharacters = new Dictionary<int, PlayerCharacterData>();
    // 캐릭터별 영혼조각 캐릭터ID, 개수
    public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();
    // 성급업에 필요한 영혼조각 InitializeUpgradeCosts에 있음
    private Dictionary<int, int> starUpgradeCosts;
    // 성급업 비용 상수
    private const int STAR_UPGRADE_COST_1 = 20;
    private const int STAR_UPGRADE_COST_2 = 40;
    private const int STAR_UPGRADE_COST_3 = 120;
    private const int STAR_UPGRADE_COST_4 = 180;
    // 중복 획득시 영혼조각
    private const int FRAGMENT_GAIN_1 = 10;
    private const int FRAGMENT_GAIN_2 = 40;
    private const int FRAGMENT_GAIN_3 = 300;
    // 가챠 횟수
    public int GachaPityCounter { get; set; }


    public event System.Action<PlayerCharacterData> OnCharacterDataUpdated;
    public event System.Action OnOwnedCharactersChanged;

    private bool isBatchUpdating = false; // 일괄 업데이트 상태 플래그

    protected override void Awake()
    {
        base.Awake();

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
        // 게임 시작 시 보유 캐릭터가 없으면 기본 캐릭터 지급
        if (ownedCharacters.Count == 0)
        {
            GrantStartingCharacters();
            // 기본 캐릭터 지급 후, 자동으로 팀을 편성합니다.
            Debug.Log("기본 캐릭터 지급 완료. 자동 편성을 시작합니다.");
            AutoFormTeam();
        }

        StartCoroutine(InitialCalculationCoroutine());
    }

    private void GrantStartingCharacters()
    {
        Debug.Log("기본 지급 캐릭터가 있는지 확인합니다...");
        if (startingCharacters == null || startingCharacters.Count == 0)
        {
            Debug.Log("기본으로 지급할 캐릭터가 설정되지 않았습니다.");
            return;
        }

        Debug.Log($"{startingCharacters.Count}명의 기본 캐릭터를 지급합니다.");
        foreach (CharacterData characterSO in startingCharacters)
        {
            if (characterSO != null)
            {
                AddCharacter(characterSO);
            }
            else
            {
                Debug.LogWarning("Starting Characters 리스트에 null인 항목이 있습니다.");
            }
        }
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
            { 1, STAR_UPGRADE_COST_1 },
            { 2, STAR_UPGRADE_COST_2 },
            { 3, STAR_UPGRADE_COST_3 },
            { 4, STAR_UPGRADE_COST_4 }
        };
    }

    public PlayerCharacterData AddCharacter(CharacterData characterdata)
    {
        if (ownedCharacters.TryGetValue(characterdata.characterID, out PlayerCharacterData existingCharData))
        {
            int fragmentsGained = 0;
            switch (characterdata.rarity)
            {
                case Rarity.C: fragmentsGained = FRAGMENT_GAIN_1; break;
                case Rarity.B: fragmentsGained = FRAGMENT_GAIN_2; break;
                case Rarity.A: fragmentsGained = FRAGMENT_GAIN_3; break;
            }
            AddSoulFragments(characterdata.characterID, fragmentsGained);
            Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            OnOwnedCharactersChanged?.Invoke(); // 캐릭터 목록 변경(조각 획득) 이벤트 발생
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata);
            ownedCharacters.Add(characterdata.characterID, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({characterdata.rarity}성) 획득!");
            // 새로 추가된 캐릭터의 스탯을 즉시 계산
            newCharData.RecaculateStats();
            OnOwnedCharactersChanged?.Invoke(); // 캐릭터 목록 변경(신규 획득) 이벤트 발생
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
        OnCharacterDataUpdated?.Invoke(playerCharData); // 데이터 변경 이벤트 발생
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
        QuestSignalManager.Instance.LevelUp(ItemType.Character, 1);
        character.RecaculateStats();
        OnCharacterDataUpdated?.Invoke(character); // 데이터 변경 이벤트 발생
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
        CrewRole position = characterData.characterdata.crewRole;

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
            return 3; // 포지션 가득 참
        }
        if (position != CrewRole.Captain && positionList.Count >= 2)
        {
            Debug.Log($"{position} 포지션에는 두 명까지만 배치할 수 있습니다.");
            return 4;
        }

        // 캐릭터 추가
        positionList.Add(characterData);
        Debug.Log($"[PDM] {characterData.characterdata.characterName}을(를) {position} 포지션에 추가했습니다.");
        RecalculateTeamBattlePower();
        OnOwnedCharactersChanged?.Invoke(); // 이벤트 발생
        Debug.Log("[PDM] OnOwnedCharactersChanged 이벤트 발생!");
        return 0; // 성공
    }

    public bool RemoveCharacterFromFormation(PlayerCharacterData characterData)
    {
        CrewRole position = characterData.characterdata.crewRole;
        if (formation.ContainsKey(position) && formation[position].Remove(characterData))
        {
            Debug.Log($"{characterData.characterdata.characterName}을(를) {position} 포지션에서 제거했습니다.");
            RecalculateTeamBattlePower();
            OnOwnedCharactersChanged?.Invoke(); // 이벤트 발생
            Debug.Log("[PDM] OnOwnedCharactersChanged 이벤트 발생!");
            return true;
        }
        Debug.LogWarning($"{characterData.characterdata.characterName}을(를) 편성에서 찾을 수 없습니다.");
        return false;
    }

    public bool IsInFormation(PlayerCharacterData characterData)
    {
        CrewRole position = characterData.characterdata.crewRole;
        if (formation.ContainsKey(position))
        {
            return formation[position].Contains(characterData);
        }
        return false;
    }

    #region 장비 관리

    /// <summary>
    /// 캐릭터에게 무기를 장착시킵니다. (스탯/직업 검사 없음)
    /// </summary>
    public bool EquipWeapon(PlayerCharacterData character, WeaponObject weapon)
    {
        if (character == null || weapon == null)
        {
            Debug.LogError("캐릭터 또는 무기 데이터가 null입니다.");
            return false;
        }

        // 다른 캐릭터가 이미 장착했는지 확인
        if (weapon.EquippedByCharacterId != null)
        {
            Debug.LogWarning($"{weapon.itemName}은(는) 이미 다른 캐릭터(ID: {weapon.EquippedByCharacterId})가 장착 중입니다.");
            return false;
        }

        // 현재 캐릭터가 다른 무기를 장착하고 있다면 해제
        if (character.EquippedWeapon != null)
        {
            UnequipWeapon(character);
        }

        // 무기 장착
        character.EquippedWeapon = weapon;
        weapon.EquippedByCharacterId = character.characterdata.characterID.ToString();

        Debug.Log($"{character.characterdata.characterName}이(가) {weapon.itemName}을(를) 장착했습니다.");
        OnCharacterDataUpdated?.Invoke(character); // UI 갱신 등을 위해 이벤트 호출

        return true;
    }

    /// <summary>
    /// 캐릭터의 무기 장착을 해제합니다. (스탯 검사 없음)
    /// </summary>
    public void UnequipWeapon(PlayerCharacterData character)
    {
        if (character == null || character.EquippedWeapon == null)
        {
            return;
        }

        WeaponObject weaponToUnequip = character.EquippedWeapon;

        Debug.Log($"{character.characterdata.characterName}의 {weaponToUnequip.itemName} 장착을 해제합니다.");

        weaponToUnequip.EquippedByCharacterId = null;
        character.EquippedWeapon = null;

        OnCharacterDataUpdated?.Invoke(character); // UI 갱신 등을 위해 이벤트 호출
    }

    #endregion

    /// <summary>
    /// 보유 캐릭터 중 전투력이 높은 순서대로 편성을 자동으로 구성합니다.
    /// 편성 조건: 각 포지션(전, 중, 후, 최후)에 최소 1명씩, 총 5명.
    /// </summary>
    /// <returns>자동 편성 성공 여부</returns>
    public bool AutoFormTeam()
    {
        Debug.Log("[PDM] 자동 편성 시작.");

        // 1. 초기화: 현재 편성 비우기 및 보유 캐릭터 목록 준비
        // 기존 편성을 비웁니다.
        if (formation == null)
        {
            Debug.LogError("[PDM] AutoFormTeam 오류: formation 딕셔너리가 null입니다! Awake에서 초기화되었는지 확인하세요.");
            return false;
        }
        foreach (var roleObj in System.Enum.GetValues(typeof(CrewRole)))
        {
            CrewRole role = (CrewRole)roleObj; // 명시적 캐스팅 추가
            Debug.Log($"[PDM] AutoFormTeam: 현재 처리 중인 역할: {role}");
            if (!formation.ContainsKey(role))
            {
                Debug.LogError($"[PDM] AutoFormTeam 오류: formation 딕셔너리에 {role} 키가 없습니다! Awake에서 모든 역할이 초기화되었는지 확인하세요.");
                // 이 경우는 Awake에서 초기화가 제대로 안 된 경우입니다.
                // 안전하게 빈 리스트를 추가해줍니다.
                formation.Add(role, new List<PlayerCharacterData>());
            }
            formation[role].Clear(); // <--- 이 부분에서 오류가 발생했습니다。
            Debug.Log($"[PDM] AutoFormTeam: {role} 포지션 클리어 완료.");
        }
        // 모든 보유 캐릭터를 임시 리스트에 복사합니다.
        List<PlayerCharacterData> availableCharacters = new List<PlayerCharacterData>(ownedCharacters.Values);

        // 2. 1단계: 필수 포지션 채우기 (각 역할별 1명)
        // 각 역할별로 가장 전투력이 높은 캐릭터를 찾습니다.
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            PlayerCharacterData bestCharacterForRole = availableCharacters
                .Where(c => c.characterdata.crewRole == role)
                .OrderByDescending(c => c.battlePower)
                .FirstOrDefault();

            if (bestCharacterForRole != null)
            {
                formation[role].Add(bestCharacterForRole);
                availableCharacters.Remove(bestCharacterForRole); // 사용된 캐릭터는 목록에서 제거
                Debug.Log($"[PDM] 자동 편성: {role} 포지션에 {bestCharacterForRole.characterdata.characterName} 배치.");
            }
            else
            {
                Debug.LogWarning($"[PDM] 자동 편성 실패: {role} 포지션에 배치할 캐릭터가 없습니다.");
                // 편성을 초기화하고 실패를 알립니다.
                if (formation == null) // formation 딕셔너리 null 체크 추가
                {
                    Debug.LogError("[PDM] AutoFormTeam 오류: formation 딕셔너리가 null입니다! (실패 초기화 중)");
                    OnOwnedCharactersChanged?.Invoke(); // UI 갱신
                    return false;
                }
                foreach (var roleToClearObj in System.Enum.GetValues(typeof(CrewRole)))
                {
                    CrewRole roleToClear = (CrewRole)roleToClearObj; // 명시적 캐스팅 추가
                    if (!formation.ContainsKey(roleToClear)) // 키 존재 여부 체크 추가
                    {
                        Debug.LogError($"[PDM] AutoFormTeam 오류: formation 딕셔너리에 {roleToClear} 키가 없습니다! (실패 초기화 중)");
                        formation.Add(roleToClear, new List<PlayerCharacterData>()); // 키가 없으면 추가
                    }
                    formation[roleToClear].Clear(); // 이 부분 수정
                }
                OnOwnedCharactersChanged?.Invoke(); // UI 갱신
                return false;
            }
        }

        // 3. 2단계: 5번째 캐릭터 채우기
        // 남은 캐릭터 중 전투력이 가장 높은 캐릭터를 찾습니다.
        // Captain 역할을 제외한 남은 캐릭터 중 전투력이 가장 높은 캐릭터를 찾습니다.
        PlayerCharacterData fifthCharacter = availableCharacters
            .Where(c => c.characterdata.crewRole != CrewRole.Captain) // Captain 제외 필터링 추가
            .OrderByDescending(c => c.battlePower)
            .FirstOrDefault();

        if (fifthCharacter != null)
        {
            // 5번째 캐릭터는 Captain 포지션이 아니어야 합니다. (이미 위에서 필터링했으므로 이 조건은 항상 true)
            // 그리고 해당 포지션에 아직 2명이 배치되지 않았어야 합니다.
            CrewRole fifthRole = fifthCharacter.characterdata.crewRole;
            if (formation[fifthRole].Count < 2) // Captain이 아니므로 이 조건만 확인
            {
                formation[fifthRole].Add(fifthCharacter);
                Debug.Log($"[PDM] 자동 편성: 5번째 캐릭터로 {fifthCharacter.characterdata.characterName} ({fifthRole}) 배치.");
            }
            else
            {
                // 이 경우는 Captain이 아닌데도 해당 포지션이 이미 2명으로 꽉 찬 경우입니다.
                // (예: Deckhand 2명, Sailor 1명, Cook 1명, Captain 1명인 상태에서 남은 캐릭터가 Sailor인 경우)
                Debug.LogWarning($"[PDM] 자동 편성 실패: 5번째 캐릭터({fifthCharacter.characterdata.characterName})를 배치할 적절한 포지션이 없습니다. (해당 포지션({fifthRole})이 이미 꽉 참)");
                OnOwnedCharactersChanged?.Invoke(); // UI 갱신
                return false;
            }
        }
        else
        {
            Debug.LogWarning("[PDM] 자동 편성 실패: 5번째 캐릭터로 배치할 캐릭터가 없습니다. (Captain 제외)"); // 로그 메시지 수정
            OnOwnedCharactersChanged?.Invoke(); // UI 갱신
            return false;
        }

        // 4. 마무리: 팀 전투력 재계산 및 UI 갱신
        RecalculateTeamBattlePower();
        OnOwnedCharactersChanged?.Invoke(); // UI 갱신
        Debug.Log("[PDM] 자동 편성 완료.");
        return true;
    }

    public int IsValidFormation()
    {
        // 모든 필수 포지션에 캐릭터가 배치되었는지 확인
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            if (!formation.ContainsKey(role) || formation[role].Count == 0)
            {
                Debug.LogWarning($"편성 오류: {role} 포지션에 캐릭터가 배치되지 않았습니다. (최소 1명 필요)");
                return 1; // 해당 포지션에 캐릭터가 없으면 유효하지 않음
            }
        }

        // 2. 총 편성 인원이 MAX_FORMATION_SIZE(5명)인지 확인
        int currentFormationCount = GetFormationCharacterCount();
        if (currentFormationCount != MAX_FORMATION_SIZE)
        {
            Debug.LogWarning($"편성 오류: 총 편성 인원이 {MAX_FORMATION_SIZE}명이 아닙니다. (현재 {currentFormationCount}명)");
            return 2; // 총 인원이 5명이 아니면 유효하지 않음
        }

        // 모든 필수 포지션에 캐릭터가 배치되었고 총 인원도 5명이라면 유효함
        Debug.Log("편성 유효성 검사 통과: 모든 필수 포지션에 캐릭터가 배치되었습니다. (총 인원 5명)");
        return 0;
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
