using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PartyManager : Singleton<PartyManager>
{
    [Header("미리 배치된 캐릭터 슬롯")]
    [Tooltip("씬에 미리 배치한 5개의 캐릭터 오브젝트를 순서대로 할당하세요.")]
    public List<CombatCharacter> characterSlots;
    public List<CombatCharacter> GetAllPartyMembers()
    {
        return characterSlots.Where(c => c != null).ToList(); // SetActive 여부 상관없이 전부 줌
    }
    [Header("전투시만 필요한 스탯")]
    [Tooltip("굳이 PlayerCharacterData에서 가져올 필요 없는 고정된 스탯인 이동 속도와 사거리는 여기서 담당")]
    public float moveSpeed;
    public float attackRange;
    public float attackRange2;

    private void Start()
    {
        SetupBattleParty();
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

    /// <summary>
    /// PlayerDataManager의 편성 정보를 가져와 씬에 있는 캐릭터들에게 적용합니다.
    /// </summary>
    public void SetupBattleParty()
    {
        // 1. PlayerDataManager에서 현재 편성된 파티원 리스트를 가져옵니다.
        //    (순서를 보장하기 위해 CrewRole Enum 순서대로 리스트를 만듭니다)
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager가 씬에 없습니다!");
            return;
        }
        List<PlayerCharacterData> currentParty = new List<PlayerCharacterData>();

        // CrewRole Enum에 정의된 순서대로 딕셔너리에 접근합니다.
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            // 해당 역할에 편성된 캐릭터 리스트가 있다면
            if (PlayerDataManager.Instance.formation.TryGetValue(role, out List<PlayerCharacterData> charactersInRole))
            {
                // 리스트에 추가합니다.
                currentParty.AddRange(charactersInRole);
            }
        }

        Debug.Log($"현재 편성된 파티원 수: {currentParty.Count}명 (순서 보장됨)");

        // 2. 씬에 배치된 캐릭터 슬롯(CombatCharacter)들에 파티원 데이터를 주입합니다.
        for (int i = 0; i < characterSlots.Count; i++)
        {
            // 할당할 파티원이 리스트에 남아있다면
            if (i < currentParty.Count)
            {
                // 해당 슬롯을 활성화하고, 파티원의 데이터로 초기화합니다.
                characterSlots[i].gameObject.SetActive(true);
                characterSlots[i].Initialize(currentParty[i]);
                Debug.Log($"{i}번 슬롯에 '{currentParty[i].characterdata.characterName}' 캐릭터 데이터 적용 완료.");
            }
            else
            {
                // 할당할 파티원이 없다면, 남는 슬롯은 비활성화합니다.
                characterSlots[i].gameObject.SetActive(false);
                Debug.Log($"{i}번 슬롯은 사용하지 않으므로 비활성화합니다.");
            }
        }
    }
}
