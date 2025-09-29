using UnityEngine;
using System.Collections.Generic;

// SkillSO가 다른 네임스페이스에 있다면 using을 추가해주세요!
// using MyGame.Skills;

/// <summary>
/// 개별 시너지의 발동 조건과 효과를 정의하는 Scriptable Object입니다.
/// </summary>
public class SynergySO : ScriptableObject
{
    [Header("시너지 정보")]
    public int synergyID;
    public string synergyName; // 시너지 이름

    [Header("발동 조건")]
    [Tooltip("이 리스트에 있는 모든 캐릭터가 파티에 포함되어야 시너지가 발동돼요.")]
    public List<int> requiredCharacterIDs; // 필요한 캐릭터 ID 리스트

    [Header("시너지 효과")]
    [Tooltip("조건이 만족되었을 때 적용할 버프(스킬 SO)를 연결해주세요.")]
    public SkillSO buffToApply; // 적용할 버프 (SkillSO)
}
