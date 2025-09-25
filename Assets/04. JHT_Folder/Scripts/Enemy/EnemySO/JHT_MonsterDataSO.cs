using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSO", menuName = "MonsterSO/NoramlMonster")]
public class JHT_MonsterDataSO : ScriptableObject
{
    public int ID;
    public string monsterName;

    public float attackRange;
    public float attackDelay;
    public float chaseRange;
    public float moveSpeed;


    public AtkRangeType monsterAttackType;
    public CrewRole monsterCrewRole;

    public List<MonsterStat> monsterStat;

    public Sprite projectileSprite;

    public RuntimeAnimatorController baseController;
    
    public string normalSkill;
    public string skill1;
    public string skill2;
}

[System.Serializable]
public class MonsterStat
{
    public Stat stat;
    public float amount;
}


public enum MonsterRarity
{
    Normal,
    Elite,
    Boss
}

//public enum CrewRole
//{
//    Deckhand,   // 갑판원
//    Sailor,    // 선원
//    Cook,      // 요리사
//    Captain  // 선장
//}

//public enum Stat
//{
//    Attack,
//    Health,
//    Defense,
//    CritChance,
//    CritDamage,
//    AttackSpeed
//}