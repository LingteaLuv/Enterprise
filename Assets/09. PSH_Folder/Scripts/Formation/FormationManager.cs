using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 캐릭터 편성의 임시 상태 관리, 유효성 검사, 저장을 담당하는 매니저.
/// </summary>
public class FormationManager : Singleton<FormationManager>
{
    // 임시로 편집 중인 편성 정보
    private Dictionary<CrewRole, List<PlayerCharacterData>> tempFormation;
    public IReadOnlyDictionary<CrewRole, List<PlayerCharacterData>> TempFormation => tempFormation;

    // 원본 편성 정보 (되돌리기를 위해 저장)
    private Dictionary<CrewRole, List<PlayerCharacterData>> originalFormation;

    // 역할별 최대 인원수 정의
    private readonly Dictionary<CrewRole, int> roleLimits = new Dictionary<CrewRole, int>
    {
        { CrewRole.Captain, 1 },
        { CrewRole.Deckhand, 2 },
        { CrewRole.Sailor, 2 },
        { CrewRole.Cook, 2 }
    };

    public bool HasUnsavedChanges { get; private set; }
    public event System.Action OnTempFormationChanged;

    protected override void Awake()
    {
        base.Awake();
        tempFormation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
        originalFormation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
    }

    public void InitializeTempFormation()
    {
        originalFormation = DeepCopyFormation(PlayerDataManager.Instance.formation);
        tempFormation = DeepCopyFormation(PlayerDataManager.Instance.formation);
        HasUnsavedChanges = false;
        Debug.Log("임시 편성 초기화 완료.");
    }

    /// <summary>
    /// 캐릭터를 임시 편성에 추가/제거합니다. 추가 시 유효성 검사를 먼저 수행합니다.
    /// </summary>
    public void ToggleCharacterInTempFormation(PlayerCharacterData character)
    {
        CrewRole role = character.characterdata.crewRole;
        if (!tempFormation.ContainsKey(role)) tempFormation[role] = new List<PlayerCharacterData>();

        if (tempFormation[role].Contains(character))
        {
            // 이미 있으면 제거
            tempFormation[role].Remove(character);
            Debug.Log($"{character.characterdata.characterName}을(를) 임시 편성에서 제거.");
        }
        else
        {
            // 없으면 추가하기 전에, 규칙을 먼저 확인합니다.
            // 규칙 1: 최대 편성 인원 초과 불가
            int totalCharacters = tempFormation.Values.Sum(list => list.Count);
            if (totalCharacters >= PlayerDataManager.MAX_FORMATION_SIZE)
            {
                PopManager.Instance.ShowOKPopup($"편성 인원은 최대 {PlayerDataManager.MAX_FORMATION_SIZE}명을 초과할 수 없습니다.");
                return;
            }

            // 규칙 2: 역할별 최대 인원 초과 불가
            if (tempFormation[role].Count >= roleLimits[role])
            {
                PopManager.Instance.ShowOKPopup($"{role} 역할은 최대 {roleLimits[role]}명까지만 배치할 수 있습니다.");
                return;
            }



            // 모든 검사를 통과하면 추가
            tempFormation[role].Add(character);
            Debug.Log($"{character.characterdata.characterName}을(를) 임시 편성에 추가.");
        }

        CheckForUnsavedChanges();
        OnTempFormationChanged?.Invoke();
    }

    public bool IsInTempFormation(PlayerCharacterData character)
    {
        return tempFormation.Values.Any(list => list.Contains(character));
    }

    /// <summary>
    /// 임시 편성의 유효성을 검사합니다. (저장 또는 탭 이동 시 사용)
    /// </summary>
    public bool ValidateFormation(out string errorMessage)
    {
        // 규칙 1: 선장은 반드시 1명이어야 합니다.
        if (!tempFormation.ContainsKey(CrewRole.Captain) || tempFormation[CrewRole.Captain].Count != 1)
        {
            errorMessage = "선장은 반드시 1명을 편성해야 합니다!";
            return false;
        }

        // 규칙 2: 역할별 최대 인원 수를 초과할 수 없습니다.
        foreach (var roleLimit in roleLimits)
        {
            if (tempFormation.ContainsKey(roleLimit.Key) && tempFormation[roleLimit.Key].Count > roleLimit.Value)
            {
                errorMessage = $"{roleLimit.Key} 역할은 최대 {roleLimit.Value}명까지만 배치할 수 있습니다.";
                return false;
            }
        }

        // 규칙 3: 총 편성 인원은 반드시 5명이어야 합니다.
        int totalCharacters = tempFormation.Values.Sum(list => list.Count);
        if (totalCharacters != PlayerDataManager.MAX_FORMATION_SIZE)
        {
            errorMessage = $"편성 인원은 반드시 {PlayerDataManager.MAX_FORMATION_SIZE}명이어야 합니다!";
            return false;
        }

        // 규칙 4: 총 인원이 5명일 때, 모든 역할에 최소 1명씩 배치되어야 합니다.
        // 이 규칙은 총 인원이 5명일 때만 적용됩니다.
        if (totalCharacters == PlayerDataManager.MAX_FORMATION_SIZE)
        {
            foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
            {
                // Captain 역할은 이미 규칙 1에서 1명인지 확인했으므로, 여기서는 0명인 경우만 체크합니다.
                // 다른 역할들은 roleLimits에 따라 0명일 수 있으므로, 해당 역할에 캐릭터가 없는 경우를 확인합니다.
                if (!tempFormation.ContainsKey(role) || tempFormation[role].Count == 0)
                {
                    errorMessage = "모든 역할에 최소 1명의 캐릭터를 배치해야 합니다.";
                    return false;
                }
            }
        }

        errorMessage = "";
        return true;
    }

    /// <summary>
    /// 1. 역할별 최강자 1명씩 우선 선택
    /// 2. 남은 슬롯은 선장을 제외한 나머지 중 전투력 순으로 충원
    /// 하는 방식으로 자동으로 임시 편성을 구성합니다.
    /// </summary>
    public void AutoFormTeam()
    {
        Debug.Log("새로운 자동 편성 로직을 시작합니다...");
        var allCharacters = PlayerDataManager.Instance.ownedCharacters.Values.ToList();
        if (allCharacters.Count == 0) return;

        // 1. 새 편성 정보와 이미 선택된 캐릭터를 추적할 Set을 준비합니다.
        var newFormation = new Dictionary<CrewRole, List<PlayerCharacterData>>();
        var addedCharacters = new HashSet<PlayerCharacterData>();
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            newFormation[role] = new List<PlayerCharacterData>();
        }

        // 2. 역할별 에이스(전투력 1등)를 1명씩 먼저 뽑습니다.
        foreach (CrewRole role in System.Enum.GetValues(typeof(CrewRole)))
        {
            var bestInRole = allCharacters
                .Where(c => c.characterdata.crewRole == role)
                .OrderByDescending(c => c.battlePower)
                .FirstOrDefault();

            if (bestInRole != null)
            {
                newFormation[role].Add(bestInRole);
                addedCharacters.Add(bestInRole);
            }
        }

        // 3. 남은 인원들 중, 선장을 제외하고 전투력이 높은 순으로 5명이 될 때까지 충원합니다.
        int currentTeamSize = addedCharacters.Count;
        if (currentTeamSize < PlayerDataManager.MAX_FORMATION_SIZE)
        {
            var remainingCandidates = allCharacters
                .Except(addedCharacters) // 이미 뽑힌 에이스들은 제외
                .Where(c => c.characterdata.crewRole != CrewRole.Captain) // 선장 역할 제외
                .OrderByDescending(c => c.battlePower)
                .ToList();

            foreach (var candidate in remainingCandidates)
            {
                if (currentTeamSize >= PlayerDataManager.MAX_FORMATION_SIZE) break;

                CrewRole role = candidate.characterdata.crewRole;
                if (newFormation[role].Count < roleLimits[role])
                {
                    newFormation[role].Add(candidate);
                    addedCharacters.Add(candidate);
                    currentTeamSize++;
                }
            }
        }

        // 4. 완성된 편성을 임시 편성으로 설정하고 변경사항을 알립니다.
        tempFormation = newFormation;
        CheckForUnsavedChanges();
        OnTempFormationChanged?.Invoke();
        Debug.Log("새로운 자동 편성이 완료되었습니다.");
    }

    public bool SaveChanges(out string errorMessage)
    {
        if (!HasUnsavedChanges)
        {
            errorMessage = "변경사항이 없습니다.";
            return true;
        }

        if (!ValidateFormation(out errorMessage))
        {
            return false;
        }
        PlayerDataManager.Instance.SetFormation(DeepCopyFormation(tempFormation));
        InitializeTempFormation();
        return true;
    }

    public void RevertFormationChanges()
    {
        tempFormation = DeepCopyFormation(originalFormation);
        HasUnsavedChanges = false;
        OnTempFormationChanged?.Invoke();
    }

    private void CheckForUnsavedChanges()
    {
        HasUnsavedChanges = !AreFormationsEqual(originalFormation, tempFormation);
    }

    private bool AreFormationsEqual(Dictionary<CrewRole, List<PlayerCharacterData>> f1, Dictionary<CrewRole, List<PlayerCharacterData>> f2)
    {
        if (f1.Keys.Count != f2.Keys.Count) return false;
        foreach (var key in f1.Keys)
        {
            if (!f2.ContainsKey(key)) return false;
            var list1 = f1[key].OrderBy(c => c.characterdata.characterID).ToList();
            var list2 = f2[key].OrderBy(c => c.characterdata.characterID).ToList();
            if (!list1.SequenceEqual(list2)) return false;
        }
        return true;
    }

    private Dictionary<CrewRole, List<PlayerCharacterData>> DeepCopyFormation(Dictionary<CrewRole, List<PlayerCharacterData>> formationToCopy)
    {
        return formationToCopy.ToDictionary(
            entry => entry.Key,
            entry => new List<PlayerCharacterData>(entry.Value)
        );
    }
}
