
using UnityEngine;

/// <summary>
/// 모든 스킬 효과의 기반이 되는 추상 ScriptableObject입니다.
/// 이 클래스를 상속받아 다양한 스킬 효과를 구현합니다.
/// </summary>
public abstract class SkillEffectSO : ScriptableObject
{
    /// <summary>
    /// 스킬 효과를 대상에게 적용합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 캐릭터</param>
    /// <param name="target">스킬의 대상이 되는 캐릭터</param>
    public abstract void ApplyEffect(CombatCharacter caster, CombatCharacter target);
}

// ---------------------------------------------------- //

/// <summary>
/// 특정 능력치를 일정 시간 동안 증가시키는 버프 효과입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Buff Stat Effect", menuName = "Skills/Effects/Buff Stat Effect")]
public class BuffStatEffectSO : SkillEffectSO
{
    [Header("버프 설정")]
    [SerializeField] private Stat statToBuff; // 버프할 능력치
    [SerializeField] private float buffValue;   // 버프 수치
    [SerializeField] private float duration;    // 버프 지속시간

    public override void ApplyEffect(CombatCharacter caster, CombatCharacter target)
    {
        // 대상 캐릭터의 버프 처리 시스템에 버프를 전달합니다.
        // (CombatCharacter에 이 로직을 추가해야 합니다)
        target.ApplyBuff(statToBuff, buffValue, duration);
    }
}
