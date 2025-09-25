using JHT;
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
        base.ApplyEffect(caster, target);

        var targetMonoBehaviour = target as MonoBehaviour;
        if (targetMonoBehaviour == null) return;

        var combatCharacter = targetMonoBehaviour.GetComponent<CombatCharacter>();
        if (combatCharacter != null)
        {
            combatCharacter.ApplyOnHitDamageBuff(damageValue, duration);
            Debug.Log($"아군 '{combatCharacter.charName}'에게 {duration}초 동안 추가 데미지({damageValue}) 버프를 적용합니다.");
        }
        else
        {
            var monsterFsm = targetMonoBehaviour.GetComponent<JHT_BaseMonsterFSM>();
            if (monsterFsm != null)
            {
                // 나중에 추가된다면 주석 해제
                // monsterFsm.ApplyOnHitDamageBuff(damageValue, duration);
                Debug.Log($"몬스터 '{monsterFsm.name}'에게 {duration}초 동안 추가 데미지({damageValue}) 버프를 적용합니다.");
            }
            else
            {
                Debug.LogWarning($"추가 데미지 버프를 적용할 수 없는 대상입니다: {targetMonoBehaviour.name}");
            }
        }
    }
}
