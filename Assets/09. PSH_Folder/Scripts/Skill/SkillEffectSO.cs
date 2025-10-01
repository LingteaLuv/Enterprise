
using UnityEngine;

/// <summary>
/// 모든 스킬 효과의 기반이 되는 ScriptableObject입니다.
/// 이 클래스를 상속받아 다양한 스킬 효과를 구현하며, 공통 로직을 포함할 수 있습니다.
/// </summary>
public class SkillEffectSO : ScriptableObject
{
    [Header("효과 비주얼")]
    [Tooltip("시전자에게 생성될 이펙트 프리팹")]
    public GameObject casterEffectPrefab;

    [Tooltip("대상에게 생성될 이펙트 프리팹")]
    public GameObject targetEffectPrefab;

    /// <summary>
    /// 스킬 효과를 대상에게 적용합니다.
    /// 자식 클래스에서 이 메소드를 override하여 구체적인 효과를 구현합니다.
    /// </summary>
    public virtual void ApplyEffect(IAttacker caster, IDamageable target)
    {
        if (caster == null || target == null)
        {
            Debug.LogWarning($"[{name}] Caster 또는 Target이 null이므로 효과를 적용할 수 없습니다.");
            return;
        }

        SpawnVisualEffect(caster as Component, casterEffectPrefab);
        SpawnVisualEffect(target as Component, targetEffectPrefab);
    }

    /// <summary>
    /// 설정된 effectPrefab을 대상의 위치에 생성합니다.
    /// </summary>
    private void SpawnVisualEffect(Component comp, GameObject prefab)
    {
        if (prefab == null || comp == null) return;
        EffectPoolManager.Instance.SpawnEffect(prefab, comp.transform.position, Quaternion.identity, 2f);
        var moveEffect = prefab.GetComponent<IMoveEffect>();
        if (moveEffect != null)
        {
            // moveEffect.Init(casterComp.transform, targetComp.transform);
            moveEffect.Init(comp.transform);
        }
        else
        {
            Debug.LogWarning($"[{name}] MoveEffectPrefab에 IMoveEffect 컴포넌트가 없습니다.");
        }
    }
}

