using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ESkillTargetType
{
    Offensive,  // 적을 공격하는 스킬
    Supportive, // 아군을 돕는 스킬
}

/// <summary>
/// 스킬의 기본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("스킬 정보")]
    public int skillID;
    public string skillName;
    [TextArea] public string skillDescription;
    public Sprite skillIcon;
    public float cooldown;
    public ESkillTargetType skillTargetType = ESkillTargetType.Offensive;

    [Header("스킬 효과 목록")]
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    // Use 함수에서 targets 파라미터를 제거하고, 타겟을 스스로 찾도록 변경!
    public void Use(IAttacker caster)
    {
        CombatCharacter casterCharacter = caster as CombatCharacter;
        if (casterCharacter == null)
        {
            Debug.LogError("스킬 시전자가 CombatCharacter가 아닙니다!");
            return;
        }

        List<IDamageable> finalTargets = new List<IDamageable>();

        // 스킬 타입에 따라 타겟을 찾아요.
        if (skillTargetType == ESkillTargetType.Supportive)
        {
            // 버프/힐 스킬일 경우, 첫 번째 버프 이펙트의 타겟 로직을 따라요.
            BuffEffectSO buffEffect = effects.OfType<BuffEffectSO>().FirstOrDefault();
            if (buffEffect != null)
            {
                finalTargets.AddRange(FindTargetsForBuff(casterCharacter, buffEffect));
            }
        }
        else // Offensive 스킬일 경우
        {
            // TODO: 공격 스킬의 타겟팅 로직이 있다면 여기에 추가!
            // 예: DamageEffectSO에 타겟팅 정보가 있다면 그에 맞게 FindTargetsForAttack() 호출

            // 기본적으로 가장 가까운 적 1명을 공격해요.
            if (finalTargets.Count == 0)
            {
                finalTargets.AddRange(FindDefaultEnemyTarget(casterCharacter));
            }
        }

        // 찾아낸 모든 타겟들에게, 모든 이펙트를 적용해요!
        foreach (var target in finalTargets.Distinct()) // 중복된 타겟이 없도록 Distinct() 처리
        {
            if (target == null) continue;
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.ApplyEffect(caster, target);
                }
            }
        }
    }

    private List<IDamageable> FindTargetsForBuff(CombatCharacter caster, BuffEffectSO buffEffect)
    {
        List<IDamageable> found = new List<IDamageable>();
        var allAllies = GameObject.FindGameObjectsWithTag("Crew")
                                  .Select(go => go.GetComponent<CombatCharacter>())
                                  .Where(cc => cc != null)
                                  .ToList();

        switch (buffEffect.targetLogic)
        {
            case EBuffTargetLogic.Self:
                found.Add(caster);
                break;

            case EBuffTargetLogic.AllAllies:
                found.AddRange(allAllies);
                break;

            case EBuffTargetLogic.AllAllies_ByRole:
            case EBuffTargetLogic.SingleLowestAlly_ByRole:
                List<CombatCharacter> candidates = new List<CombatCharacter>();
                var roleValues = System.Enum.GetValues(typeof(CrewRole)).Cast<CrewRole>().ToList();
                int startIndex = roleValues.IndexOf(buffEffect.targetRole);

                if (startIndex == -1) break; // 유효하지 않은 역할이면 중단

                // --- ✨ 스마트 폴백(Fallback) 로직 시작 ✨ ---
                for (int i = startIndex; i < roleValues.Count; i++)
                {
                    CrewRole currentRole = roleValues[i];
                    candidates = allAllies.Where(ally => ally.CharacterStats.characterdata.crewRole == currentRole).ToList();

                    if (candidates.Count > 0)
                    {
                        Debug.Log($"타겟 발견! 역할: '{currentRole}'");
                        break; // 후보를 찾았으면 다음 역할로 넘어가지 않고 중단
                    }
                    Debug.Log($"역할 '{currentRole}'을(를) 가진 아군이 없어 다음 역할을 탐색합니다: {roleValues.ElementAtOrDefault(i + 1)}");
                }
                // --- 스마트 폴백(Fallback) 로직 끝 ---

                if (buffEffect.targetLogic == EBuffTargetLogic.AllAllies_ByRole)
                {
                    found.AddRange(candidates);
                }
                else // SingleLowestAlly_ByRole
                {
                    var lowestAlly = candidates.OrderBy(c => c.GetComponent<HealthSystem>().currentHealth / c.GetComponent<HealthSystem>().maxHealth)
                                               .FirstOrDefault();
                    if (lowestAlly != null) found.Add(lowestAlly);
                }
                break;
        }
        return found;
    }

    private List<IDamageable> FindDefaultEnemyTarget(CombatCharacter caster)
    {
        List<IDamageable> found = new List<IDamageable>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = float.MaxValue;
        GameObject closestEnemy = null;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            var health = enemy.GetComponent<HealthSystem>();
            if (health != null && health.currentHealth <= 0) continue; // 죽은 적은 타겟에서 제외

            float distance = Vector3.Distance(caster.transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            var damageable = closestEnemy.GetComponent<IDamageable>();
            if (damageable != null) found.Add(damageable);
        }
        return found;
    }
}
