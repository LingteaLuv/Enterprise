
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace JHT
{
    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject
    {
        public enum State { Idle, Move, Attack, Dead }
        public State currentState;

        protected Coroutine attackRoutine;

        public LayerMask targetLayer;
        public Transform target;
        public JHT_MonsterDataSO monsterSO;
        public JHT_BaseMonsterStat monsterStat;
        public JHT_UIMonster monsterUI;

        private bool canAttack;
        private int curAnim;
        private bool isHurt;
        // 임시용
        Coroutine attackCor;

        [SerializeField] private Transform damageTextPos;

        // test용
        public float curHP;

        #region Animator

        Animator animator;

        public readonly int IDLE = Animator.StringToHash("IDLE");
        public readonly int MOVE = Animator.StringToHash("MOVE");
        public readonly int ATTACK = Animator.StringToHash("ATTACK");

        #endregion



        protected virtual void Start()
        {
            ChangeState(State.Idle);
            target = GameObject.FindWithTag("Crew").transform;
            canAttack = true;
        }

        public virtual void Init(JHT_MonsterDataSO so)
        {
            monsterSO = so;
            monsterStat.Init(monsterSO);

            GameObject spawnedVisual = Instantiate(so.enemyCharacter,transform);
            spawnedVisual.transform.localPosition = Vector3.zero;
            spawnedVisual.transform.localRotation = Quaternion.identity;
            spawnedVisual.transform.localScale = Vector3.one;

            monsterStat.CurHp = so.maxHp;

            if (monsterStat.monsterRarity == MonsterRarity.Elite)
            {
                gameObject.transform.localScale *= 1.3f;
                spawnedVisual.transform.localScale = gameObject.transform.localScale;
            }
            else
            {
                gameObject.transform.localScale = Vector3.one;
                spawnedVisual.transform.localScale = gameObject.transform.localScale;
            }

            animator = GetComponentInChildren<Animator>();

            if (so.animatorOverrideController != null) 
            {
                animator.runtimeAnimatorController = monsterStat.animatorOverride;
                animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            }
        }

        protected virtual void OnEnable()
        {
            monsterStat.OnChangeHp += ShowMonsterUI;
        }

        protected virtual void OnDisable()
        {
            monsterStat.OnChangeHp += ShowMonsterUI;
        }

        protected virtual void Update()
        {
            
            switch (currentState)
            {
                case State.Idle:
                    HandleIdle();
                    break;
                case State.Attack:
                    HandleAttack();
                    break;
                case State.Move:
                    HandleMove();
                    break;
                case State.Dead:
                    HandleDie();
                    break;
            }
            
            CheckDistance();
        }

        protected void ChangeState(State newState)
        {
            if (isHurt)
            {
                if (newState == State.Idle)
                {
                    return;
                }
            }

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
                ChangeState(State.Move);
            }
            else
            {
                ChangeState(State.Idle);
            }
        }

        protected virtual void HandleIdle()
        {
            //애니메이션 설정만
            AnimatorChange(IDLE);
        }
        protected virtual void HandleAttack()
        {
            AnimatorChange(ATTACK);
        }

        protected virtual void HandleMove()
        {
            AnimatorChange(MOVE);
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * monsterStat.moveSpeed);
            //Rotate();
        }


        protected virtual void HandleDie()
        {
            Die();
        }


        //다시
        private void Rotate()
        {
            if (target == null)
                return;

            float direction = target.position.x - transform.position.x;
        }

        public void TakeDamage(float damage)
        {
            monsterStat.CurHp = Mathf.Max(monsterStat.CurHp - damage, 0);
            if (monsterStat.CurHp == 0)
            {
                JHT_DamageBox obj = JHT_MonsterSpawnManager.Instance.damageTextPool.GetPooled() as JHT_DamageBox; 
                obj.ShowDamageText(damage);
                obj.transform.position = damageTextPos.position;
                ChangeState(State.Dead);
            }
            else
            {
                JHT_DamageBox obj = JHT_MonsterSpawnManager.Instance.damageTextPool.GetPooled() as JHT_DamageBox;
                obj.ShowDamageText(damage);
                obj.transform.position = damageTextPos.position;
                ChangeState(State.Move);
                isHurt = true;
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


        private void AnimatorChange(int temp)
        {
            animator.Play(temp);
            curAnim = temp;
        }


        private void ShowMonsterUI(float value)
        {
            if (monsterUI == null) 
                return;
            curHP = value;
            monsterUI.ChangeHP(value);
        }
    }
}