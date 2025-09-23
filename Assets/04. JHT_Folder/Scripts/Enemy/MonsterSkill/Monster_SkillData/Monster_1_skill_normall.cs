using JHT;
using UnityEngine;

public class Monster_1_skill_normal : MonsterSkillSet
{
    public Monster_1_skill_normal(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO) : base(stat, _skillSO)
    {
    }

    public override void UseSkill(JHT_BaseMonsterFSM fsm)
    {

        Debug.LogError("Monster_1_skill_normal Set");
    }
}
