using System;
using Cysharp.Threading.Tasks;
using JHT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Codex;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    [Header("테스트용")]
    [Tooltip("체크 시, 게임 시작할 때 모든 캐릭터를 지급합니다.")]
    public bool giveAllCharactersForTest = false;

    [Header("시작 캐릭터 설정")]
    [Tooltip("게임 시작 시 기본으로 지급할 캐릭터 목록")]
    public List<CharacterData> startingCharacters = new List<CharacterData>();
   
    
    [Header("캐릭터 편성")]
    //Todo석원 : DB 연동
    public Dictionary<CrewRole, List<PlayerCharacterData>> formation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
    public const int MAX_FORMATION_SIZE = 5;
    public BigInteger teamBattlePower;

    [Header("캐릭터 레벨업 설정")]
    public const int MAX_CHARACTER_LEVEL = 10;
    public BigInteger fixedLevelUpGoldCost = 1000;
    public BigInteger fixedLevelUpStoneCost = 20;
    
    public Dictionary<int, PlayerCharacterData> OwnedCharacters = new Dictionary<int, PlayerCharacterData>();
    //public Dictionary<int, int> characterSoulFragments = new Dictionary<int, int>();

    public class ParsingPlayerData
    {
        public int Level;
        public int Soul;
        public int Star;
    }

    public class ParsingFormationData
    {
        public List<int> Formation;
    }
    
    private Dictionary<int, int> starUpgradeCosts;
    private const int STAR_UPGRADE_COST_1 = 20;
    private const int STAR_UPGRADE_COST_2 = 40;
    private const int STAR_UPGRADE_COST_3 = 120;
    private const int STAR_UPGRADE_COST_4 = 180;
    private const int FRAGMENT_GAIN_1 = 4;
    private const int FRAGMENT_GAIN_2 = 8;
    private const int FRAGMENT_GAIN_3 = 16;
    private const int CAPTAIN_FRAGMENT_GAIN_1 = 2;
    private const int CAPTAIN_FRAGMENT_GAIN_2 = 3;
    private const int CAPTAIN_FRAGMENT_GAIN_3 = 6;
    public int GachaPityCounter { get; set; }

    public event System.Action<PlayerCharacterData> OnCharacterDataUpdated;
    public event System.Action OnOwnedCharactersChanged;
    public event System.Action OnFormationSaved;

    private bool isBatchUpdating = false;// 일괄 작업할 때 키는 불값

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
        DataManager.Instance.OnWeaponReady += async () =>
        {
            await InitDatabase();
            if (OwnedCharacters.Count == 0)
            {
                if (giveAllCharactersForTest)
                {
                    await GrantAllCharactersForTest();
                }
                else
                {
                    await GrantStartingCharacters();
                }
                // 게임 시작 시, PDM의 데이터를 직접 수정하는 자동 편성을 호출합니다.
                //Debug.LogError("기본 캐릭터 지급 완료. 자동 편성을 시작합니다.");
            }
            DataManager.Instance.OnCrewReady.Invoke();
        };
        
        DataManager.Instance.OnCrewReady += () =>
        {
            DatabaseManager.Instance.LoadFormationAsync((result) =>
            {
                if (result.Count == 0)
                {
                    AutoFormTeam();
                }
                else
                {
                    foreach (object crewId in result)
                    {
                        int id = Convert.ToInt32(crewId);
                        PlayerCharacterData crew = OwnedCharacters[id];
                        formation[crew.characterdata.crewRole].Add(crew);
                        Debug.LogError($"{id}, {crew.characterdata.characterName}");
                    }
                }
                StartCoroutine(InitialCalculationCoroutine());
            });
        };
    }

    private async UniTask InitDatabase()
    {
        await DatabaseManager.Instance.LoadCharactersAsync((result) =>
        {
            foreach (var kvp in result)
            {
                int id = kvp.Key;
                ParsingPlayerData data = kvp.Value;
                PlayerCharacterData character = new PlayerCharacterData(id, data);

                OwnedCharacters[id] = character;
            }
        });
    }
    
    private async UniTask GrantStartingCharacters()
    {
        Debug.Log("기본 지급 캐릭터가 있는지 확인합니다...");
        if (startingCharacters == null || startingCharacters.Count == 0)
        {
            Debug.Log("기본으로 지급할 캐릭터가 설정되지 않았습니다.");
            return;
        }

        Debug.Log($"{startingCharacters.Count}명의 기본 캐릭터를 지급합니다.");
        var tasks = new List<UniTask>();
        foreach (CharacterData characterSO in startingCharacters)
        {
            if (characterSO != null)
            {
                tasks.Add(AddCharacter(characterSO).AsUniTask());
            }
            else
            {
                Debug.LogWarning("Starting Characters 리스트에 null인 항목이 있습니다.");
            }
        }
        await UniTask.WhenAll(tasks);
        Debug.Log("기본 캐릭터 지급이 완료되었습니다.");
    }

    private async UniTask GrantAllCharactersForTest()
    {
        Debug.Log("<color=yellow>[테스트] 모든 캐릭터를 지급합니다...</color>");
        var tasks = new List<UniTask>();

#if UNITY_EDITOR
        Debug.Log("[테스트] 에디터 모드: AssetDatabase를 사용하여 모든 캐릭터를 로드합니다.");
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterData");
        Debug.Log($"[테스트] {guids.Length}개의 CharacterData 에셋을 찾았습니다.");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            CharacterData characterSO = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (characterSO != null)
            {
                tasks.Add(AddCharacter(characterSO).AsUniTask());
            }
        }
#else
        Debug.Log("[테스트] 빌드 모드: 어드레서블을 사용하여 모든 캐릭터를 로드합니다.");
        var handle = Addressables.LoadAssetsAsync<CharacterData>("Characters");
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var allCharacters = handle.Result;
            Debug.Log($"[테스트] {allCharacters.Count}명의 캐릭터를 지급합니다.");
            foreach (var characterSO in allCharacters)
            {
                if (characterSO != null)
                {
                    tasks.Add(AddCharacter(characterSO).AsUniTask());
                }
            }
            Addressables.Release(handle);
        }
        else
        {
            Debug.LogError("[테스트] 캐릭터 에셋 로딩에 실패했습니다. 'Characters' 레이블을 확인해주세요.");
            Addressables.Release(handle);
        }
#endif

        await UniTask.WhenAll(tasks);
        Debug.Log("<color=yellow>[테스트] 모든 캐릭터 지급이 완료되었습니다.</color>");
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

    // 초기 지급 캐릭터 관련
    public async Task<PlayerCharacterData> AddCharacter(CharacterData characterdata)
    {
        if (OwnedCharacters.TryGetValue(characterdata.characterID, out PlayerCharacterData existingCharData))
        {
            int fragmentsGained = FRAGMENT_GAIN_1;
            AddSoulFragments(characterdata.characterID, fragmentsGained);
            Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            OnOwnedCharactersChanged?.Invoke();
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = await PlayerCharacterData.Instantiate(characterdata, 1);
            OwnedCharacters.Add(characterdata.characterID, newCharData);
            //await DatabaseManager.Instance.SaveCrewDataAsync($"StatusData/Crew/{characterdata.characterID}", newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({newCharData.Star}성) 획득!");
            newCharData.RecalculateStats();
            OnOwnedCharactersChanged?.Invoke();
            return newCharData;
        }
    }

    // 뽑기로 획득하는 캐릭터
    public async UniTask<PlayerCharacterData> AddCharacter(CharacterData characterdata, GachaGrade grade)
    {
        if (OwnedCharacters.TryGetValue(characterdata.characterID, out PlayerCharacterData existingCharData))
        {
            int newStarLevel = (int)grade;

            if (newStarLevel > existingCharData.Star.Value)
            {
                int oldStarLevel = existingCharData.Star.Value;
                Debug.Log($"<color=cyan>[승급!] {existingCharData.characterdata.characterName}의 등급이 {oldStarLevel}성에서 {newStarLevel}성으로 올랐습니다!</color>");
                existingCharData.Star.Value = newStarLevel;
                existingCharData.RecalculateStats();
                OnCharacterDataUpdated?.Invoke(existingCharData);

                int fragmentsGained = 0;
                switch (oldStarLevel)
                {
                    case 1: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_1 : FRAGMENT_GAIN_1; break;
                    case 2: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_2 : FRAGMENT_GAIN_2; break;
                    case 3: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_3 : FRAGMENT_GAIN_3; break;
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
                    case GachaGrade.OneStar: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_1 : FRAGMENT_GAIN_1; break;
                    case GachaGrade.TwoStar: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_2 : FRAGMENT_GAIN_2; break;
                    case GachaGrade.ThreeStar: fragmentsGained = characterdata.crewRole == CrewRole.Captain ? CAPTAIN_FRAGMENT_GAIN_3 : FRAGMENT_GAIN_3; break;
                }
                AddSoulFragments(characterdata.characterID, fragmentsGained);
                Debug.Log($"[중복] {characterdata.characterName} 획득! 영혼 조각 +{fragmentsGained}");
            }

            OnOwnedCharactersChanged?.Invoke();
            return existingCharData;
        }
        else
        {
            PlayerCharacterData newCharData = await PlayerCharacterData.Instantiate(characterdata, (int)grade);
            OwnedCharacters.Add(characterdata.characterID, newCharData);
            Debug.Log($"[신규] {characterdata.characterName}({newCharData.Star.Value}성) 획득!");
            newCharData.RecalculateStats();
            OnOwnedCharactersChanged?.Invoke();
            return newCharData;
        }
    }

    /*public async void AddSoulFragments(int characterId, int amount)
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
        await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Crew/{characterId}/SoulFragments", characterSoulFragments[characterId]);
    }*/

    private void AddSoulFragments(int characterId, int amount)
    {
        OwnedCharacters[characterId].Soul.Value += amount;

        Debug.Log($"캐릭터 ID {characterId}의 영혼 조각 +{amount}. 현재: {OwnedCharacters[characterId].Soul.Value}개");
    }

    /*public async UniTask<bool> TryUpgradeCharacterStar(PlayerCharacterData playerCharData)
    {
        if (playerCharData == null) { Debug.LogError("업그레이드할 캐릭터 데이터가 null입니다."); return false; }
        int characterId = playerCharData.characterdata.characterID;
        if (playerCharData.Star.Value >= 5) { Debug.LogWarning($"{playerCharData.characterdata.characterName}은(는) 이미 최대 성급입니다."); return false; }
        if (!starUpgradeCosts.TryGetValue(playerCharData.Star.Value, out int cost)) { Debug.LogError($"현재 성급 {playerCharData.Star.Value} 업그레이드 비용이 정의되지 않았습니다."); return false; }
        if (!characterSoulFragments.ContainsKey(characterId) || characterSoulFragments[characterId] < cost)
        {
            Debug.LogWarning($"{playerCharData.characterdata.characterName}의 영혼 조각이 부족합니다!");
            return false;
        }
        characterSoulFragments[characterId] -= cost;
        await DatabaseManager.Instance.SaveFieldAsync($"StatusData/Crew/{characterId}/SoulFragments", characterSoulFragments[characterId]);
        playerCharData.Star.Value++;
        Debug.Log($"{playerCharData.characterdata.characterName}이(가) {playerCharData.Star.Value}성으로 승급했습니다!");
        playerCharData.RecalculateStats();
        OnCharacterDataUpdated?.Invoke(playerCharData);
        return true;
    }*/

    public bool TryUpgradeCharacterStar(PlayerCharacterData playerCharData)
    {
        if (playerCharData == null) { Debug.LogError("업그레이드할 캐릭터 데이터가 null입니다."); return false; }
        int characterId = playerCharData.characterdata.characterID;
        if (playerCharData.Star.Value >= 5) { Debug.LogWarning($"{playerCharData.characterdata.characterName}은(는) 이미 최대 성급입니다."); return false; }
        if (!starUpgradeCosts.TryGetValue(playerCharData.Star.Value, out int cost)) { Debug.LogError($"현재 성급 {playerCharData.Star.Value} 업그레이드 비용이 정의되지 않았습니다."); return false; }
        if (!OwnedCharacters.ContainsKey(characterId) || OwnedCharacters[characterId].Soul.Value < cost)
        {
            Debug.LogWarning($"{playerCharData.characterdata.characterName}의 영혼 조각이 부족합니다!");
            return false;
        }

        OwnedCharacters[characterId].Soul.Value -= cost;
        playerCharData.Star.Value++;
        Debug.Log($"{playerCharData.characterdata.characterName}이(가) {playerCharData.Star.Value}성으로 승급했습니다!");
        QuestSignalManager.Instance.RankUp(ItemType.Character, 1);
        CodexSiganlManager.Instance.RaiseRankSignal(playerCharData.characterdata.faction, 1);
        playerCharData.RecalculateStats();
        OnCharacterDataUpdated?.Invoke(playerCharData);
        return true;
    }

    public bool TryGetUpgradeCost(int currentStarLevel, out int cost)
    {
        return starUpgradeCosts.TryGetValue(currentStarLevel, out cost);
    }

    public bool TryLevelUpCharacter(PlayerCharacterData character)
    {
        if (character.Level.Value >= MAX_CHARACTER_LEVEL)
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

        character.Level.Value++;
        Debug.Log($"{character.characterdata.characterName} 레벨업! (Lv.{character.Level.Value})");
        QuestSignalManager.Instance.LevelUp(ItemType.Character, 1);
        CodexSiganlManager.Instance.RaiseLevelSignal(character.characterdata.faction, 1);
        character.RecalculateStats();
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
        OnFormationSaved?.Invoke(); // 새로운 편성 저장 이벤트 발생
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
        if (!string.IsNullOrEmpty(newItem.EquippedByCharacterId.Value))
        {
            if (int.TryParse(newItem.EquippedByCharacterId.Value, out int ownerId) && OwnedCharacters.TryGetValue(ownerId, out oldOwner))
            {
                if (oldOwner != character)
                {
                    UnequipItem(oldOwner, newItem, false);
                }
            }
        }

        EquipCategory category = newItem.equipCategory;
        string itemCategory = category.ToString();
        if (character.equippedItems.ContainsKey(category))
        {
            UnequipItem(character, category, false);
        }

        character.equippedItems[category] = newItem;
        DatabaseManager.Instance.SaveField($"StatusData/Crew/{character.characterdata.characterID}/Equips/{itemCategory}", newItem.itemNum);
        newItem.EquippedByCharacterId.Value = character.characterdata.characterID.ToString();

        Debug.Log($"{character.characterdata.characterName}이(가) {newItem.itemName}을(를) {category} 슬롯에 장착했습니다.");

        if (!isBatchUpdating)
        {
            character.RecalculateStats();
            OnCharacterDataUpdated?.Invoke(character);
        }

        if (oldOwner != null && oldOwner != character)
        {
            oldOwner.RecalculateStats();
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

        itemToUnequip.EquippedByCharacterId.Value = "";
        character.equippedItems.Remove(category);
        
        string itemCategory = category.ToString();
        DatabaseManager.Instance.SaveField($"StatusData/Crew/{character.characterdata.characterID}/Equips/{itemCategory}", null);

        if (triggerUpdate)
        {
            character.RecalculateStats();
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
        isBatchUpdating = true;

        var categories = new List<EquipCategory>(character.equippedItems.Keys);

        foreach (var category in categories)
        {
            UnequipItem(character, category, false);
        }

        isBatchUpdating = false;

        character.RecalculateStats();
        OnCharacterDataUpdated?.Invoke(character);

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
        isBatchUpdating = true;

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

        isBatchUpdating = false;

        if (equippedSomething)
        {
            Debug.Log("전체 장비 자동 장착이 완료되었습니다.");
            character.RecalculateStats();
            OnCharacterDataUpdated?.Invoke(character);
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
        foreach (var character in OwnedCharacters.Values)
        {
            character.RecalculateStats();
        }
    }

    /// <summary>
    /// [게임 시작 시 전용] 소유한 캐릭터 중 가장 강력한 캐릭터들로 기본 편성을 구성합니다.
    /// </summary>
    public bool AutoFormTeam()
    {
        if (OwnedCharacters.Count == 0) return false;

        // 역할별 빈 리스트 생성
        var newFormation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            newFormation[role] = new List<PlayerCharacterData>();
        }

        // 조건 무시 — 그냥 다 넣기
        foreach (var character in OwnedCharacters.Values)
        {
            var role = character.characterdata.crewRole;
            newFormation[role].Add(character);
        }

        // 3. 실제 편성에 적용
        formation = newFormation;
        RecalculateTeamBattlePower();
        OnOwnedCharactersChanged?.Invoke();

        Debug.Log("초기 자동 편성이 완료되었습니다.");
        return true;
    }
}