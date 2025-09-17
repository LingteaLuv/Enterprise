using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSO", menuName = "MonsterSO/NoramlMonster")]
public class JHT_MonsterDataSO : ScriptableObject
{
    public int ID;
    public string monsterName;
    public AtkRangeType monsterAttackType;
    public CrewRole monsterCrewRole;
    public Sprite enemyIcon;
    public float maxHp;
    public float attackPower;
    public float criticalAttackPower;
    public float criticalPower;
    public float defense;
    public float attackRange;
    public float attackSpeed;
    public float attackDelay;
    public float chaseRange;
    public float moveSpeed;
    public int cost;

    // 1. 프리팹 하나로 만들경우 모든 몬스터의 projectile을 만들어야함
    // 2. 프리팹, 이미지 따로할경우 하나의 프리팹으로 모든 이미지를 적용할 수 있음
    public Sprite projectileSprite;

    public RuntimeAnimatorController baseController;
    //[field: SerializeField] public Sprite particle { get; private set; } = null;
    //[field: SerializeField] public EnemySkill enemySkill { get; private set; } = null;

    public string normalSkill;
    public string skill1;
    public string skill2;
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