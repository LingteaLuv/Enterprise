using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

// 이 클래스는 싱글톤으로 구현되었으며, 게임 내 시너지 효과를 총괄 관리합니다.
// 1. 모든 SynergySO를 로드하여 '시너지 지도'를 구축합니다. (어떤 캐릭터가 어떤 시너지에 속하는지)
// 2. 파티 구성이 변경될 때마다 효율적으로 시너지를 확인하고 적용/해제합니다.
// 3. [수정] '전투용 시너지'와 '미리보기용 시너지'를 분리하여 관리합니다.
public class SynergyManager : Singleton<SynergyManager>
{
    public static bool IsApplyingSynergy { get; private set; } = false;

    // --- Public Properties --- //
    /// <summary>
    /// UI에서 표시하기 위한 '미리보기용' 시너지 리스트입니다.
    /// </summary>
    public IReadOnlyList<SynergySO> PreviewSynergies => _previewSynergies;

    // --- Private Fields --- //
    private List<SynergySO> _allSynergies; // 로드된 모든 시너지 SO
    private Dictionary<int, List<SynergySO>> _characterToSynergiesMap; // 캐릭터 ID를 Key로, 해당 캐릭터가 포함된 시너지 리스트를 Value로 갖는 맵

    [Header("Synergy Lists")]
    [SerializeField] private List<SynergySO> _combatSynergies; // '실제 전투'에 적용되는 시너지 리스트
    [SerializeField] private List<SynergySO> _previewSynergies; // '편성 화면'에서 보여주기 위한 임시 시너지 리스트

    private bool _isInitialized = false;
    private const string SYNERGY_SO_LABEL = "SynergyData"; // 어드레서블 라벨

    protected override void Awake()
    {
        base.Awake();

        // --- 초기화 --- //
        _allSynergies = new List<SynergySO>();
        _characterToSynergiesMap = new Dictionary<int, List<SynergySO>>();
        _combatSynergies = new List<SynergySO>();
        _previewSynergies = new List<SynergySO>();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await LoadAllSynergiesAsync();
        BuildSynergyMap();
        _isInitialized = true;
        Debug.Log("[SynergyManager] 초기화 완료!");
    }

    private void OnEnable()
    {
        // FormationManager의 임시 편성 변경 이벤트 구독
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged += HandleTempFormationChange;
        }
    }

    private void OnDisable()
    {
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged -= HandleTempFormationChange;
        }
    }

    /// <summary>
    /// [수정] 임시 편성 변경 시, '미리보기' 시너지만 업데이트합니다.
    /// </summary>
    private void HandleTempFormationChange()
    {
        if (!_isInitialized) return;
        Debug.Log("[SynergyManager] 임시 편성 변경 감지. '미리보기' 시너지를 업데이트합니다.");
        var tempFormation = FormationManager.Instance.TempFormation;

        List<int> currentPartyIDs = tempFormation.Values
                                          .SelectMany(list => list)
                                          .Select(character => character.characterdata.characterID)
                                          .ToList();

        UpdatePreviewSynergies(currentPartyIDs);
    }

    /// <summary>
    /// [신규] 파티 편성이 '확정'되었을 때 호출됩니다.
    /// '전투용' 시너지를 최종적으로 갱신합니다.
    /// </summary>
    /// <param name="finalPartyIDs">최종 확정된 파티의 캐릭터 ID 리스트</param>
    public void ConfirmCombatSynergies(List<int> finalPartyIDs)
    {
        if (!_isInitialized) return;
        Debug.Log("[SynergyManager] 파티 편성 확정! '전투용' 시너지를 업데이트합니다.");
        UpdateCombatSynergies(finalPartyIDs);
    }

    /// <summary>
    /// 전투 시작 시, 활성화된 '전투용' 시너지 효과를 적용합니다.
    /// </summary>
    public void ApplySynergyEffectsForBattle()
    {
        if (!_isInitialized) return;

        var party = PartyManager.Instance?.ActiveParty;
        if (party == null || party.Count == 0)
        {
            Debug.Log("[SynergyManager] 파티가 구성되지 않아 시너지 적용을 건너뜁니다.");
            return;
        }

        // 1. 새로운 버프를 적용하기 전, 모든 파티원의 기존 시너지 버프를 전부 제거합니다.
        Debug.Log("[SynergyManager] 기존 시너지 버프를 제거합니다...");
        foreach (var member in party)
        {
            member.RemoveAllSynergyBuffs();
        }

        // [수정] _combatSynergies 리스트를 사용합니다.
        if (_combatSynergies == null || _combatSynergies.Count == 0)
        {
            Debug.Log("[SynergyManager] 활성화된 전투 시너지가 없어 신규 효과 적용을 건너뜁니다.");
            return;
        }

        // 2. 새로운 시너지 버프를 적용합니다.
        Debug.Log($"[SynergyManager] {_combatSynergies.Count}개의 활성화된 전투 시너지 효과를 새로 적용합니다.");
        try
        {
            IsApplyingSynergy = true;

            foreach (var synergy in _combatSynergies)
            {
                if (synergy.buffToApply == null) continue;

                var skill = synergy.buffToApply;
                Debug.Log($"- 시너지 '{synergy.synergyName}'의 효과 '{skill.skillName}'을(를) 적용합니다.");

                if (skill.targetLogic == ETargetLogic.Self)
                {
                    var requiredIDs = new HashSet<int>(synergy.requiredCharacterIDs);
                    var synergyMembers = party.Where(c => requiredIDs.Contains(c.CharacterStats.characterdata.characterID));
                    foreach (var member in synergyMembers)
                    {
                        skill.Use(member, member);
                    }
                }
                else
                {
                    CombatCharacter caster = party.FirstOrDefault(c => c.CharacterStats.characterdata.crewRole == CrewRole.Captain) ?? party.FirstOrDefault();
                    if (caster != null)
                    {
                        skill.Use(caster);
                    }
                }
            }
        }
        finally
        {
            IsApplyingSynergy = false;
        }
    }

    #region Synergy Calculation Logic

    /// <summary>
    /// [신규] '미리보기' 시너지 리스트를 업데이트합니다.
    /// </summary>
    private void UpdatePreviewSynergies(List<int> currentPartyIDs)
    {
        CalculateSynergies(currentPartyIDs, _previewSynergies, "미리보기");
    }

    /// <summary>
    /// [신규] '전투용' 시너지 리스트를 업데이트합니다.
    /// </summary>
    private void UpdateCombatSynergies(List<int> currentPartyIDs)
    {
        CalculateSynergies(currentPartyIDs, _combatSynergies, "전투");
    }

    /// <summary>
    /// [신규] 시너지 계산을 위한 공통 로직입니다.
    /// </summary>
    /// <param name="partyIDs">계산할 파티의 캐릭터 ID 리스트</param>
    /// <param name="targetSynergyList">결과를 저장할 시너지 리스트 (e.g., _combatSynergies)</param>
    /// <param name="logPrefix">로그에 표시될 접두사 (e.g., "전투")</param>
    private void CalculateSynergies(List<int> partyIDs, List<SynergySO> targetSynergyList, string logPrefix)
    {
        HashSet<int> partyIdSet = new HashSet<int>(partyIDs);

        // 1. 비활성화 검사
        List<SynergySO> synergiesToDeactivate = new List<SynergySO>();
        foreach (var activeSynergy in targetSynergyList)
        {
            if (!activeSynergy.requiredCharacterIDs.All(id => partyIdSet.Contains(id)))
            {
                synergiesToDeactivate.Add(activeSynergy);
            }
        }
        synergiesToDeactivate.ForEach(s => {
            targetSynergyList.Remove(s);
            Debug.Log($"<color=yellow>[{logPrefix}] 시너지 비활성화: {s.synergyName}</color>");
        });

        // 2. 활성화 검사
        HashSet<SynergySO> potentialSynergies = new HashSet<SynergySO>();
        foreach (var partyMemberId in partyIDs)
        {
            if (_characterToSynergiesMap.TryGetValue(partyMemberId, out var synergies))
            {
                potentialSynergies.UnionWith(synergies);
            }
        }

        foreach (var potential in potentialSynergies)
        {
            if (targetSynergyList.Contains(potential)) continue;

            if (potential.requiredCharacterIDs.All(id => partyIdSet.Contains(id)))
            {
                targetSynergyList.Add(potential);
                Debug.Log($"<color=cyan>[{logPrefix}] 시너지 활성화: {potential.synergyName}</color>");
            }
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadAllSynergiesAsync()
    {
        var handle = await Addressables.LoadAssetsAsync<SynergySO>(SYNERGY_SO_LABEL, null).Task;
        _allSynergies = handle.ToList();
        Debug.Log($"[SynergyManager] {_allSynergies.Count}개의 시너지 정보를 로드했어요!");
    }

    private void BuildSynergyMap()
    {
        _characterToSynergiesMap.Clear();
        foreach (var synergy in _allSynergies)
        {
            foreach (var charId in synergy.requiredCharacterIDs)
            {
                if (!_characterToSynergiesMap.ContainsKey(charId))
                {
                    _characterToSynergiesMap[charId] = new List<SynergySO>();
                }
                _characterToSynergiesMap[charId].Add(synergy);
            }
        }
    }

    #endregion

    #region Synergy Check (CSJ Added)

    public bool IsSynergyActive()
    {
        return _combatSynergies != null && _combatSynergies.Count != 0;
    }

    #endregion
}