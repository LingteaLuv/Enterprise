
using System.Collections.Generic;
using System.Linq;
using JHT;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSkill", menuName = "MonsterSkillSO/MonsterSkillSO")]
public class MonsterSkillSO : ScriptableObject
{
    public int ID;
    public string skillName;


    [Header("타겟 설정")]
    public ESkillTargetType skillTargetType;
    public ETargetLogic targetLogic;
    public CrewRole targetRole;
    public MonsterSkillAttackType monsterSkillAttackType;

    public float coolTime;
    public AnimationClip clip;

    //몬스터 생성 초기에 초기화 시켜야함
    //public MonsterSkillSet monsterSkillSet;

    //모든 스탯 증가일때 스탯마다 스킬을 다 넣어야하기 떄문에 list로
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    public void Init(JHT_BaseMonsterStat stat)
    {
        //switch (monsterSkillAttackType)
        //{
        //    case MonsterSkillAttackType.Monster_0_attack_normal:
        //        monsterSkillSet = new Monster_0_attack_normal(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_0_attack_bow:
        //        monsterSkillSet = new Monster_0_attack_bow(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_0_attack_magic:
        //        monsterSkillSet = new Monster_0_attack_magic(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_longspearattack_1:
        //        monsterSkillSet = new Monster_longspearattack_1(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_shotswordattack_1:
        //        monsterSkillSet = new Monster_shotswordattack_1(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_1_skill_normal:
        //        monsterSkillSet = new Monster_1_skill_normal(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_1_skill_magic:
        //        monsterSkillSet = new Monster_1_skill_magic(this);
        //        break;
        //    case MonsterSkillAttackType.Monster_1_skill_bow:
        //        monsterSkillSet = new Monster_1_skill_bow(this);
        //        break;
        //    default:
        //        monsterSkillSet = new MonsterSkillSet(this);
        //        break;
        //}

        
    }

    public void Use(JHT_BaseMonsterFSM fsm)
    {
        //monsterSkillSet.UseSkill(fsm);

        if (fsm == null) return;

        List<IDamageable> finalTargets = new List<IDamageable>();

        // 1. 버프 스킬이면, 아군 찾는 로직 수행
        if (skillTargetType == ESkillTargetType.Supportive)
        {
            finalTargets = FindSupportiveTargets(fsm);
        }
        // 2. 공격 스킬이면, 적군 찾는 로직 수행
        else // Offensive
        {
            finalTargets = FindOffensiveTargets(fsm, null);
        }

        // 3. 최종 타겟들에게 효과 적용
        foreach (var target in finalTargets.Distinct())
        {
            if (target == null) continue;
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.ApplyEffect(fsm, target);
                }
            }
        }
    }

    protected List<IDamageable> FindOffensiveTargets(JHT_BaseMonsterFSM fsm, IDamageable primaryTarget)
    {
        List<IDamageable> targets = new List<IDamageable>();
        switch (targetLogic)
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

        switch (targetLogic)
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
                int startIndex = roleValues.IndexOf(targetRole);
                if (startIndex == -1) break;

                // 지정된 역할부터 시작해서, 다음 역할 순으로 아군을 찾아요.
                for (int i = startIndex; i < roleValues.Count; i++)
                {
                    CrewRole currentRole = roleValues[i];
                    candidates = allAllies.Where(ally => ally.monsterStat.monsterCrewRole == currentRole).ToList();
                    if (candidates.Count > 0) break;
                }

                if (targetLogic == ETargetLogic.AllAllies_ByRole)
                {
                    found.AddRange(candidates);
                }
                else if (targetLogic == ETargetLogic.SingleLowestAlly_ByRole)
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

public enum MonsterSkillType
{
    Buff,
    Attack
}

public enum MonsterSkillAttackType
{
    Monster_0_attack_normal,
    Monster_0_attack_bow,
    Monster_0_attack_magic,
    Monster_shotswordattack_1,
    Monster_longspearattack_1,
    Monster_1_skill_normal,
    Monster_1_skill_magic,
    Monster_1_skill_bow
}


//public enum ETargetLogic
//{
//    // --- 아군 대상 ---
//    Self,                       // 나 자신
//    AllAllies,                  // 모든 아군
//    SingleAlly_ByRole,          // 특정 역할의 아군 1명 (첫 번째)
//    SingleLowestAlly_ByRole,    // 특정 역할 중 체력이 가장 낮은 아군 1명
//    AllAllies_ByRole,           // 특정 역할의 모든 아군

//    // --- 적군 대상 ---
//    PrimaryTarget,              // 캐릭터의 현재 주 타겟 1명
//    ClosestEnemy,               // 가장 가까운 적 1명
//    LowestHealthEnemy,          // 체력이 가장 낮은 적 1명
//    AllEnemiesInRadius          // 특정 반경 내 모든 적 (광역)
//}