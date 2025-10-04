using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "Skills/Effects/Damage")]
public class DamageEffectSO : SkillEffectSO
{
    [Header("데미지 설정")]
    [Tooltip("시전자 공격력에 곱해지는 피해 계수입니다.")]
    public float powerRatio = 1f;

    [Tooltip("총 공격 횟수입니다.")]
    public int hitCount = 1;

    [Tooltip("타격 사이의 시간 간격 (밀리초)")]
    public int delayBetweenHits = 100;

    [Header("이동 효과 설정")]
    [Tooltip("Caster에서 Target으로 이동하는 효과 프리팹")]
    public GameObject moveEffectPrefab;


    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        Debug.Log($"[{name}] ApplyEffect 호출됨. Caster: {caster}, Target: {target}");
        base.ApplyEffect(caster, target);

        if (moveEffectPrefab != null && caster is Component casterComp && target is Component targetComp)
        {
            Debug.Log($"[{name}] 이동 효과 생성 시도.");
            // 이동 효과의 지속시간을 2초로 설정합니다.
            GameObject effectObj = EffectPoolManager.Instance.SpawnEffect(moveEffectPrefab, casterComp.transform.position, Quaternion.identity, 2f);
            if (effectObj == null)
            {
                Debug.LogError($"[{name}] EffectPoolManager가 null을 반환했습니다.");
                ApplyDamageAsync(caster, target);
                return;
            }

            var moveEffect = effectObj.GetComponent<TestMoveEffect>();
            if (moveEffect != null)
            {
               moveEffect.Init(casterComp.transform, targetComp.transform);
                //moveEffect.Init(targetComp.transform);
            }
            else
            {
                Debug.LogWarning($"[{name}] MoveEffectPrefab에 IMoveEffect 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] 이동 효과를 생성할 수 없습니다. Prefab: {moveEffectPrefab != null}, Caster: {caster is Component}, Target: {target is Component}");
        }

        //CombatCharacter casterCharacter = caster as CombatCharacter;
        //if (casterCharacter == null) return;
        //GameObject inst = (caster as MonoBehaviour).gameObject;
        //GameObject t = (target as MonoBehaviour).gameObject;

        ApplyDamageAsync(caster, target);
    }

    private async void ApplyDamageAsync(IAttacker caster, IDamageable target)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (target == null || (target as Object) == null)
            {
                Debug.Log("타겟이 사라져서 데미지 적용을 중단합니다.");
                break;
            }
            target.TakeDamage(caster, powerRatio);
            if (i < hitCount - 1)
            {
                await Task.Delay(delayBetweenHits);
            }
        }
    }

}