using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 이 클래스는 싱글톤으로 구현되었으며, 게임 내 시너지 효과를 총괄 관리합니다.
// 1. 모든 SynergySO를 로드하여 '시너지 지도'를 구축합니다. (어떤 캐릭터가 어떤 시너지에 속하는지)
// 2. 파티 구성이 변경될 때마다 효율적으로 시너지를 확인하고 적용/해제합니다.
public class SynergyManager : Singleton<SynergyManager>
{
    // --- Private Fields --- //
    private List<SynergySO> _allSynergies; // 로드된 모든 시너지 SO
    private Dictionary<int, List<SynergySO>> _characterToSynergiesMap; // 캐릭터 ID를 Key로, 해당 캐릭터가 포함된 시너지 리스트를 Value로 갖는 맵
    private List<SynergySO> _activeSynergies; // 현재 활성화된 시너지 리스트

    protected override void Awake()
    {
        base.Awake();

        // --- 초기화 --- //
        _allSynergies = new List<SynergySO>();
        _characterToSynergiesMap = new Dictionary<int, List<SynergySO>>();
        _activeSynergies = new List<SynergySO>();

        LoadAllSynergies();
        BuildSynergyMap();
    }

    /// <summary>
    /// Resources/Synergies 폴더에 있는 모든 SynergySO 에셋을 로드합니다.
    /// (어드레서블을 사용한다면 이 부분을 수정해주세요)
    /// </summary>
    private void LoadAllSynergies()
    {
        _allSynergies = Resources.LoadAll<SynergySO>("Synergies").ToList();
        Debug.Log($"[SynergyManager] {_allSynergies.Count}개의 시너지 정보를 로드했습니다.");
    }

    /// <summary>
    /// 로드된 시너지 정보를 바탕으로 캐릭터 ID와 시너지 리스트를 매핑하는 '시너지 지도'를 구축합니다.
    /// </summary>
    private void BuildSynergyMap()
    {
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

    /// <summary>
    /// 현재 파티 구성을 바탕으로 활성화될 시너지를 검사하고 업데이트합니다.
    /// 이 함수는 파티 멤버가 추가되거나 제거될 때마다 호출되어야 합니다.
    /// </summary>
    /// <param name="currentPartyIDs">현재 파티에 속한 캐릭터들의 ID 리스트</param>
    public void UpdateActiveSynergies(List<int> currentPartyIDs)
    {
        HashSet<int> partyIdSet = new HashSet<int>(currentPartyIDs); // 빠른 조회를 위해 HashSet 사용

        // 1. 비활성화 검사: 기존에 활성화됐지만, 더 이상 조건이 맞지 않는 시너지들을 비활성화
        List<SynergySO> synergiesToDeactivate = new List<SynergySO>();
        foreach (var activeSynergy in _activeSynergies)
        {
            bool stillActive = true;
            foreach (var requiredId in activeSynergy.requiredCharacterIDs)
            {
                if (!partyIdSet.Contains(requiredId))
                {
                    stillActive = false;
                    break;
                }
            }

            if (!stillActive)
            {
                synergiesToDeactivate.Add(activeSynergy);
            }
        }

        foreach (var synergy in synergiesToDeactivate)
        {
            DeactivateSynergy(synergy);
        }

        // 2. 활성화 검사: 현재 파티 구성으로 새롭게 활성화될 수 있는 시너지들을 찾아서 활성화
        HashSet<SynergySO> potentialSynergies = new HashSet<SynergySO>();
        foreach (var partyMemberId in currentPartyIDs)
        {
            if (_characterToSynergiesMap.ContainsKey(partyMemberId))
            {
                potentialSynergies.UnionWith(_characterToSynergiesMap[partyMemberId]);
            }
        }

        foreach (var potentialSynergy in potentialSynergies)
        {
            if (_activeSynergies.Contains(potentialSynergy)) continue; // 이미 활성화된 시너지는 건너뛰기

            bool allRequirementsMet = true;
            foreach (var requiredId in potentialSynergy.requiredCharacterIDs)
            {
                if (!partyIdSet.Contains(requiredId))
                {
                    allRequirementsMet = false;
                    break;
                }
            }

            if (allRequirementsMet)
            {
                ActivateSynergy(potentialSynergy);
            }
        }
    }

    private void ActivateSynergy(SynergySO synergy)
    {
        _activeSynergies.Add(synergy);
        Debug.Log($"<color=cyan>시너지 활성화: {synergy.synergyName}</color>");

        // TODO: 여기에 실제 버프를 적용하는 코드를 작성하세요.
        // 예: PartyManager.Instance.ApplyBuffToAll(synergy.buffToApply);
    }

    private void DeactivateSynergy(SynergySO synergy)
    {
        _activeSynergies.Remove(synergy);
        Debug.Log($"<color=yellow>시너지 비활성화: {synergy.synergyName}</color>");

        // TODO: 여기에 적용됐던 버프를 해제하는 코드를 작성하세요.
        // 예: PartyManager.Instance.RemoveBuffFromAll(synergy.buffToApply);
    }
}
