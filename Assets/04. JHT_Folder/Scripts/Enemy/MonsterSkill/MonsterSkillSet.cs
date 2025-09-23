using System;
using System.Collections.Generic;
using JHT;
using UnityEngine;

[Serializable]
public class MonsterSkillSet
{
    MonsterSkillSO skillSO;
    public float totalPower; //?
    MonsterSkillType skillType;
    ETargetLogic target;
    private JHT_BaseMonsterStat stat;

    public MonsterSkillSet(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO)
    {
        this.stat = stat;
        skillSO = _skillSO;
        totalPower = stat.monsterStats[Stat.Attack];
        skillType = skillSO.skillType;
        target = skillSO.targetLogic;
    }

    //private GameObject skillEffect;

    public virtual void UseSkill(JHT_BaseMonsterFSM fsm)
    {
        Debug.LogError("스킬 적용안됨");
    }

}
