
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
        public JHT_MonsterDataSO monsterSO;
        public JHT_BaseMonsterStat monsterStat;
        private SpriteRenderer enemySpriteRender;

        Animator animator;

        private bool canAttack;

        // 임시용
        Coroutine attackCor;
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            enemySpriteRender = GetComponent<SpriteRenderer>();
        }



        protected virtual void Start()
        {
            ChangeState(State.Idle);
            target = GameObject.FindWithTag("Player").transform;
            canAttack = true;
        }

        public virtual void Init(JHT_MonsterDataSO so)
        {
            monsterSO = so;
            monsterStat.Init(monsterSO);

            enemySpriteRender.sprite = monsterSO.enemyCharacter;
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
            if (canAttack)
            {
                AnimatorChange("Attack");
                canAttack = false;
            }
            else
            {
                if(attackCor == null)
                StartCoroutine(CullTime(monsterStat.attackDelay));
            }
        }

        protected virtual void HandleChase()
        {
            AnimatorChange("Chase");
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * monsterStat.moveSpeed);
            Rotate();
        }


        protected virtual void HandleDie()
        {
            Die();
        }

        //private async UnitaskVoid CullTime(float cullTime)
        //{
        //    task =>
        //    {
        //        if(task.IsFault || task.IsCancled)
        //        {
        //            canAttack = false;
        //            return;
        //        }
        //        await Unitask.WatiForSeconds(cullTime);
        //        canAttack = true;
        //    };
        //    
        //}

        IEnumerator CullTime(float cullTime)
        {
            yield return new WaitForSeconds(cullTime);
            canAttack = true;

            if (attackCor != null)
            {
                StopCoroutine(attackCor);
                attackCor = null;
            }
            
        }
        

        private void Rotate()
        {
            float direction = ((Vector2)target.position - (Vector2)transform.position).magnitude;
            if (direction < 0)
                enemySpriteRender.flipX = false;
            else
                enemySpriteRender.flipX = true;
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

    }
}