using JHT;
using LHI;
using System.Collections;
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
    protected Animator animator;
    private bool isDead = false;
    protected virtual void Awake()
    {
        stats = GetComponent<CombatCharacter>();
        health = GetComponent<HealthSystem>();
        animator = GetComponentInChildren<Animator>();

        if (health == null) Debug.LogError($"{name} : HealthSystem 없음!");
    }

    protected virtual void Start()
    {
        ChangeState(State.Idle);
    }

    protected virtual void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
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

    protected void HandleDeath()
    {
        Debug.Log("핸들 데스 실행됨.");
        if (isDead) return;
        isDead = true;
        ChangeState(State.Dead);
        Die();
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
    public void Die()
    {
        if (animator == null)
        {
            Debug.LogError($"{name} Animator가 null!");
            return;
        }
        Debug.Log($"{stats.charName} 사망 - Die() 진입");


        animator.SetBool("1_Move", false);
        animator.SetBool("0_Idle", false);
        animator.SetTrigger("4_Death");

        Debug.Log($"{stats.charName} 사망");

        if (CompareTag("Crew"))
            BattleManager.Instance?.OnPlayerDead(gameObject);
        else if (CompareTag("Enemy"))
        {
            var monsterStat = GetComponent<JHT_BaseMonsterStat>();
            if (monsterStat != null)
                BattleManager.Instance?.OnEnemyDead(monsterStat);
        }

        // Death 애니메이션 끝나고 비활성화하는 코루틴 추천
        StartCoroutine(DeactivateAfterAnimation());
    }

    private IEnumerator DeactivateAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f); // Death 애니메이션 길이
        gameObject.SetActive(false);
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
