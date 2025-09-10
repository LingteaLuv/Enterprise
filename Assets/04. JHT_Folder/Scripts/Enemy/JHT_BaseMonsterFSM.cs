
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public GameObject target;
        public JHT_MonsterDataSO monsterSO;
        public JHT_BaseMonsterStat monsterStat;
        public JHT_UIMonster monsterUI;
        public GameObject monsterPrefab;

        private int curAnim;
        private bool isDead;

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
        public readonly int DEATH = Animator.StringToHash("DEATH");

        #endregion


        public virtual void Init(JHT_MonsterDataSO so)
        {
            StartSetting(so);
        }

        private void StartSetting(JHT_MonsterDataSO so)
        {
            isDead = false;
            target = null;
            ChangeState(State.Idle);

            monsterSO = so;
            monsterStat.Init(monsterSO);

            monsterPrefab = Instantiate(so.enemyCharacter, transform);
            monsterPrefab.transform.localPosition = Vector3.zero;
            monsterPrefab.transform.localRotation = Quaternion.identity;
            monsterPrefab.transform.localScale = Vector3.one;

            monsterStat.CurHp = so.maxHp;

            animator = GetComponentInChildren<Animator>();

            if (so.animatorOverrideController != null)
            {
                animator.runtimeAnimatorController = monsterStat.animatorOverride;
                animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            }
        }

        protected virtual void OnEnable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp += ShowMonsterUI;
        }

        protected virtual void OnDisable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp -= ShowMonsterUI;
        }

        protected virtual void Update()
        {
            if (isDead)
                return;

            if (!IsValidTarget(target))
            {
                TryAcquireNextTarget();
            }

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
            if (isDead)
                return;

            currentState = newState;

            if (newState != State.Attack && attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
        }

        protected void CheckDistance()
        { 
            if (target == null || isDead)
                return;

            float distance = (target.transform.position - transform.position).magnitude;

            if (distance < monsterStat.attackRange)
            {
                ChangeState(State.Attack);
            }
            else if (distance > monsterStat.chaseRange)
            {
                ChangeState(State.Idle);
            }
            
        }
        protected bool IsValidTarget(GameObject target)
        {
            if (target == null)
            {
                ChangeState(State.Idle);
                return false;
            }

            HealthSystem hs = target.GetComponent<HealthSystem>();

            if (hs == null)
            {
                ChangeState(State.Idle);
                return false;
            }

            if (hs.currentHealth <= 0)
            {
                target = null;
                ChangeState(State.Idle);
                return false;
            }


            return true;
        }

        // 진짜 for쓰기 싫은데...
        protected void TryAcquireNextTarget()
        {
            if (isDead)
                return;

            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.chaseRange, targetLayer);

            foreach (var c in cols)
            {
                if (c.gameObject.GetComponent<HealthSystem>().currentHealth <= 0)
                {
                    continue;
                }

                if (c != null)
                {
                    float d = (c.gameObject.transform.position - transform.position).magnitude;
                    if (d < monsterStat.chaseRange)
                    {
                        target = c.gameObject;
                        ChangeState(State.Move);
                        break;
                    }
                }
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
            if (target == null)
            {
                ChangeState(State.Idle);
            }
        }

        protected virtual void HandleMove()
        {
            AnimatorChange(MOVE);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * monsterStat.moveSpeed);
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

            //float direction = target.transform.position.x - transform.position.x;
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
            }
        }

        private void Die()
        {
            BattleManager.Instance.OnEnemyDead(monsterSO);
            
            Outit();
        }

        public void Outit()
        {
            isDead = true;
            monsterSO = null;

            monsterUI.ReleaseMonsterHP();
            monsterUI.gameObject.SetActive(false);
            

            if (monsterPrefab != null)
                Destroy(monsterPrefab, 0.2f);

            AnimatorChange(DEATH);

            Release(0.2f);

        }

        private void AnimatorChange(int temp)
        {
            if (curAnim == temp && temp != ATTACK || animator == null)
                return;

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