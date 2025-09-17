using System;
using System.Collections;
using UnityEngine;



namespace JHT
{
    // 상태 통일
    public enum MonsterSkillCool
    {
        Normal,
        Skill1,
        Skill2
    }


    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject, IAttacker
    {
        public enum MonsterState { IDLE, MOVE, ATTACK, DEATH, SKILL1, SKILL2 }

        [Header("State")]
        public MonsterState currentState;
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
        [SerializeField] private Transform damageTextPos;

        private Coroutine takeDamageCor;
        private Coroutine attackCor;
        private WaitForSeconds attackDelayCount;

        private bool _initialized;

        private float curHP;
        public float CurHP { get { return curHP; } set { curHP = value; OnChangeHp?.Invoke(curHP); } }
        public Action<float> OnChangeHp;

        #region Animator

        [Header("Anim")]
        public Animator animator;

        public readonly int IDLE = Animator.StringToHash("IDLE");
        public readonly int MOVE = Animator.StringToHash("MOVE");
        public readonly int ATTACK = Animator.StringToHash("ATTACK");
        public readonly int DEATH = Animator.StringToHash("DEATH");
        #endregion


        public virtual void Init(JHT_BaseMonsterStat stat)
        {
            StartCoroutine(StartSetting(stat));
        }

        private IEnumerator StartSetting(JHT_BaseMonsterStat stat)
        {
            _initialized = false;
            target = null;

            yield return new WaitUntil(() => MonsterDataManager.Instance.IsPrefabLoadedFinish);

            yield return null;

            monsterSO = stat.curSO;
            monsterStat = stat;

            OnChangeHp += ShowMonsterUI;

            curHP = monsterStat.maxHp;

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

            if (MonsterDataManager.Instance.monsterPrefabDic.TryGetValue(monsterSO.ID.ToString(), out GameObject value))
            {
                monsterPrefab = Instantiate(value, transform);
                monsterPrefab.transform.localRotation = Quaternion.identity;
            }


            attackDelayCount = new WaitForSeconds(monsterStat.attackDelay - 1);

            animator = monsterPrefab.GetComponentInChildren<Animator>(true);
            
            if (monsterStat.aoc != null)
            {
                animator.runtimeAnimatorController = monsterStat.aoc;
                //animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            }
            StateMachineInit();
            MonsterScale(monsterStat);

            head = monsterPrefab.transform.Find($"{monsterPrefab.name}/UnitRoot/Root/BodySet/P_Body/HeadSet/P_Head/P_Head/5_Head")?.GetComponent<SpriteRenderer>();
        }

        private void StateMachineInit()
        {
            stateMachine = new JHT_StateMachine();
            stateMachine.stateDic.Add(MonsterState.IDLE, new Monster_Idle(this));
            stateMachine.stateDic.Add(MonsterState.MOVE, new Monster_Move(this));
            stateMachine.stateDic.Add(MonsterState.ATTACK, new Monster_Attack(this));
            stateMachine.stateDic.Add(MonsterState.DEATH, new Monster_Die(this));
            stateMachine.stateDic.Add(MonsterState.SKILL1, new Monster_Skill1(this));
            stateMachine.stateDic.Add(MonsterState.SKILL2, new Monster_Skill2(this));

            currentState = MonsterState.IDLE;
            stateMachine.curState = stateMachine.stateDic[MonsterState.IDLE];

            _initialized = true;
        }

        private void MonsterScale(JHT_BaseMonsterStat monsterStat)
        {

            switch (monsterStat.monsterRarity)
            {
                case MonsterRarity.Elite:
                    gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1);
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.2f, 0);
                    break;
                case MonsterRarity.Normal:
                    gameObject.transform.localScale = Vector3.one;
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.15f, 0);
                    break;
                case MonsterRarity.Boss:
                    gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.3f, 0);
                    break;
            }
        }


        protected virtual void OnDisable()
        {
            OnChangeHp -= ShowMonsterUI;

            //canAttack = false;
            //OnChangeAttack -= CanAttackCor;
        }

        protected virtual void Update()
        {
            if (!_initialized) return;

            if (currentState == MonsterState.DEATH)
                return;

            if (stateMachine == null || stateMachine.curState == null) return;

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
            ChangeAnim(IDLE, MonsterState.IDLE);
        }

        public virtual void HandleAttack()
        {
            if (target == null)
                return;

            ChangeAnim(ATTACK, MonsterState.ATTACK);

            if (attackCor == null)
            {
                attackCor = StartCoroutine(AttackCor());
            }

        }

        public virtual void HandleMove()
        {
            if (target == null)
                return;
            ChangeAnim(MOVE, MonsterState.MOVE);

        }

        public virtual void HandleDie()
        {
            currentState = MonsterState.DEATH;
            Die();
        }

        private IEnumerator AttackCor()
        {
            while (true)
            {
                if (!animator) 
                    yield break;

                animator.Play(ATTACK, 0, 0f);
                yield return new WaitForSeconds(1f);

                yield return attackDelayCount;
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
            CurHP = Mathf.Max(CurHP - damage, 0);
            if (CurHP == 0)
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
            BattleManager.Instance.OnEnemyDead(monsterStat);

            Outit();
        }

        public void Outit()
        {
            monsterSO = null;

            monsterUI.gameObject.SetActive(false);

            ChangeAnim(DEATH, MonsterState.DEATH);

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

        private void ChangeAnim(int temp, MonsterState curState)
        {
            if (animator == null || curState == currentState)
                return;

            if (currentState == MonsterState.ATTACK && attackCor != null)
            {
                StopCoroutine(attackCor);
                attackCor = null;
            }

            currentState = curState;

            animator.Play(temp);
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