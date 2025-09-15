using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Skills/Effects/Heal")]
public class HealEffectSO : SkillEffectSO
{
    [Header("힐 설정")]
    public float healAmount; // 힐량

    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        // BaseCharecterFSM에 있는 Heal 함수를 호출해요.
        var targetFSM = target as HealthSystem;
        if (targetFSM != null)
        {
            targetFSM.Heal(healAmount);
            Debug.Log($"'{targetFSM.name}'에게 {healAmount}만큼 힐!");
        }
    }
}
