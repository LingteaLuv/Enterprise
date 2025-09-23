using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterTable", menuName = "MonsterTable/Dungeon")]
public class JHT_MonsterDataTable : ScriptableObject
{
    public int ID;
    public List<JHT_MonsterDataSO> monsterData;
    public float addStat;
    public int roundCount;
}
