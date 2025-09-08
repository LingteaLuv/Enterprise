using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterTable", menuName = "MonsterTable/Dungeon")]
public class JHT_MonsterDataTable : ScriptableObject
{
    public List<JHT_MonsterDataSO> monsterData;
    public List<JHT_MonsterSetManager> monsterPosData;
    public int roundCount;
    public int totalCost;
}
