
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
    /// <param name="caster">스킬을 시전한 주체</param>
    /// <param name="target">스킬의 대상이 되는 주체</param>
    public abstract void ApplyEffect(IAttacker caster, IDamageable target);
}

