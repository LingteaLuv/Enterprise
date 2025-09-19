using JHT;
using UnityEngine;

public class Monster_longspearattack_1 : MonsterSkillSet
{
    public Monster_longspearattack_1(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO) : base(stat, _skillSO)
    {
    }

    public override void UseSkill(JHT_BaseMonsterFSM fsm)
    {
        
        Debug.LogError("Monster_longspearattack_1 Set");
    }
}
