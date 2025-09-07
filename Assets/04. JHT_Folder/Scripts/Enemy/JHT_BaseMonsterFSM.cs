
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JHT
{
    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject
    {
        public enum State { Idle, Move, Attack, Chase, Dead }
        public State currentState;

        protected Coroutine attackRoutine;

        public LayerMask targetLayer;
        public Transform target;
        public JHT_BaseMonsterStat monsterStat;
        private SpriteRenderer enemyCharacter;

        // 테스트용 public -> 테이블에서 Pool진행 후 Init으로 몬스터 so지정
        JHT_ObservableProperty<State> OnChangeState;

        Animator animator;

        protected virtual void Awake()
        {
            monsterStat = GetComponent<JHT_BaseMonsterStat>();
            animator = GetComponent<Animator>();
            enemyCharacter = GetComponent<SpriteRenderer>();
        }



        protected virtual void Start()
        {
            ChangeState(State.Idle);
            monsterStat.Init(monsterStat.curSO);
            target = GameObject.FindWithTag("Player").transform;
        }

        public virtual void Init(JHT_BaseMonsterStat so)
        {
            monsterStat = so;
            enemyCharacter.sprite = monsterStat.characterImage;
            if (monsterStat.monsterRarity == MonsterRarity.Elite)
            {
                gameObject.transform.localScale *= 1.3f;
            }

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
                case State.Chase:
                    HandleChase();
                    break;
                case State.Dead:
                    HandleDie();
                    break;
            }

            CheckDistance();
        }

        protected void ChangeState(State newState)
        {
            currentState = newState;

            if (newState != State.Attack && attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
        }

        protected void CheckDistance()
        {
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance < monsterStat.attackRange)
            {
                ChangeState(State.Attack);
            }
            else if (distance < monsterStat.chaseRange)
            {
                ChangeState(State.Chase);
            }
            else
            {
                ChangeState(State.Idle);
            }
        }

        protected virtual void HandleIdle()
        {
            //애니메이션 설정만
            AnimatorChange("Idle");
        }
        protected virtual void HandleMove()
        {
            
        }
        protected virtual void HandleAttack()
        {
            AnimatorChange("Attack");
        }

        protected virtual void HandleChase()
        {
            AnimatorChange("Chase");
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * monsterStat.moveSpeed);
            //Rotate();
        }

        protected virtual void HandleDie()
        {
            Die();
        }

        private void Rotate()
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            direction.y = 0f;
            transform.forward = direction;
        }

        public void TakeDamage(float damage)
        {
            monsterStat.curHp = Mathf.Max(monsterStat.curHp - damage, 0);
            if (monsterStat.curHp <= 0)
            {
                ChangeState(State.Dead);
            }
            else
            {
                ChangeState(State.Chase);
            }
        }

        public void Die()
        {
            Debug.Log($"{gameObject.name} 사망");
            BattleManager.Instance.OnEnemyDead(gameObject);
            // 애니메이션 / 이펙트 등
            // gameObject.SetActive(false);
            Release(0.2f);
        }


        private void AnimatorChange(string temp)
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Chase", false);
            animator.SetBool("Attack", false);
        
            animator.SetBool(temp, true);
        }

        //IEnumerator WaitForNextState(State state, float _delay)
        //{
        //       
        //}
    }
}