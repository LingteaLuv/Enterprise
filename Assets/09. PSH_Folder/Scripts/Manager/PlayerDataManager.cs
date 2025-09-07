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

    [Header("캐릭터 레벨업 설정")]
    public const int MAX_CHARACTER_LEVEL = 10;
    public BigInteger fixedLevelUpGoldCost = 1000;
    public BigInteger fixedLevelUpStoneCost = 20;

    public Dictionary<int, PlayerCharacterData> ownedCharacters = new Dictionary<int, PlayerCharacterData>();
    public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();
    private Dictionary<int, int> starUpgradeCosts;
    private const int STAR_UPGRADE_COST_1 = 20;
    private const int STAR_UPGRADE_COST_2 = 40;
    private const int STAR_UPGRADE_COST_3 = 120;
    private const int STAR_UPGRADE_COST_4 = 180;
    private const int FRAGMENT_GAIN_1 = 10;
    private const int FRAGMENT_GAIN_2 = 40;
    private const int FRAGMENT_GAIN_3 = 300;
    public int GachaPityCounter { get; set; }

    public event System.Action<PlayerCharacterData> OnCharacterDataUpdated;
    public event System.Action OnOwnedCharactersChanged;

    private bool isBatchUpdating = false;

    protected override void Awake()
    {
        base.Awake();

        formation = new Dictionary<CrewRole, List<PlayerCharacterData>>
        {
            { CrewRole.Deckhand, new List<PlayerCharacterData>() },
            { CrewRole.Sailor, new List<PlayerCharacterData>() },
            { CrewRole.Cook, new List<PlayerCharacterData>() },
            { CrewRole.Captain, new List<PlayerCharacterData>() }
        };

        InitializeUpgradeCosts();
    }

    private void Start()
    {
        if (ownedCharacters.Count == 0)
        {
            GrantStartingCharacters();
            // 게임 시작 시, PDM의 데이터를 직접 수정하는 자동 편성을 호출합니다.
            AutoFormTeam();
            Debug.Log("기본 캐릭터 지급 완료. 자동 편성을 시작합니다.");
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
            int fragmentsGained = FRAGMENT_GAIN_1;
            AddSoulFragments(characterdata.characterID, fragmentsGained);
            Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            OnOwnedCharactersChanged?.Invoke();
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata);
            ownedCharacters.Add(characterdata.characterID, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({newCharData.stars}성) 획득!");
            newCharData.RecaculateStats();
            OnOwnedCharactersChanged?.Invoke();
            return newCharData;
        }
    }

    public PlayerCharacterData AddCharacter(CharacterData characterdata, GachaGrade grade)
    {
        if (ownedCharacters.TryGetValue(characterdata.characterID, out PlayerCharacterData existingCharData))
        {
            int newStarLevel = (int)grade;

            if (newStarLevel > existingCharData.stars)
            {
                int oldStarLevel = existingCharData.stars;
                Debug.Log($"<color=cyan>[승급!] {existingCharData.characterdata.characterName}의 등급이 {oldStarLevel}성에서 {newStarLevel}성으로 올랐습니다!</color>");
                existingCharData.stars = newStarLevel;
                existingCharData.RecaculateStats();
                OnCharacterDataUpdated?.Invoke(existingCharData);

                int fragmentsGained = 0;
                switch (oldStarLevel)
                {
                    case 1: fragmentsGained = FRAGMENT_GAIN_1; break;
                    case 2: fragmentsGained = FRAGMENT_GAIN_2; break;
                    case 3: fragmentsGained = FRAGMENT_GAIN_3; break;
                }

                if (fragmentsGained > 0)
                {
                    AddSoulFragments(characterdata.characterID, fragmentsGained);
                    Debug.Log($"기존 등급({oldStarLevel}성)에 대한 보상으로 영혼 조각 +{fragmentsGained}개가 지급되었습니다.");
                }
            }
            else
            {
                int fragmentsGained = 0;
                switch (grade)
                {
                    case GachaGrade.OneStar: fragmentsGained = FRAGMENT_GAIN_1; break;
                    case GachaGrade.TwoStar: fragmentsGained = FRAGMENT_GAIN_2; break;
                    case GachaGrade.ThreeStar: fragmentsGained = FRAGMENT_GAIN_3; break;
                }
                AddSoulFragments(characterdata.characterID, fragmentsGained);
                Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            }

            OnOwnedCharactersChanged?.Invoke();
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = new PlayerCharacterData(characterdata, grade);
            ownedCharacters.Add(characterdata.characterID, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({newCharData.stars}성) 획득!");
            newCharData.RecaculateStats();
            OnOwnedCharactersChanged?.Invoke();
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
        playerCharData.RecaculateStats();
        OnCharacterDataUpdated?.Invoke(playerCharData);
        return true;
    }

    public bool TryGetUpgradeCost(int currentStarLevel, out int cost)
    {
        return starUpgradeCosts.TryGetValue(currentStarLevel, out cost);
    }

    public bool TryLevelUpCharacter(PlayerCharacterData character)
    {
        if (character.characterLevel >= MAX_CHARACTER_LEVEL)
        {
            Debug.LogWarning($"{character.characterdata.characterName}은(는) 이미 최대 레벨({MAX_CHARACTER_LEVEL})입니다.");
            return false;
        }

        if (!CurrencyManager.Instance.CanSpendCurrency(CurrencyType.Gold, fixedLevelUpGoldCost)
         || !CurrencyManager.Instance.CanSpendCurrency(CurrencyType.EnhancementStone, fixedLevelUpStoneCost))
        {
            Debug.LogWarning("캐릭터 레벨업 실패: 재화 부족");
            return false;
        }

        CurrencyManager.Instance.SpendCurrency(CurrencyType.Gold, fixedLevelUpGoldCost);
        CurrencyManager.Instance.SpendCurrency(CurrencyType.EnhancementStone, fixedLevelUpStoneCost);

        character.characterLevel++;
        Debug.Log($"{character.characterdata.characterName} 레벨업! (Lv.{character.characterLevel})");
        QuestSignalManager.Instance.LevelUp(ItemType.Character, 1);
        character.RecaculateStats();
        OnCharacterDataUpdated?.Invoke(character);
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
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

    public void SetFormation(Dictionary<CrewRole, List<PlayerCharacterData>> newFormation)
    {
        formation = newFormation;
        RecalculateTeamBattlePower();
        OnOwnedCharactersChanged?.Invoke();
        Debug.Log("[PDM] 새로운 편성이 적용되었습니다.");
    }

    #region 아이템 관리

    public bool EquipItem(PlayerCharacterData character, WeaponObject newItem)
    {
        if (character == null || newItem == null)
        {
            Debug.LogError("캐릭터 또는 아이템 데이터가 null입니다.");
            return false;
        }

        PlayerCharacterData oldOwner = null;
        if (!string.IsNullOrEmpty(newItem.EquippedByCharacterId))
        {
            if (int.TryParse(newItem.EquippedByCharacterId, out int ownerId) && ownedCharacters.TryGetValue(ownerId, out oldOwner))
            {
                if (oldOwner != character)
                {
                    UnequipItem(oldOwner, newItem, false);
                }
            }
        }

        EquipCategory category = newItem.equipCategory;

        if (character.equippedItems.ContainsKey(category))
        {
            UnequipItem(character, category, false);
        }

        character.equippedItems[category] = newItem;
        newItem.EquippedByCharacterId = character.characterdata.characterID.ToString();

        Debug.Log($"{character.characterdata.characterName}이(가) {newItem.itemName}을(를) {category} 슬롯에 장착했습니다.");

        character.RecaculateStats();
        OnCharacterDataUpdated?.Invoke(character);

        if (oldOwner != null && oldOwner != character)
        {
            oldOwner.RecaculateStats();
            OnCharacterDataUpdated?.Invoke(oldOwner);
        }

        return true;
    }

    public void UnequipItem(PlayerCharacterData character, EquipCategory category, bool triggerUpdate = true)
    {
        if (character == null || !character.equippedItems.ContainsKey(category))
        {
            return;
        }

        WeaponObject itemToUnequip = character.equippedItems[category];

        Debug.Log($"{character.characterdata.characterName}의 {category} 슬롯에서 {itemToUnequip.itemName} 장착을 해제합니다.");

        itemToUnequip.EquippedByCharacterId = null;
        character.equippedItems.Remove(category);

        if (triggerUpdate)
        {
            character.RecaculateStats();
            OnCharacterDataUpdated?.Invoke(character);
        }
    }

    public void UnequipItem(PlayerCharacterData character, WeaponObject itemToUnequip, bool triggerUpdate = true)
    {
        if (character == null || itemToUnequip == null)
        {
            return;
        }

        EquipCategory category = itemToUnequip.equipCategory;
        if (character.equippedItems.ContainsKey(category) && character.equippedItems[category] == itemToUnequip)
        {
            UnequipItem(character, category, triggerUpdate);
        }
    }

    public void UnequipAllItems(PlayerCharacterData character)
    {
        if (character == null)
        {
            Debug.LogWarning("장비 해제를 진행할 캐릭터가 없습니다.");
            return;
        }

        Debug.Log($"{character.characterdata.characterName}의 모든 장비 해제를 시작합니다.");

        var categories = new List<EquipCategory>(character.equippedItems.Keys);

        foreach (var category in categories)
        {
            UnequipItem(character, category);
        }

        Debug.Log($"{character.characterdata.characterName}의 모든 장비 해제가 완료되었습니다.");
    }

    #endregion

    #region 자동 장착
    public void AutoEquipBestItems(PlayerCharacterData character)
    {
        if (character == null)
        {
            Debug.LogWarning("자동 장착을 진행할 캐릭터가 선택되지 않았습니다.");
            return;
        }

        Debug.Log($"{character.characterdata.characterName}의 전체 장비 자동 장착을 시작합니다.");
        bool equippedSomething = false;

        foreach (EquipCategory category in System.Enum.GetValues(typeof(EquipCategory)))
        {
            var equippableWeapons = InventoryManager.Instance.weaponList.Where(weapon =>
                weapon.equipCategory == category && IsEquippableByCharacter(weapon, character)
            ).ToList();

            if (equippableWeapons.Count == 0) continue;

            var bestWeapon = equippableWeapons
                .OrderByDescending(w => w.ItemStar)
                .ThenByDescending(w => w.ItemLevel)
                .FirstOrDefault();

            if (bestWeapon != null)
            {
                character.equippedItems.TryGetValue(category, out var currentWeapon);
                if (currentWeapon == bestWeapon) continue;

                Debug.Log($"  - {category} 부위: {bestWeapon.itemName} 장착.");
                EquipItem(character, bestWeapon);
                equippedSomething = true;
            }
        }

        if (equippedSomething)
        {
            Debug.Log("전체 장비 자동 장착이 완료되었습니다.");
        }
        else
        {
            Debug.Log("더 좋은 장비가 없거나, 장착할 수 있는 장비가 없습니다.");
        }
    }

    public bool IsEquippableByCharacter(WeaponObject weapon, PlayerCharacterData character)
    {
        if (weapon == null || character == null) return false;

        if (weapon.equipCategory == EquipCategory.Armor || weapon.equipCategory == EquipCategory.Shield)
        {
            return true;
        }

        ItemWeaponSO weaponSO = (ItemWeaponSO)weapon.itemSO;
        EquipType type = weaponSO.equipType;
        CrewRole role = character.characterdata.crewRole;

        switch (role)
        {
            case CrewRole.Captain:
                return true;
            case CrewRole.Cook:
                return type == EquipType.Mace || type == EquipType.Staff;
            case CrewRole.Sailor:
                if (character.characterdata.atkRangeType == AtkRangeType.Ranged_Attack)
                {
                    return type == EquipType.Bow || type == EquipType.Gun;
                }
                else
                {
                    return type == EquipType.Sword || type == EquipType.Spear;
                }
            case CrewRole.Deckhand:
                return type == EquipType.Axe || type == EquipType.Hammer;
            default:
                return false;
        }
    }
    #endregion

    private void HandleCharacterBattlePowerChange(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        if (isBatchUpdating) return;

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
        RecalculateTeamBattlePower();

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

    /// <summary>
    /// [게임 시작 시 전용] 소유한 캐릭터 중 가장 강력한 캐릭터들로 기본 편성을 구성합니다.
    /// </summary>
    public bool AutoFormTeam()
    {
        if (ownedCharacters.Count == 0) return false;

        // 역할별 최대 인원수 정의 (임시, FormationManager와 동기화 필요)
        var roleLimits = new Dictionary<CrewRole, int>
        {
            { CrewRole.Captain, 1 },
            { CrewRole.Deckhand, 2 },
            { CrewRole.Sailor, 2 },
            { CrewRole.Cook, 2 }
        };

        var newFormation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
        var addedCharacters = new HashSet<PlayerCharacterData>();
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            newFormation[role] = new List<PlayerCharacterData>();
        }

        // 1. 역할별 에이스 1명씩 선발
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            var bestInRole = ownedCharacters.Values
                .Where(c => c.characterdata.crewRole == role)
                .OrderByDescending(c => c.battlePower)
                .FirstOrDefault();

            if (bestInRole != null)
            {
                newFormation[role].Add(bestInRole);
                addedCharacters.Add(bestInRole);
            }
        }

        // 2. 남은 슬롯 충원
        int currentTeamSize = addedCharacters.Count;
        if (currentTeamSize < MAX_FORMATION_SIZE)
        {
            var remainingCandidates = ownedCharacters.Values
                .Except(addedCharacters)
                .Where(c => c.characterdata.crewRole != CrewRole.Captain)
                .OrderByDescending(c => c.battlePower)
                .ToList();

            foreach (var candidate in remainingCandidates)
            {
                if (currentTeamSize >= MAX_FORMATION_SIZE) break;

                CrewRole role = candidate.characterdata.crewRole;
                if (newFormation[role].Count < roleLimits[role])
                {
                    newFormation[role].Add(candidate);
                    addedCharacters.Add(candidate);
                    currentTeamSize++;
                }
            }
        }

        // 3. 실제 편성에 적용
        formation = newFormation;
        RecalculateTeamBattlePower();
        OnOwnedCharactersChanged?.Invoke();

        Debug.Log("초기 자동 편성이 완료되었습니다.");
        return true;
    }
}