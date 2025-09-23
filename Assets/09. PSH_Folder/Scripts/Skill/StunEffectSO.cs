using UnityEngine;

/// <summary>
/// 대상에게 지정된 시간 동안 스턴 효과를 적용하는 SkillEffect입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Stun Effect", menuName = "Skills/Effects/StunEffect")]
public class StunEffectSO : SkillEffectSO
{
    [Header("스턴 설정")]
    [Tooltip("스턴 효과가 지속될 시간(초)")]
    public float duration = 2f;

    /// <summary>
    /// 대상에게 스턴 효과를 적용합니다.
    /// </summary>
    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        // IDamageable에서 MonoBehaviour를 가져와 컴포넌트에 접근합니다.
        var targetMonoBehaviour = target as MonoBehaviour;
        if (targetMonoBehaviour == null) return;

        // 대상의 FSM(상태 머신)을 가져옵니다. 적도 스턴을 쓴다면 태그에 따라 FSM 컴포넌트를 가져오는 코드 추가
        var fsm = targetMonoBehaviour.GetComponent<BaseCharacterFSM>();
        if (fsm != null)
        {
            Debug.Log($"{targetMonoBehaviour.name}에게 {duration}초 동안 스턴 효과를 적용합니다!");
            
            // FSM에 스턴 상태를 N초간 적용하라고 명령합니다.
            // fsm.ApplyStun(duration);
        }
    }
}
