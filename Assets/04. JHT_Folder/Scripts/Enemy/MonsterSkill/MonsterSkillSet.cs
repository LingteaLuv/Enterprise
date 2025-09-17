using System;
using System.Collections.Generic;
using JHT;
using UnityEngine;

public abstract class MonsterSkillSet : MonoBehaviour
{
    public float totalPower; //?
    MonsterSkillType skillType;
    

    //private GameObject skillEffect;

    public void Init(JHT_BaseMonsterStat stat)
    {
        totalPower = stat.totalAttackPower;
        
    }

    public abstract void Attck(MonsterSkillCool collType);
}
