using JHT;
using UnityEngine;

public class Monster_1_skill_magic : MonsterSkillSet
{
    public Monster_1_skill_magic(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO) : base(stat, _skillSO)
    {
    }

    public override void UseSkill(JHT_BaseMonsterFSM fsm)
    {
        
        Debug.LogError("Monster_1_attack_magic Set");
    }
}
