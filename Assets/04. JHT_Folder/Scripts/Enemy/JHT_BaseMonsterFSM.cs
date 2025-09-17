using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace JHT
{
    // 상태 통일

    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject, IAttacker, IDamageable
    {
        //public enum State { Idle, Move, Attack, Dead }

        [Header("State")]
        public PlayerState currentState;
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
        private Coroutine attackBoolCor;
        private WaitForSeconds attackDelayCount;

        public bool canAttack;
        //public bool CanAttack { get { return canAttack; } set { canAttack = value; OnChangeAttack?.Invoke(canAttack); } }
        //public Action<bool> OnChangeAttack;
        #region Animator

        //[Header("Anim")]
        //public Animator animator;

        //public readonly int IDLE = Animator.StringToHash("IDLE");
        //public readonly int MOVE = Animator.StringToHash("MOVE");
        //public readonly int ATTACK = Animator.StringToHash("ATTACK");
        //public readonly int DEATH = Animator.StringToHash("DEATH");

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
            monsterPrefab.transform.localRotation = Quaternion.identity;

            monsterStat.CurHp = monsterStat.maxHp;
            attackDelayCount = new WaitForSeconds(monsterStat.attackDelay);

            //animator = monsterPrefab.GetComponentInChildren<Animator>(true);


            //if (so.animatorOverrideController != null)
            //{
            //    animator.runtimeAnimatorController = monsterStat.animatorOverride;
            //    //animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            //}
            StateMachineInit();

            head = monsterPrefab.transform.Find("Monster_SPUM_1/UnitRoot/Root/BodySet/P_Body/HeadSet/P_Head/P_Head/5_Head")?.GetComponent<SpriteRenderer>();
        }

        private void StateMachineInit()
        {
            stateMachine = new JHT_StateMachine();
            stateMachine.stateDic.Add(PlayerState.IDLE, new Monster_Idle(this));
            stateMachine.stateDic.Add(PlayerState.MOVE, new Monster_Move(this));
            stateMachine.stateDic.Add(PlayerState.ATTACK, new Monster_Attack(this));
            stateMachine.stateDic.Add(PlayerState.DEATH, new Monster_Die(this));
            stateMachine.stateDic.Add(PlayerState.OTHER, new Monster_AttackDelay(this));

            stateMachine.curState = stateMachine.stateDic[PlayerState.IDLE];
            currentState = PlayerState.IDLE;
        }

        protected virtual void OnEnable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp += ShowMonsterUI;

            //OnChangeAttack += CanAttackCor;
        }

        protected virtual void OnDisable()
        {
            if (monsterStat != null)
                monsterStat.OnChangeHp -= ShowMonsterUI;

            //canAttack = false;
            //OnChangeAttack -= CanAttackCor;
        }

        protected virtual void Update()
        {
            if (currentState == PlayerState.DEATH)
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

                    float d = Mathf.Abs(Vector2.Distance(c.gameObject.transform.position, transform.position));

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
            currentState = PlayerState.IDLE;
            ChangeAnim(this.monsterPrefab.GetComponent<PlayerObj>(), currentState);
        }

        public virtual void HandleAttack()
        {
            if (target == null)
                return;

            canAttack = true;
            currentState = PlayerState.ATTACK;
            ChangeAnim(this.monsterPrefab.GetComponent<PlayerObj>(), currentState);

            if (attackBoolCor == null)
            {
                attackBoolCor = StartCoroutine(AttackBoolCor());
            }

        }

        public virtual void HandleMove()
        {
            if (target == null)
                return;

            currentState = PlayerState.MOVE;
            ChangeAnim(this.monsterPrefab.GetComponent<PlayerObj>(), currentState);

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position,
                Time.deltaTime * monsterStat.moveSpeed);


            Rotate();
        }

        public virtual void HandleAttackDelay()
        {
            currentState = PlayerState.OTHER;

            Rotate();

            ChangeAnim(this.monsterPrefab.GetComponent<PlayerObj>(), currentState);

            if (attackCor == null && gameObject != null)
                attackCor = StartCoroutine(AttackCor());

        }

        private IEnumerator AttackBoolCor()
        {
            yield return new WaitForSeconds(1f);
            canAttack = false;

            if (attackBoolCor != null)
            {
                StopCoroutine(attackBoolCor);
                attackBoolCor = null;
            }

        }

        private IEnumerator AttackCor()
        {
            yield return attackDelayCount;
            canAttack = true;

            if (attackCor != null)
            {
                StopCoroutine(attackCor);
                attackCor = null;
            }

        }

        public virtual void HandleDie()
        {
            currentState = PlayerState.DEATH;
            Die();
        }

        //private IEnumerator AttackCor()
        //{
        //    while (true)
        //    {
        //        //if (!animator) yield break;
        //        Debug.LogError("Attackkkkkkkkkkkk222222");

        //        //animator.Play(ATTACK, 0, 0f);
        //        yield return attackDelayCount;
        //    }
        //}

        //private void CanAttackCor(bool value)
        //{
        //    if (value)
        //    {
        //        if (attackCor == null)
        //        {
        //            attackCor = StartCoroutine(AttackCor());
        //        }
        //    }
        //    else
        //    {
        //        if (attackCor != null)
        //        {
        //            StopCoroutine(attackCor);
        //            attackCor = null;
        //        }
        //    }
        //}

        public void Rotate()
        {
            if (target == null || monsterUI == null)
                return;


            float direction = target.transform.position.x - transform.position.x;
            RectTransform ui = monsterUI.GetComponent<RectTransform>();

            if (direction < 0)
            {
                //gameObject.transform.localEulerAngles = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                ui.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                gameObject.transform.localEulerAngles = new Vector3(transform.position.x, 180, transform.position.z);
                ui.transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        private void ApplyDamageEffects(float damage)
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
                Destroy(monsterPrefab);

            Release();
        }


        private void ShowMonsterUI(float value)
        {
            if (monsterUI == null)
                return;
            curHP = value;

            float percent = curHP / (float)monsterStat.maxHp;
            monsterUI.GetComponentInChildren<JHT_UIMonster>().ChangeHP(percent);
        }

        public void ChangeAnim(PlayerObj Unit, PlayerState state)
        {
            Unit.isAction = true;
            Unit._prefabs._anim.Rebind();
            Unit.SetStateAnimationIndex(state);
            Unit.PlayStateAnimation(state);
        }

        // --- IAttacker 인터페이스 구현 ---
        public float GetCurrentStat(Stat stat)
        {
            // BaseMonsterStat에 만든 함수를 호출해서 스탯을 가져와요.
            return monsterStat.GetCurrentStat(stat);
        }

        // --- IDamageable 인터페이스 구현 ---
        public string GetName()
        {
            // 몬스터의 이름을 돌려줘요.
            return monsterStat.curSO.name;
        }

        public void TakeDamage(IAttacker attacker, float powerRatio = 1f)
        {
            // 1. 공격자와 방어자의 스탯 가져오기
            float attackerAttack = attacker.GetCurrentStat(Stat.Attack) * powerRatio;
            float defenderDefense = GetCurrentStat(Stat.Defense); // 자신의 스탯 사용

            // 2. 데미지 계산
            float finalDamage = attackerAttack * (100f / (100f + defenderDefense));

            ApplyDamageEffects(finalDamage);
        }
    }
}