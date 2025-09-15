using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ETargetLogic
{
    Self,                       // 나 자신
    AllAllies,                  // 모든 아군
    SingleAlly_ByRole,          // 특정 역할의 아군 1명 (첫 번째)
    SingleLowestAlly_ByRole,    // 특정 역할 중 체력이 가장 낮은 아군 1명
    AllAllies_ByRole            // 특정 역할의 모든 아군
}

public enum ESkillTargetType
{
    Offensive,  // 적을 공격하는 스킬
    Supportive, // 아군을 돕는 스킬
}

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

    [Header("타겟 설정")]
    public ETargetLogic targetLogic = ETargetLogic.Self;
    public CrewRole targetRole; // targetLogic이 ByRole일 때 사용할 역할

    [Header("스킬 효과 목록")]
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    public void Use(IAttacker caster)
    {
        CombatCharacter casterCharacter = caster as CombatCharacter;
        if (casterCharacter == null) return;

        Debug.Log($"'{casterCharacter.name}'이(가) '{skillName}' 스킬을 사용!");

        // 1. 이 스킬의 targetLogic에 따라 타겟을 찾아요.
        List<IDamageable> finalTargets = FindTargets(casterCharacter);

        // 2. 찾아낸 모든 타겟에게, 모든 이펙트를 적용해요!
        foreach (var target in finalTargets.Distinct())
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

    private List<IDamageable> FindTargets(CombatCharacter caster)
    {
        List<IDamageable> found = new List<IDamageable>();

        if (skillTargetType == ESkillTargetType.Supportive)
        {
            var allAllies = GameObject.FindGameObjectsWithTag("Player")
                                      .Select(go => go.GetComponent<CombatCharacter>())
                                      .Where(cc => cc != null)
                                      .ToList();

            switch (targetLogic)
            {
                case ETargetLogic.Self:
                    found.Add(caster);
                    break;

                case ETargetLogic.AllAllies:
                    found.AddRange(allAllies);
                    break;

                case ETargetLogic.SingleAlly_ByRole:
                case ETargetLogic.AllAllies_ByRole:
                case ETargetLogic.SingleLowestAlly_ByRole:
                    List<CombatCharacter> candidates = new List<CombatCharacter>();
                    var roleValues = System.Enum.GetValues(typeof(CrewRole)).Cast<CrewRole>().ToList();
                    int startIndex = roleValues.IndexOf(targetRole);
                    if (startIndex == -1) break;

                    for (int i = startIndex; i < roleValues.Count; i++)
                    {
                        CrewRole currentRole = roleValues[i];
                        candidates = allAllies.Where(ally => ally.CharacterStats.characterdata.crewRole == currentRole).ToList();
                        if (candidates.Count > 0) break;
                    }

                    if (targetLogic == ETargetLogic.AllAllies_ByRole)
                    {
                        found.AddRange(candidates);
                    }
                    else if (targetLogic == ETargetLogic.SingleLowestAlly_ByRole)
                    {
                        var lowestAlly = candidates.OrderBy(c => c.GetComponent<HealthSystem>().currentHealth / c.GetComponent<HealthSystem>().maxHealth).FirstOrDefault();
                        if (lowestAlly != null) found.Add(lowestAlly);
                    }
                    else // SingleAlly_ByRole
                    {
                        var firstAlly = candidates.FirstOrDefault();
                        if (firstAlly != null) found.Add(firstAlly);
                    }
                    break;
            }
        }
        else // Offensive
        {
            // 기본 공격처럼 가장 가까운 적 1명 찾기
            var closestEnemy = GameObject.FindGameObjectsWithTag("Enemy")
                .Select(e => new { obj = e, health = e.GetComponent<HealthSystem>() })
                .Where(e => e.obj != null && e.health != null && e.health.currentHealth > 0)
                .OrderBy(e => Vector3.Distance(caster.transform.position, e.obj.transform.position))
                .FirstOrDefault();
            if (closestEnemy != null) found.Add(closestEnemy.obj.GetComponent<IDamageable>());
        }
        return found;
    }
}