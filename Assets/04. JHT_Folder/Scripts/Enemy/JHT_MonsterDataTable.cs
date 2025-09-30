using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterTable", menuName = "MonsterTable/Dungeon")]
public class JHT_MonsterDataTable : ScriptableObject
{
    public int ID;
    public List<JHT_MonsterDataSO> monsterData;
    public List<JHT_MonsterDataSO> captinMonsterData;
    public float addStat;
    public float captinAddStat;
    public int roundCount;
}
