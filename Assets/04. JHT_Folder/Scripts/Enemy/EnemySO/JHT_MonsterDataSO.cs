using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSO", menuName = "MonsterSO/NoramlMonster")]
public class JHT_MonsterDataSO : ScriptableObject
{
    [field : SerializeField] public int ID { get; private set; }
    [field: SerializeField] public MonsterType monsterType { get; private set; }
    [field: SerializeField] public MonsterRarity monsterRarity { get; private set; }
    [field: SerializeField] public CrewRole monsterCrewRole { get; private set; }
    [field: SerializeField] public Sprite enemyCharacter { get; private set; }
    [field: SerializeField] public Sprite enemyIcon { get; private set; }
    [field: SerializeField] public float maxHp { get; private set; }
    [field: SerializeField] public float attackPower { get; private set; }
    [field: SerializeField] public float defense { get; private set; }
    [field: SerializeField] public float attackRange { get; private set; }
    [field: SerializeField] public float attackSpeed { get; private set; } = 0;
    [field: SerializeField] public float chaseRange { get; private set; }
    [field: SerializeField] public float moveSpeed { get; private set; }
    [field: SerializeField] public float attackDelay { get; private set; }
    [field: SerializeField] public int cost { get; private set; }

    // 1. 프리팹 하나로 만들경우 모든 몬스터의 projectile을 만들어야함
    // 2. 프리팹, 이미지 따로할경우 하나의 프리팹으로 모든 이미지를 적용할 수 있음
    [field: SerializeField] public Sprite projectileSprite { get; private set; } = null;

    //[field: SerializeField] public Sprite particle { get; private set; } = null;
    //[field: SerializeField] public EnemySkill enemySkill { get; private set; } = null;
    //애니메이션 R&D 후 추가 animator override controller OR animator
    [field: SerializeField] public AnimatorOverrideController animatorOverrideController { get; private set; } = null;
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

//public enum CrewRole
//{
//    Deckhand,   // 갑판원
//    Sailor,    // 선원
//    Cook,      // 요리사
//    Captain  // 선장
//}