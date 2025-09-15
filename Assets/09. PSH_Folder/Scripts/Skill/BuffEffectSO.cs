using UnityEngine;

public enum BuffType
{
    Flat,       // 고정 수치
    Percent     // 백분율
}

public enum EBuffTargetLogic
{
    Self,                       // 나 자신
    AllAllies,                  // 모든 아군
    AllAllies_ByRole,           // 특정 역할의 모든 아군
    SingleLowestAlly_ByRole     // 특정 역할 중 체력이 가장 낮은 아군 1명
}

[CreateAssetMenu(fileName = "New Buff Effect", menuName = "Skills/Effects/Buff")]
public class BuffEffectSO : SkillEffectSO
{
    [Header("타겟 설정")]
    public EBuffTargetLogic targetLogic = EBuffTargetLogic.Self;

    // targetLogic이 ByRole일 때 사용할 역할
    public CrewRole targetRole;

    [Header("버프 설정")]
    public Stat statToBuff; // 버프할 스탯
    public BuffType buffType;   // 버프 타입 (고정 또는 백분율)
    public float buffValue;     // 버프 수치
    public float duration;      // 지속 시간

    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        CombatCharacter targetCharacter = target as CombatCharacter;
        if (targetCharacter != null)
        {
            targetCharacter.ApplyBuff(statToBuff, buffValue, duration, buffType);
            Debug.Log($"'{targetCharacter.name}'에게 {statToBuff} 버프를 {duration}초 동안 적용했습니다.");
        }
        else
        {
            Debug.LogWarning("버프는 CombatCharacter에게만 적용할 수 있습니다.");
        }
    }
}
