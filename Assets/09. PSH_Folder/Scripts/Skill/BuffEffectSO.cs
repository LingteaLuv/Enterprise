using JHT;
using UnityEngine;

public enum BuffType
{
    Flat,       // 고정 수치
    Percent     // 백분율
}

[CreateAssetMenu(fileName = "New Buff Effect", menuName = "Skills/Effects/Buff")]
public class BuffEffectSO : SkillEffectSO
{
    [Header("버프 설정")]
    public Stat statToBuff; // 버프할 스탯
    public BuffType buffType;   // 버프 타입 (고정 또는 백분율)
    public float buffValue;     // 버프 수치
    public float duration;      // 지속 시간

    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        base.ApplyEffect(caster, target);

        var targetMonoBehaviour = target as MonoBehaviour;
        if (targetMonoBehaviour == null) return;

        var combatCharacter = targetMonoBehaviour.GetComponent<CombatCharacter>();
        if (combatCharacter != null)
        {
            combatCharacter.ApplyBuff(statToBuff, buffValue, duration, buffType);
            Debug.Log($"아군 '{combatCharacter.charName}'에게 {statToBuff} 버프를 {duration}초 동안 적용했습니다.");
        }
        else
        {
            var monsterFsm = targetMonoBehaviour.GetComponent<JHT_BaseMonsterFSM>();
            if (monsterFsm != null)
            {
                // JHT_BaseMonsterFSM에 ApplyBuff가 있다고 가정합니다.
                monsterFsm.ApplyBuff(statToBuff, buffValue, duration, buffType);
                Debug.Log($"몬스터 '{monsterFsm.name}'에게 {statToBuff} 버프를 {duration}초 동안 적용했습니다.");
            }
            else
            {
                Debug.LogWarning($"버프를 적용할 수 없는 대상입니다: {targetMonoBehaviour.name}");
            }
        }
    }
}