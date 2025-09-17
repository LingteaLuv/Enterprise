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
        CombatCharacter casterCharacter = caster as CombatCharacter;
        if (casterCharacter == null) return;

        // 비동기 메서드를 호출해서 여러 번 때리는 로직을 실행해요.
        ApplyDamageAsync(casterCharacter, target);
    }

    private async void ApplyDamageAsync(CombatCharacter caster, IDamageable target)
    {
        for (int i = 0; i < hitCount; i++)
        {
            // 루프를 도는 중에 타겟이 사라졌는지 확인해요.
            if (target == null || (target as Object) == null)
            {
                Debug.Log("타겟이 사라져서 데미지 적용을 중단합니다.");
                break;
            }

            // 새로 만든 TakeDamage 메서드를 호출하면서 powerRatio를 넘겨줘요!
            target.TakeDamage(caster, powerRatio);

            // 마지막 타격 후에는 딜레이를 주지 않아요.
            if (i < hitCount - 1)
            {
                await Task.Delay(delayBetweenHits);
            }
        }
    }
}