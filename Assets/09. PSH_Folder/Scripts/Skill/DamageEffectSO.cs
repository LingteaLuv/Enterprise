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


    public override void ApplyEffect(IAttacker caster, IDamageable target)
    {
        base.ApplyEffect(caster, target);

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