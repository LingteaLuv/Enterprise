using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        private float attackDelayCount;

        private bool _initialized;
        public bool skill1Active;
        public bool skill2Active;
        public bool isAttacking;

        private CancellationTokenSource[] token;

        private float curHP;
        public float CurHP { get { return curHP; } set { curHP = value; OnChangeHp?.Invoke(curHP); } }
        public Action<float> OnChangeHp;

        #region Animator

        [Header("Anim")]
        public Animator animator;

        public readonly int IDLE = Animator.StringToHash("IDLE");
        public readonly int MOVE = Animator.StringToHash("MOVE");
        public readonly int ATTACK = Animator.StringToHash("ATTACK");
        public readonly int SKILL1 = Animator.StringToHash("SKILL1");
        public readonly int SKILL2 = Animator.StringToHash("SKILL2");
        public readonly int DEATH = Animator.StringToHash("DEATH");
        #endregion


        public virtual void Init(JHT_BaseMonsterStat stat)
        {
            StartCoroutine(StartSetting(stat));
        }

        private IEnumerator StartSetting(JHT_BaseMonsterStat stat)
        {
            // 풀에서 받은 이전의 값 정리
            _initialized = false;
            target = null;
            isAttacking = false;


            yield return new WaitUntil(() => MonsterDataManager.Instance.IsPrefabLoadedFinish);

            yield return null;

            // 초기 데이터 값 세팅

            monsterSO = stat.curSO;
            monsterStat = stat;
            OnChangeHp += ShowMonsterUI;

            
            curHP = monsterStat.maxHp;

            // 이전의 코루틴 정리
            if (takeDamageCor != null)
            {
                StopCoroutine(takeDamageCor);
                takeDamageCor = null;
            }

            // 몬스터 데이터(프리팹) 가져오기
            if (MonsterDataManager.Instance.monsterPrefabDic.TryGetValue(monsterSO.ID.ToString(), out GameObject value))
            {
                monsterPrefab = Instantiate(value, transform);
                monsterPrefab.transform.localRotation = Quaternion.identity;
            }

            //ResetTokens();
            SetAnimSkill();
            // 초기 상태 세팅
            StateMachineInit();
            MonsterScale(monsterStat);

           
        }
        #region 초기설정
        private void SetAnimSkill()
        {
            //공격 딜레이 설정
            attackDelayCount = monsterStat.normalSkill.skillDelay - monsterStat.normalSkill.clip.length > 0 ?
                monsterStat.normalSkill.skillDelay - monsterStat.normalSkill.clip.length : 1;

            //스킬 초기화
            if (monsterStat.normalSkill != null)
            {
                monsterStat.normalSkill.Init(monsterStat);
            }

            if (monsterStat.skill1 != null)
            {
                monsterStat.skill1.Init(monsterStat);
            }

            if (monsterStat.skill2 != null)
            {
                monsterStat.skill2.Init(monsterStat);
            }

            // 애니메이션 세팅
            animator = monsterPrefab.GetComponentInChildren<Animator>(true);

            if (monsterStat.aoc != null)
            {
                animator.runtimeAnimatorController = monsterStat.aoc;
                //animator.SetFloat("AttackSpeed", monsterStat.attackSpeed);
            }
        }

        private void StateMachineInit()
        {
            stateMachine = new JHT_StateMachine();
            stateMachine.stateDic.Add(MonsterState.IDLE, new Monster_Idle(this));
            stateMachine.stateDic.Add(MonsterState.MOVE, new Monster_Move(this));
            stateMachine.stateDic.Add(MonsterState.ATTACK, new Monster_Attack(this));
            stateMachine.stateDic.Add(MonsterState.DEATH, new Monster_Die(this));

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
        private void ResetTokens()
        {
            token = new CancellationTokenSource[3];

            // 기존 토큰 정리
            for (int i = 0; i < token.Length; i++)
            {
                if (token[i] != null)
                {
                    token[i].Cancel();
                    token[i].Dispose();
                    token[i] = null;
                }
            }

            if (monsterStat.normalSkill != null)
                token[0] = new CancellationTokenSource();
            else
                token[0] = null;

            if (monsterStat.skill1 != null)
                token[1] = new CancellationTokenSource();
            else
                token[1] = null;

            if (monsterStat.skill2 != null)
                token[2] = new CancellationTokenSource();
            else
                token[2] = null;
        }
#endregion
        
        protected virtual void OnDisable()
        {
            OnChangeHp -= ShowMonsterUI;

            if (token != null)
            {
                for (int i = 0; i < token.Length; i++)
                {
                    if (token[i] != null)
                    {
                        token[i].Dispose();
                        token[i] = null;
                    }
                }
            }
            
            //canAttack = false;
            //OnChangeAttack -= CanAttackCor;
        }

        protected virtual void Update()
        {
            if (!_initialized) return;

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

        public virtual void HandleAttackAsync()
        {
            if (target == null)
                return;

            ResetTokens();

            ChangeAnim(ATTACK, MonsterState.ATTACK);

            if (monsterStat.normalSkill != null)
                _= Attack(attackDelayCount);

            if (monsterStat.skill1 != null)
                _ = Skill1CoolTime(monsterStat.skill1.skillDelay);
            
            if (monsterStat.skill2 != null) 
                _ = Skill2CoolTime(monsterStat.skill2.skillDelay);
        }

        public virtual void HandleMove()
        {
            if (target == null)
                return;
            ChangeAnim(MOVE, MonsterState.MOVE);

        }

        public virtual void HandleDie()
        {
            Die();
        }


        private async UniTaskVoid Attack(float coolTime)
        {
            skill2Active = false;
            skill2Active = false;

            while (true)
            {
                if (token[0] == null || token[0].Token.IsCancellationRequested) 
                    return;

                if (!animator)
                    return;

                await UniTask.WaitUntil(() => !skill1Active || !skill2Active, cancellationToken: token[0].Token);

                isAttacking = true;
                animator.Play(ATTACK, 0, 0f);
                Debug.LogError("NormalAttack");
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.normalSkill.clip.length), cancellationToken: token[0].Token);

                isAttacking = false;
                await UniTask.Delay(TimeSpan.FromSeconds(coolTime), cancellationToken: token[0].Token);

            }
        }

        private async UniTaskVoid Skill1CoolTime(float collTime)
        {
            if (animator == null || monsterStat.skill1 == null || token[1] == null)
            {
                await UniTask.WaitUntil(() => !skill1Active, cancellationToken: token[1].Token);
                await UniTask.Delay(TimeSpan.FromSeconds(collTime), cancellationToken: token[1].Token);
                await UniTask.WaitUntil(() => !isAttacking || !skill2Active, cancellationToken: token[1].Token);
                skill1Active = true;
                animator.Play("SKILL1",0, 0f);
                Debug.LogError("Skill1Attack");
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.skill1.clip.length), cancellationToken: token[1].Token);
                skill2Active = false;
            }
            else
            {
                skill1Active = false;
            }
        }
        //배열형식으로 사용
        // Stopcoroutine을 사용해야할때 token사용해야됨
        private async UniTaskVoid Skill2CoolTime(float collTime)
        {
            if (monsterStat.skill2 != null || monsterStat.skill2 == null || token[2] == null)
            {
                while (true)
                {
                    await UniTask.WaitUntil(() => !skill2Active, cancellationToken: token[2].Token);
                    await UniTask.Delay(TimeSpan.FromSeconds(collTime), cancellationToken: token[2].Token);
                    await UniTask.WaitUntil(() => !isAttacking || !skill1Active, cancellationToken: token[2].Token);
                    skill2Active = true;
                    animator.Play("SKILL2", 0, 0f);
                    Debug.LogError("SKILL2Attack");
                    await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.skill2.clip.length), cancellationToken: token[2].Token);
                    skill2Active = false;
                }
            }
            else
            {
                skill2Active = false;
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

            Debug.LogError($"죽음");
            if (token != null)
            {
                for (int i = 0; i < token.Length; i++)
                {
                    if (token[i] != null)
                        token[i].Cancel();
                }
            }
            

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

            if (currentState == MonsterState.ATTACK && curState != currentState)
            {
                for (int i = 0; i < token.Length; i++)
                {
                    if (token[i] != null)
                    {
                        token[i].Cancel();
                        token[i].Dispose();
                    }
                }
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