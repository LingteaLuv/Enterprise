using System;
using System.Collections.Generic;
using System.Linq;
using JHT;
using UnityEngine;

[Serializable]
public class MonsterSkillSet
{
    MonsterSkillSO skillSO;

    public List<SkillEffectSO> effects = new List<SkillEffectSO>();
    public MonsterSkillSet(MonsterSkillSO _skillSO)
    {
        skillSO = _skillSO;
        effects = skillSO.effects;
    }

    public virtual void UseSkill(IAttacker caster, IDamageable primaryTarget = null)
    {
        JHT_BaseMonsterFSM fsm = caster as JHT_BaseMonsterFSM;
        if (fsm == null) return;
        
        List<IDamageable> finalTargets = new List<IDamageable>();

        // 1. 버프 스킬이면, 아군 찾는 로직 수행
        if (skillSO.skillTargetType == ESkillTargetType.Supportive)
        {
            finalTargets = FindSupportiveTargets(fsm);
        }
        // 2. 공격 스킬이면, 적군 찾는 로직 수행
        else // Offensive
        {
            finalTargets = FindOffensiveTargets(fsm, primaryTarget);
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

    protected List<IDamageable> FindOffensiveTargets(JHT_BaseMonsterFSM fsm, IDamageable primaryTarget)
    {
        List<IDamageable> targets = new List<IDamageable>();
        switch (skillSO.targetLogic)
        {
            case ETargetLogic.PrimaryTarget:
                if (primaryTarget != null)
                {
                    targets.Add(primaryTarget);
                }
                else // 주 타겟이 없으면, 대신 가장 가까운 적을 찾아요.
                {
                    var closest = FindAllEnemies(fsm).FirstOrDefault();
                    if (closest != null) targets.Add(closest);
                }
                break;

            case ETargetLogic.ClosestEnemy:
                var closestEnemy = FindAllEnemies(fsm).FirstOrDefault();
                if (closestEnemy != null) targets.Add(closestEnemy);
                break;

            case ETargetLogic.LowestHealthEnemy:
                var lowestEnemy = FindAllEnemies(fsm)
                    .OrderBy(e => (e as Component).GetComponent<JHT_BaseMonsterFSM>()?.CurHP ?? float.MaxValue)
                    .FirstOrDefault();
                if (lowestEnemy != null) targets.Add(lowestEnemy);
                break;

            case ETargetLogic.AllEnemiesInRadius:
                var enemiesInRadius = FindAllEnemies(fsm)
                    .Where(e => Vector3.Distance(fsm.transform.position, (e as MonoBehaviour).transform.position) <= fsm.monsterStat.attackRange);
                targets.AddRange(enemiesInRadius);
                break;
        }
        return targets;
    }

    protected List<IDamageable> FindSupportiveTargets(JHT_BaseMonsterFSM fsm)
    {
        List<IDamageable> found = new List<IDamageable>();
        var allAllies = GameObject.FindGameObjectsWithTag("Enemy")
                                  .Select(go => go.GetComponent<JHT_BaseMonsterFSM>())
                                  .Where(cc => cc != null)
                                  .ToList();

        switch (skillSO.targetLogic)
        {
            case ETargetLogic.Self:
                found.Add(fsm);
                break;

            case ETargetLogic.AllAllies:
                found.AddRange(allAllies);
                break;

            case ETargetLogic.SingleAlly_ByRole:
            case ETargetLogic.AllAllies_ByRole:
            case ETargetLogic.SingleLowestAlly_ByRole:
                List<JHT_BaseMonsterFSM> candidates = new List<JHT_BaseMonsterFSM>();
                var roleValues = System.Enum.GetValues(typeof(CrewRole)).Cast<CrewRole>().ToList();
                int startIndex = roleValues.IndexOf(skillSO.targetRole);
                if (startIndex == -1) break;

                // 지정된 역할부터 시작해서, 다음 역할 순으로 아군을 찾아요.
                for (int i = startIndex; i < roleValues.Count; i++)
                {
                    CrewRole currentRole = roleValues[i];
                    candidates = allAllies.Where(ally => ally.monsterStat.monsterCrewRole == currentRole).ToList();
                    if (candidates.Count > 0) break;
                }

                if (skillSO.targetLogic == ETargetLogic.AllAllies_ByRole)
                {
                    found.AddRange(candidates);
                }
                else if (skillSO.targetLogic == ETargetLogic.SingleLowestAlly_ByRole)
                {
                    var lowestAlly = candidates.OrderBy(c => c.GetComponent<JHT_BaseMonsterFSM>().CurHP / c.GetComponent<HealthSystem>().maxHealth).FirstOrDefault();
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

    protected IEnumerable<CombatCharacter> FindAllEnemies(JHT_BaseMonsterFSM caster)
    {
        // 이제 몬스터도 IDamageable을 사용하므로, GetComponent<CombatCharacter>로 찾아요.
        IEnumerable<CombatCharacter> crews = GameObject.FindGameObjectsWithTag("Crew")
            .Select(e => e.GetComponent<CombatCharacter>())
            .Where(e => e != null) // 살아있는지 여부는 각 컴포넌트가 관리
            .OrderBy(e => Vector3.Distance(caster.transform.position, (e as MonoBehaviour).transform.position));
        return crews;
    }

}
