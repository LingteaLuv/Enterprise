using LHI;
using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class BaseCharacterFSM : MonoBehaviour
{
    public enum State { Idle, Move, Attack, Skill, Dead }
    public State currentState;

    protected CombatCharacter stats;
    protected HealthSystem health;
    protected Coroutine attackRoutine;
    protected PartyManager partyManager;

    protected virtual void Awake()
    {
        stats = GetComponent<CombatCharacter>();
        health = GetComponent<HealthSystem>();
    }

    protected virtual void Start()
    {
        ChangeState(State.Idle);
    }

    protected virtual void OnEnable()
    {
        // 자식 클래스에서 override 가능하게 열어줌
    }

    protected virtual void OnDisable()
    {

    }
    protected virtual void Update()
    {
        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Move: HandleMove(); break;
            case State.Attack: HandleAttack(); break;
            case State.Skill: HandleSkill(); break; // ← 추가
            case State.Dead: break;
        }
    }

    protected virtual void ChangeState(State newState)
    {
        //Debug.Log($"[FSM] {stats.charName} 상태 전환: {currentState} → {newState}");
        currentState = newState;

        if (newState != State.Attack && attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }
    /*
    public virtual void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
        if (health.currentHealth <= 0)
        {
            ChangeState(State.Dead);
            Die();
        }
    }
    */
    public virtual void Die()
    {
        Debug.Log($"{stats.charName} 사망");

        if (CompareTag("Crew"))
            BattleManager.Instance?.OnPlayerDead(gameObject);

        Destroy(gameObject);
    }

    public virtual void Heal(float amount)
    {
        health.currentHealth = Mathf.Min(health.currentHealth + amount, health.maxHealth);
    }

    protected abstract void HandleIdle();
    protected abstract void HandleMove();
    protected abstract void HandleAttack();
    protected abstract void HandleSkill();
    public void ChangeStateIdleForce()
    {
        currentState = State.Idle;
      //  Debug.Log($"[FSM] {stats.charName} 상태 강제 리셋: Idle");
    }
}
