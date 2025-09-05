using LHI;
using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class BaseCharacterFSM : MonoBehaviour
{
    protected enum State { Idle, Move, Attack, Dead }
    protected State currentState;

    protected CombatCharacter stats;
    protected HealthSystem health;
    protected Coroutine attackRoutine;

    protected virtual void Awake()
    {
        stats = GetComponent<CombatCharacter>();
        health = GetComponent<HealthSystem>();
    }

    protected virtual void Start()
    {
        ChangeState(State.Idle);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Move: HandleMove(); break;
            case State.Attack: HandleAttack(); break;
            case State.Dead: break;
        }
    }

    protected virtual void ChangeState(State newState)
    {
        currentState = newState;
        if (newState != State.Attack && attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
        if (health.currentHealth <= 0)
        {
            ChangeState(State.Dead);
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");

        if (CompareTag("Crew"))
            BattleManager.Instance?.OnPlayerDead();

        Destroy(gameObject);
    }

    public virtual void Heal(float amount)
    {
        health.currentHealth = Mathf.Min(health.currentHealth + amount, health.maxHealth);
    }

    protected abstract void HandleIdle();
    protected abstract void HandleMove();
    protected abstract void HandleAttack();
}
