using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum ETargetLogic
{
    // --- 아군 대상 ---
    Self,                       // 나 자신
    AllAllies,                  // 모든 아군
    SingleAlly_ByRole,          // 특정 역할의 아군 1명 (첫 번째)
    SingleLowestAlly_ByRole,    // 특정 역할 중 체력이 가장 낮은 아군 1명
    AllAllies_ByRole,           // 특정 역할의 모든 아군

    // --- 적군 대상 ---
    PrimaryTarget,              // 캐릭터의 현재 주 타겟 1명
    ClosestEnemy,               // 가장 가까운 적 1명
    LowestHealthEnemy,          // 체력이 가장 낮은 적 1명
    AllEnemiesInRadius          // 특정 반경 내 모든 적 (광역)
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
    public float cooldown;
    public ESkillTargetType skillTargetType;

    [Header("타겟 설정")]
    public ETargetLogic targetLogic;
    public CrewRole targetRole; // targetLogic이 ByRole일 때 사용할 역할
    public float skillRadius = 3f; // 광역 스킬일 때 사용할 반경

    [Header("스킬 효과 목록")]
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    // 캐릭터의 주 타겟을 옵션으로 받는 Use 함수
    public void Use(IAttacker caster, IDamageable primaryTarget = null)
    {
        CombatCharacter casterCharacter = caster as CombatCharacter;
        if (casterCharacter == null) return;

        List<IDamageable> finalTargets = new List<IDamageable>();

        // 1. 버프 스킬이면, 아군 찾는 로직 수행
        if (skillTargetType == ESkillTargetType.Supportive)
        {
            finalTargets = FindSupportiveTargets(casterCharacter);
        }
        // 2. 공격 스킬이면, 적군 찾는 로직 수행
        else // Offensive
        {
            finalTargets = FindOffensiveTargets(casterCharacter, primaryTarget);
        }

        // 3. 최종 타겟들에게 효과 적용
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

    private List<IDamageable> FindOffensiveTargets(CombatCharacter caster, IDamageable primaryTarget)
    {
        List<IDamageable> targets = new List<IDamageable>();
        var allEnemies = FindAllEnemies(caster);

        switch (targetLogic)
        {
            case ETargetLogic.PrimaryTarget:
                if (primaryTarget != null)
                {
                    targets.Add(primaryTarget);
                }
                else // 주 타겟이 없으면, 대신 가장 가까운 적을 찾아요.
                {
                    var closest = allEnemies.FirstOrDefault();
                    if (closest != null) targets.Add(closest);
                }
                break;

            case ETargetLogic.ClosestEnemy:
                var closestEnemy = allEnemies.FirstOrDefault();
                if (closestEnemy != null) targets.Add(closestEnemy);
                break;

            case ETargetLogic.LowestHealthEnemy:
                var lowestEnemy = allEnemies
                    .OrderBy(e => (e as Component).GetComponent<HealthSystem>()?.currentHealth ?? (e as JHT.JHT_BaseMonsterFSM)?.CurHP ?? float.MaxValue)
                    .FirstOrDefault();
                if (lowestEnemy != null) targets.Add(lowestEnemy);
                break;

            case ETargetLogic.AllEnemiesInRadius:
                var enemiesInRadius = allEnemies
                    .Where(e => Vector3.Distance(caster.transform.position, (e as MonoBehaviour).transform.position) <= skillRadius);
                targets.AddRange(enemiesInRadius);
                break;

            // 역할 기반으로 '적'을 찾는 로직
            case ETargetLogic.AllAllies_ByRole:
                var enemiesWithRole = allEnemies
                    .OfType<JHT.JHT_BaseMonsterFSM>() // 적(몬스터)만 필터링
                    .Where(fsm => fsm.monsterStat != null && fsm.monsterStat.monsterCrewRole == targetRole);
                targets.AddRange(enemiesWithRole);
                break;
        }
        return targets;
    }

    private List<IDamageable> FindSupportiveTargets(CombatCharacter caster)
    {
        List<IDamageable> found = new List<IDamageable>();
        var allAllies = GameObject.FindGameObjectsWithTag("Crew")
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

                // 지정된 역할부터 시작해서, 다음 역할 순으로 아군을 찾아요.
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
        return found;
    }

    private IEnumerable<IDamageable> FindAllEnemies(CombatCharacter caster)
    {
        // 이제 몬스터도 IDamageable을 사용하므로, GetComponent<IDamageable>로 찾아요.
        return GameObject.FindGameObjectsWithTag("Enemy")
            .Select(e => e.GetComponent<IDamageable>())
            .Where(e => e != null) // 살아있는지 여부는 각 컴포넌트가 관리
            .OrderBy(e => Vector3.Distance(caster.transform.position, (e as MonoBehaviour).transform.position));
    }
}
