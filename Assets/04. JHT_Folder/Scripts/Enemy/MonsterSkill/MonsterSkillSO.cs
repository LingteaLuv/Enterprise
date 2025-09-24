using JHT;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSkill", menuName = "MonsterSkillSO/MonsterSkillSO")]
public class MonsterSkillSO : ScriptableObject
{
    public int ID;
    public string skillName;
    public MonsterSkillAttackType monsterSkillAttackType;
    public MonsterSkillType skillType;
    public ETargetLogic targetLogic;
    public int attackCount;
    public float damagePercent;
    public float buffTime;
    public float coolTime;
    public AnimationClip clip;

    //몬스터 생성 초기에 초기화 시켜야함
    public MonsterSkillSet monsterSkillSet;

    public void Init(JHT_BaseMonsterStat stat)
    {
        switch (monsterSkillAttackType)
        {
            case MonsterSkillAttackType.Monster_0_attack_normal:
                monsterSkillSet = new Monster_0_attack_normal(stat, this);
                break;
            case MonsterSkillAttackType.Monster_0_attack_bow:
                monsterSkillSet = new Monster_0_attack_bow(stat, this);
                break;
            case MonsterSkillAttackType.Monster_0_attack_magic:
                monsterSkillSet = new Monster_0_attack_magic(stat, this);
                break;
            case MonsterSkillAttackType.Monster_longspearattack_1:
                monsterSkillSet = new Monster_longspearattack_1(stat, this);
                break;
            case MonsterSkillAttackType.Monster_shotswordattack_1:
                monsterSkillSet = new Monster_shotswordattack_1(stat, this);
                break;
            case MonsterSkillAttackType.Monster_1_skill_normal:
                monsterSkillSet = new Monster_1_skill_normal(stat, this);
                break;
            case MonsterSkillAttackType.Monster_1_skill_magic:
                monsterSkillSet = new Monster_1_skill_magic(stat, this);
                break;
            case MonsterSkillAttackType.Monster_1_skill_bow:
                monsterSkillSet = new Monster_1_skill_bow(stat, this);
                break;
            default:
                monsterSkillSet = new MonsterSkillSet(stat, this);
                break;
        }
    }

    public void Use(JHT_BaseMonsterFSM fsm)
    {
        monsterSkillSet.UseSkill(fsm);
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