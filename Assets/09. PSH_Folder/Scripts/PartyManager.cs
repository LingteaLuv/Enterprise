using UnityEngine;
using System.Collections.Generic;
using System;

public class PartyManager : Singleton<PartyManager>
{
    [Header("캐릭터 생성 설정")]
    [SerializeField] private GameObject characterPrefab; // 캐릭터로 사용할 프리팹

    [Header("전투시만 필요한 스탯")]
    public float moveSpeed;
    [Tooltip("원거리")]
    public float attackRange;
    [Tooltip("근거리")]
    public float attackRange2;

    // 생성된 파티원들을 관리하는 리스트
    private readonly List<CombatCharacter> activeParty = new List<CombatCharacter>();
    public List<CombatCharacter> ActiveParty => activeParty; // 외부에서 읽을 수 있도록 공개

    /// <summary> 파티 설정이 완료되었는지 나타내는 플래그입니다. </summary>
    public bool IsPartyReady { get; private set; } = false;

    /// <summary> 파티 설정이 완료되었을 때 호출되는 이벤트입니다. </summary>
    public static event Action<List<CombatCharacter>> OnPartyReady;

    public List<CombatCharacter> GetAllPartyMembers()
    {
        return activeParty;
    }

    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnFormationSaved += SetupBattleParty;
        }
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnFormationSaved -= SetupBattleParty;
        }
    }

    public void SetupBattleParty()
    {
        // 씬 안에서 "CharacterParent"라는 태그를 가진 오브젝트를 찾습니다.
        Transform characterParent = GameObject.FindWithTag("CharacterParent")?.transform;
        if (characterParent == null)
        {
            // 태그를 가진 오브젝트가 없으면, 전투 씬이 아니라고 판단하고 함수를 그냥 종료합니다.
            Debug.Log("이곳은 전투 씬이 아니거나 'CharacterParent' 태그가 설정되지 않았으므로, 캐릭터를 생성하지 않습니다.");
            return;
        }

        IsPartyReady = false; // 파티 설정을 다시 시작하므로 플래그를 내립니다.

        // 1. 기존에 생성된 캐릭터가 있다면 모두 파괴
        foreach (CombatCharacter oldChar in activeParty)
        {
            if (oldChar != null) Destroy(oldChar.gameObject);
        }
        activeParty.Clear();

        // 2. PlayerDataManager에서 현재 편성된 파티원 리스트를 가져옵니다.
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager가 씬에 없습니다!");
            return;
        }
        List<PlayerCharacterData> currentPartyData = new List<PlayerCharacterData>();
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            if (PlayerDataManager.Instance.formation.TryGetValue(role, out List<PlayerCharacterData> charactersInRole))
            {
                currentPartyData.AddRange(charactersInRole);
            }
        }

        // 3. 편성 정보에 따라 프리팹으로부터 캐릭터를 생성합니다.
        if (characterPrefab == null)
        {
            Debug.LogError("Character Prefab이 PartyManager에 할당되지 않았습니다!");
            return;
        }

        foreach (PlayerCharacterData data in currentPartyData)
        {
            GameObject charObject = Instantiate(characterPrefab, characterParent);

            // 데이터에 연결된 characterPrefab(모델)이 있는지 확인합니다.
            var modelPrefab = data.characterdata.characterPrefab;
            if (modelPrefab != null)
            {
                // 모델 프리팹을 방금 생성한 charObject의 자식으로 생성합니다.
                Instantiate(modelPrefab, charObject.transform);
                charObject.name = $"{data.characterdata.characterName}";
            }

            CombatCharacter combatChar = charObject.GetComponent<CombatCharacter>();
            if (combatChar != null)
            {
                combatChar.Initialize(data);
                activeParty.Add(combatChar);
            }
        }

        // 4. 모든 설정이 끝난 후, 파티가 준비되었다고 알립니다.
        OnPartyReady?.Invoke(activeParty);
        IsPartyReady = true;
        Debug.Log("[PartyManager] 파티 생성 완료! OnPartyReady 신호를 보냅니다.");
    }
}