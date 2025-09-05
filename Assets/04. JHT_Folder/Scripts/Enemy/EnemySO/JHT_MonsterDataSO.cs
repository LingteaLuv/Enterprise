using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSO", menuName = "MonsterSO/NoramlMonster")]
public class JHT_MonsterDataSO : ScriptableObject
{
    [field : SerializeField] public int ID { get; private set; }
    [field: SerializeField] public MonsterType monsterType { get; private set; }
    [field: SerializeField] public MonsterRarity monsterRarity { get; private set; }
    [field: SerializeField] public Sprite enemyIcon { get; private set; }
    [field: SerializeField] public float maxHp { get; private set; }
    [field: SerializeField] public float upAttackPower { get; private set; }
    [field: SerializeField] public float upDefense { get; private set; }
    [field: SerializeField] public float attackRange { get; private set; }
    [field: SerializeField] public float attackSpeed { get; private set; } = 0;
    [field: SerializeField] public float chaseRange { get; private set; }
    [field: SerializeField] public float upMoveSpeed { get; private set; }
    [field: SerializeField] public float attackDelay { get; private set; }
    [field: SerializeField] public JHT_MonsterProjectile projectile { get; private set; } = null;
    [field: SerializeField] public GameObject particle { get; private set; } = null;

    //[field: SerializeField] public EnemySkill enemySkill { get; private set; } = null;
    //애니메이션 R&D 후 추가 animator override controller OR animator
}

public enum MonsterType
{
    close,
    open
}

public enum MonsterRarity
{
    Normal,
    Elite
}