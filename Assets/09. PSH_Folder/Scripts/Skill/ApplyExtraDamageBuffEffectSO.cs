using UnityEngine;

[CreateAssetMenu(fileName = "New Apply Extra Damage Buff", menuName = "Skills/Effects/ApplyExtraDamageBuff")]
public class ApplyExtraDamageBuffEffectSO : SkillEffectSO
{
    [Tooltip("기본 공격 시 추가될 데미지 양")]
    public float damageValue = 10f;

    [Tooltip("버프가 지속될 시간(초)")]
    public float duration = 5f;

    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        var targetCombatChar = (target as Component)?.GetComponent<CombatCharacter>();
        if (targetCombatChar != null)
        {
            // CombatCharacter에 새로 만든 함수를 호출해서 버프를 적용합니다.
            targetCombatChar.ApplyOnHitDamageBuff(damageValue, duration);
        }
    }
}
