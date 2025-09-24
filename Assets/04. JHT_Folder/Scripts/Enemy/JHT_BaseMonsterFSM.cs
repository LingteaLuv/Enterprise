using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;



namespace JHT
{

    public abstract class JHT_BaseMonsterFSM : JHT_PooledObject, IAttacker, IDamageable
    {
        public enum MonsterState { IDLE, MOVE, ATTACK, DEATH, SKILL1, SKILL2, STUN }

        [Header("State")]
        public MonsterState currentState;
        public JHT_StateMachine stateMachine;
        public MonstserSearch monsterSearch;

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
        public bool isStun;

        private CancellationTokenSource[] token;

        private float curHP;
        public float CurHP { get { return curHP; } set { curHP = value; OnChangeHp?.Invoke(curHP); } }
        public Action<float> OnChangeHp;

        [SerializeField] private List<Buff> activeBuffs = new List<Buff>();

        #region Animator

        [Header("Anim")]
        public Animator animator;

        public readonly int IDLE = Animator.StringToHash("IDLE");
        public readonly int MOVE = Animator.StringToHash("MOVE");
        public readonly int ATTACK = Animator.StringToHash("Monster_ATTACK");
        public readonly int SKILL1 = Animator.StringToHash("Monster_SKILL1");
        public readonly int SKILL2 = Animator.StringToHash("Monster_SKILL2");
        public readonly int STUN = Animator.StringToHash("STUN");
        public readonly int DEATH = Animator.StringToHash("DEATH");
        #endregion


        public virtual void Init(JHT_BaseMonsterStat stat)
        {
            monsterSearch = GetComponent<MonstserSearch>();
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

            
            curHP = monsterStat.monsterStats[Stat.Health];

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
            attackDelayCount = monsterStat.normalSkill.coolTime - monsterStat.normalSkill.clip.length > 0 ?
                monsterStat.normalSkill.coolTime - monsterStat.normalSkill.clip.length : 1;

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
            stateMachine.stateDic.Add(MonsterState.STUN, new Monster_Stun(this));

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
            token = new CancellationTokenSource[5];

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
                        monsterSearch.target = target.transform;
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
                _ = Skill1CoolTime(monsterStat.skill1.coolTime);
            
            if (monsterStat.skill2 != null) 
                _ = Skill2CoolTime(monsterStat.skill2.coolTime);
        }

        public virtual void HandleMove()
        {
            if (target == null)
                return;
            ChangeAnim(MOVE, MonsterState.MOVE);

        }

        public virtual void HandleStun()
        {
            ChangeAnim(STUN, MonsterState.STUN);
        }

        public virtual void HandleDie()
        {
            Die();
        }

        //스턴 기능 호출만 하면 됩니다.
        public void ApplyStun(float duration)
        {
            isStun = true;
            _ = StunSetting(duration);
            ResetTokens();
        }

        private async UniTaskVoid StunSetting(float duration)
        {
            if (token[4] == null || token[4].Token.IsCancellationRequested)
                return;

            await UniTask.Delay(TimeSpan.FromSeconds(duration),cancellationToken: token[4].Token);
            isStun = false;
        }

        private async UniTaskVoid Attack(float coolTime)
        {
            skill1Active = false;
            skill2Active = false;

            while (true)
            {
                if (token[0] == null || token[0].Token.IsCancellationRequested) 
                    return;

                if (!animator)
                    return;

                await UniTask.WaitUntil(() => !skill1Active || !skill2Active, cancellationToken: token[0].Token);

                if (animator != null)
                {
                    animator.Play(ATTACK, 0, 0f);
                }
                isAttacking = true;
                
                await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.normalSkill.clip.length), cancellationToken: token[0].Token);

                isAttacking = false;
                await UniTask.Delay(TimeSpan.FromSeconds(coolTime), cancellationToken: token[0].Token);

            }
        }

        private async UniTaskVoid Skill1CoolTime(float collTime)
        {
            if (animator != null && monsterStat.skill1 != null && token[1] != null)
            {
                while (true)
                {
                    await UniTask.WaitUntil(() => !skill1Active, cancellationToken: token[1].Token);
                    await UniTask.Delay(TimeSpan.FromSeconds(collTime), cancellationToken: token[1].Token);
                    await UniTask.WaitUntil(() => !isAttacking || !skill2Active, cancellationToken: token[1].Token);

                    skill1Active = true;

                    if (animator != null)
                        animator.Play(SKILL1, 0, 0f);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.skill1.clip.length), cancellationToken: token[1].Token);
                    skill1Active = false;
                }
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
            if (monsterStat.skill2 != null && monsterStat.skill2 != null && token[2] != null)
            {
                while (true)
                {
                    await UniTask.WaitUntil(() => !skill2Active, cancellationToken: token[2].Token);
                    await UniTask.Delay(TimeSpan.FromSeconds(collTime), cancellationToken: token[2].Token);
                    await UniTask.WaitUntil(() => !isAttacking || !skill1Active, cancellationToken: token[2].Token);

                    skill2Active = true;
                    if (animator != null)
                        animator.Play(SKILL2, 0, 0f);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(monsterStat.skill2.clip.length), cancellationToken: token[2].Token);
                    skill2Active = false;
                }
            }
            else
            {
                skill2Active = false;
            }
        }

        // 버프 적용시 넣을 함수
        public void ApplyBuff(Stat stat, float value, float duration, BuffType buffType)
        {
            bool isSynergy = SynergyManager.IsApplyingSynergy;
            Buff newBuff = new Buff(stat, value, duration, buffType, isSynergy, BuffEffectType.StatModifier);
            activeBuffs.Add(newBuff);
            _ = Debuff(stat, duration, value, newBuff);

        }

        public async UniTask Debuff(Stat stat, float duration, float amount, Buff newBuff)
        {
            if (stat == Stat.Health)
            {
                CurHP += CurHP * (amount / 100);
            }

            this.monsterStat.monsterStats[stat] += this.monsterStat.monsterStats[stat] * (amount / 100);
            // 이펙트 여기에 추가
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token[5].Token);
            //이펙트 해제
            this.monsterStat.monsterStats[stat] -= this.monsterStat.monsterStats[stat] * (amount * 100);

            if (stat == Stat.Health)
            {
                CurHP -= CurHP * (amount / 100);
            }

            activeBuffs.Remove(newBuff);
        }

        public void Rotate()
        {
            if (target == null || monsterUI == null) return;

            bool targetOnRight = target.transform.position.x > transform.position.x;

            transform.localRotation = Quaternion.Euler(0f, targetOnRight ? 180f : 0f, 0f);

            var ui = monsterUI.GetComponent<RectTransform>();
            ui.localScale = new Vector3(targetOnRight ? -1f : 1f, 1f, 1f);
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
            monsterSearch.StopRoutine();

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

            float percent = curHP / (float)monsterStat.monsterStats[Stat.Health];
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
            float baseValue = 0;
            switch (stat)
            {
                case Stat.Attack: baseValue = monsterStat.monsterStats[Stat.Attack]; break;
                case Stat.Health: baseValue = monsterStat.monsterStats[Stat.Health]; break;
                case Stat.Defense: baseValue = monsterStat.monsterStats[Stat.Defense]; break;
                case Stat.CritChance: baseValue = monsterStat.monsterStats[Stat.CritChance]; break;
                case Stat.CritDamage: baseValue = monsterStat.monsterStats[Stat.CritDamage]; break;
                case Stat.AttackSpeed: baseValue = monsterStat.monsterStats[Stat.AttackSpeed]; break;
            }
            var statBuffs = activeBuffs.Where(b => b.EffectType == BuffEffectType.StatModifier && b.Stat == stat);
            float flatBonus = statBuffs.Where(b => b.BuffType == BuffType.Flat).Sum(b => b.Value);
            float percentBonus = statBuffs.Where(b => b.BuffType == BuffType.Percent).Sum(b => b.Value);
            float finalValue = (baseValue + flatBonus) * (1 + percentBonus);

            return finalValue;
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