using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace JHT
{
    // 상태 통일
    
    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject
    {
        public enum State { Idle, Move, Attack, Dead }

        [Header("State")]
        public State currentState;
        public JHT_StateMachine stateMachine;

        [Header("Target")]
        public LayerMask targetLayer;
        public GameObject target;

        [Header("Monster Setting")]
        public JHT_MonsterDataSO monsterSO;
        public JHT_BaseMonsterStat monsterStat;
        public GameObject monsterPrefab;
        public Canvas monsterUI;
        SpriteRenderer head;
        public float curHP;
        [SerializeField] private Transform damageTextPos;

        private Coroutine takeDamageCor;
        private Coroutine attackCor;
        private WaitForSeconds attackDelayCount;

        private bool canAttack;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; OnChangeAttack?.Invoke(canAttack); } }
        public Action<bool> OnChangeAttack;
        #region Animator

        [Header("Anim")]
        public Animator animator;

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
            target = null;
            canAttack = false;

            monsterSO = so;
            monsterStat.Init(monsterSO);

            if (attackCor != null)
            {
                StopCoroutine(attackCor);
                attackCor = null;
            }

            if (takeDamageCor != null)
            {
                StopCoroutine(takeDamageCor);
                takeDamageCor = null;
            }

            monsterPrefab = Instantiate(monsterStat.enemyCharacter, transform);
            monsterPrefab.transform.localPosition = Vector3.zero;
            monsterPrefab.transform.localRotation = Quaternion.identity;
            monsterPrefab.transform.localScale = Vector3.one;

            monsterStat.CurHp = monsterStat.maxHp;
            attackDelayCount = new WaitForSeconds(monsterStat.attackDelay);

            animator = monsterPrefab.GetComponentInChildren<Animator>(true);


            if (so.animatorOverrideController != null)
            {
                animator.runtimeAnimatorController = monsterStat.animatorOverride;
                //animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            }
            StateMachineInit();

            head = monsterPrefab.transform.Find("Root/BodySet/P_Body/HeadSet/P_Head/P_Head/5_Head")?.GetComponent<SpriteRenderer>();
        }

        private void StateMachineInit()
        {
            stateMachine = new JHT_StateMachine();
            stateMachine.stateDic.Add(State.Idle,new Monster_Idle(this));
            stateMachine.stateDic.Add(State.Move, new Monster_Move(this));
            stateMachine.stateDic.Add(State.Attack, new Monster_Attack(this));
            stateMachine.stateDic.Add(State.Dead, new Monster_Die(this));
            
            stateMachine.curState = stateMachine.stateDic[State.Idle];
            currentState = State.Idle;
        }

        protected virtual void OnEnable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp += ShowMonsterUI;

            OnChangeAttack += CanAttackCor;
        }

        protected virtual void OnDisable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp -= ShowMonsterUI;

            canAttack = false;
            OnChangeAttack -= CanAttackCor;
        }

        protected virtual void Update()
        {
            if (currentState == State.Dead)
                return;

            stateMachine.Update();
        }

        public void TryAcquireNextTarget()
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.chaseRange, targetLayer);

            foreach (var c in cols)
            {
                if (c != null)
                {
                    if (c.gameObject.GetComponent<HealthSystem>().currentHealth <= 0)
                    {
                        continue;
                    }

                    float d = Mathf.Abs(Vector2.Distance(c.gameObject.transform.position ,transform.position));
                    
                    if (d < monsterStat.chaseRange)
                    {
                        target = c.gameObject;
                        break;
                    }
                }
            }
        }

        public virtual void HandleIdle()
        {
            currentState = State.Idle;
        }

        public virtual void HandleAttack()
        {
            if (target == null)
                return;

            currentState = State.Attack;

        }

        public virtual void HandleMove()
        {
            if (target == null)
                return;

            currentState = State.Move;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 
                Time.deltaTime * monsterStat.moveSpeed);

            
            Rotate();
        }


        public virtual void HandleDie()
        {
            currentState = State.Dead;
            Die();
        }

        private IEnumerator AttackCor()
        {
            while (true)
            {
                if (!animator) yield break;
                animator.Play(ATTACK, 0, 0f);
                yield return attackDelayCount;
            }
        }

        private void CanAttackCor(bool value)
        {
            if (value)
            {
                if (attackCor == null)
                {
                    attackCor = StartCoroutine(AttackCor());
                }
            }
            else
            {
                if (attackCor != null)
                {
                    StopCoroutine(attackCor);
                    attackCor = null;
                }
            }
        }

        public void Rotate()
        {
            if (target == null || monsterUI == null)
                return;


            float direction = target.transform.position.x - transform.position.x;
            RectTransform ui = monsterUI.GetComponent<RectTransform>();

            if (direction < 0)
            {
                gameObject.transform.localEulerAngles = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                ui.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                gameObject.transform.localEulerAngles = new Vector3(transform.position.x, 180, transform.position.z);
                ui.localScale = new Vector3(-1, 1, 1);
            }
        }

        public void TakeDamage(float damage)
        {
            monsterStat.CurHp = Mathf.Max(monsterStat.CurHp - damage, 0);
            if (monsterStat.CurHp == 0)
            {
                JHT_DamageBox obj = JHT_MonsterSpawnManager.Instance.damageTextPool.GetPooled() as JHT_DamageBox; 
                obj.ShowDamageText(damage);
                obj.transform.position = damageTextPos.position;
            }
            else
            {
                JHT_DamageBox obj = JHT_MonsterSpawnManager.Instance.damageTextPool.GetPooled() as JHT_DamageBox;
                obj.ShowDamageText(damage);
                obj.transform.position = damageTextPos.position;

                if (takeDamageCor == null)
                {
                    takeDamageCor = StartCoroutine(TakeDamageCor());
                }
            }
        }

        IEnumerator TakeDamageCor()
        {
            WaitForSeconds pingpongTime = new WaitForSeconds(0.2f);

            Color basicColor = Color.white;
            Color changeColor = Color.red;
            
            if (head != null)
            {
                head.color = changeColor;
                yield return pingpongTime;
                head.color = basicColor;
                yield return pingpongTime;
            }
            yield return null;

            if (takeDamageCor != null)
            {
                StopCoroutine(takeDamageCor);
                takeDamageCor = null;
            }
        }

        private void Die()
        {
            BattleManager.Instance.OnEnemyDead(monsterSO);

            Outit();
        }

        public void Outit()
        {
            monsterSO = null;

            monsterUI.gameObject.SetActive(false);

            if (attackCor != null)
            {
                StopCoroutine(attackCor);
                attackCor = null;
            }
            

            if (monsterPrefab != null)
                Destroy(monsterPrefab, 2f);


            Release(0.2f);

        }


            
        private void ShowMonsterUI(float value)
        {
            if (monsterUI == null) 
                return;
            curHP = value;

            float percent = curHP / (float)monsterStat.maxHp;
            monsterUI.GetComponentInChildren<JHT_UIMonster>().ChangeHP(percent);
        }
    }
}