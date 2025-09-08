using JHT;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterTable", menuName = "MonsterTable/Dungeon")]
public class JHT_MonsterDataTable : ScriptableObject
{
    public List<JHT_MonsterDataSO> monsterData;
    public List<Transform> monsterPosData;
    public int totalCost;
}
