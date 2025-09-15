using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "Skills/Effects/Damage")]
public class DamageEffectSO : SkillEffectSO
{
    [Header("데미지 설정")]
    public float damageMultiplier = 1f; // 공격력 계수

    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        // IDamageable 인터페이스의 TakeDamage 메서드를 직접 호출합니다.
        target.TakeDamage(caster);
        
        CombatCharacter targetCharacter = target as CombatCharacter;
        if (targetCharacter != null)
        {
            Debug.Log($"'{targetCharacter.name}'에게 스킬로 데미지를 입혔습니다!");
        }
    }
}
