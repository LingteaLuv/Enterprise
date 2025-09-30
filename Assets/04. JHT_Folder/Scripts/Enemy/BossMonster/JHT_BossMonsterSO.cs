using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossMonsterSO", menuName ="BossMonsterSO/BossSO")]
public class JHT_BossMonsterSO : ScriptableObject
{
    public int ID;
    public List<JHT_MonsterDataSO> monsterData;
    public float addStat;
}
