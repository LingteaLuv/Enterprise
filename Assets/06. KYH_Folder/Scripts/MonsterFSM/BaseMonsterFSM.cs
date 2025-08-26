using LHI;
using UnityEngine;

public abstract class BaseMonsterFSM : Character
{
    protected enum State { Idle, Move, Attack, Dead }
    protected State currentState;

    [Header("공통 데이터")]
   // public float moveSpeed;         // 이동속도
   // public float attackRange;       // 사거리
   // public float attackDelay;       // 공격 딜레이
   // public int maxHP;               // 최대 체력
   // protected int currentHP;        // 현재 체력
   public CharacterData CharacterData;

    // TODO : 이 공통 데이터들은 차후에 난이도(Level) 이나 스테이지 숫자에 따라 비례증가 할 수 있으니 그에따라 계산하는 로직 필요할 수 있음( 특히 체력이나 공격력 등 )

    protected Coroutine attackRoutine;

    protected virtual void Start()
    {
        currentHP = maxHP;
        ChangeState(State.Idle);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Move:
                HandleMove();
                break;
            case State.Attack:
                HandleAttack();
                break;
            case State.Dead:
                break;
        }
    }

    protected abstract void HandleIdle();
    protected abstract void HandleMove();
    protected abstract void HandleAttack();

    protected virtual void ChangeState(State newState)
    {
        currentState = newState;

        if (newState != State.Attack && attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    public override void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            ChangeState(State.Dead);
            Die();
        }
    }

    public override void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        // 애니메이션 / 이펙트 등
        // gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public override void Heal(float amount)
    {
        
    }

    public override void Move()
    {
        
    }

    
}

