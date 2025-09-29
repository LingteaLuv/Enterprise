using JHT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 근접 캐릭터 FSM (BaseCharacterFSM 상속)
/// - A* 경로 기반 이동
/// - 일반 공격 및 스킬 처리
/// - FSM 상태 전환 및 대상 탐색 루프 포함
/// </summary>
public class MeleeCharacter : BaseCharacterFSM
{
    // 타겟 Transform (현재 추적 중인 적)
    private Transform target;

    // 코루틴 핸들 (중복 방지용)
    private Coroutine findTargetRoutine;
    private Coroutine moveRoutine;

    // 애니메이터 및 SPUM 캐릭터 애니메이션 관리
    
    private SPUM_Prefabs spum;

    // 현재 이동 경로 (A* 결과)
    private List<Vector3> currentPath;

    // 스킬 쿨다운 관련
    private float skillCooldown;
    private float lastSkillTime = -999f;
    private bool isSkillReady => Time.time >= lastSkillTime + skillCooldown;

    // 경로 재탐색 쿨타임 관련
    private float repathCooldown = 1f;
    private float lastRepathTime = -999f;

    // 외부 스킬 사용 알림용 이벤트
    public event System.Action<SkillSO> OnSkillUsed;

    protected override void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("[MeleeCharacter] Animator가 연결되지 않았습니다.");

        spum = GetComponentInChildren<SPUM_Prefabs>();
        if (spum == null)
            Debug.LogError("[MeleeCharacter] SPUM_Prefabs 컴포넌트가 없습니다.");

        StartFindTargetLoop();
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        StartFindTargetLoop();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopFindTargetLoop();
    }

    // Idle 상태 처리
    protected override void HandleIdle()
    {
        animator.SetBool("0_Idle", false);
        UpdateLookByTarget();

        if (!IsTargetValid()) return;

        if (isSkillReady && IsTargetInAttackRange())
        {
            ChangeState(State.Skill);
            return;
        }

        if (IsTargetInAttackRange())
            ChangeState(State.Attack);
        else
            ChangeState(State.Move);
    }

    // Move 상태 처리
    protected override void HandleMove()
    {
        animator.SetBool("1_Move", true);
        UpdateLookByTarget();

        if (!IsTargetValid())
        {
            ChangeState(State.Idle);
            return;
        }

        if (isSkillReady && IsTargetInAttackRange())
        {
            ChangeState(State.Skill);
            return;
        }

        if (Time.time >= lastRepathTime + repathCooldown)
        {
            RecalculatePathToTarget();
            lastRepathTime = Time.time;
        }

        if (IsTargetInAttackRange())
        {
            ChangeState(State.Attack);
            return;
        }
    }

    // Attack 상태 처리
    protected override void HandleAttack()
    {
        if (attackRoutine == null && currentState == State.Attack)
        {
            UpdateLookByTarget();
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // Skill 상태 처리
    protected override void HandleSkill()
    {
        if (attackRoutine == null && currentState == State.Skill)
        {
            Debug.Log("스킬 사용 루틴 진입");
            UpdateLookByTarget();
            attackRoutine = StartCoroutine(SkillRoutine());
        }
    }

    // 일반 공격 루틴 (지속)
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!IsTargetValid())
            {
                ChangeState(State.Idle);
                yield break;
            }

            if (!IsTargetInAttackRange())
            {
                ChangeState(State.Move);
                yield break;
            }

            animator.SetTrigger("8_normal"); // 일반 공격 애니메이션

            var targetScript = target.GetComponent<JHT_BaseMonsterFSM>();
            if (targetScript != null)
            {
                float attackPower = stats.GetCurrentStat(Stat.Attack);
                targetScript.TakeDamage(stats); // 데미지 적용
            }

            float delay = 1f / stats.GetCurrentStat(Stat.AttackSpeed);
            yield return new WaitForSeconds(delay);
        }
    }

    // 스킬 루틴 (단발성)
    private IEnumerator SkillRoutine()
    {
        var skill = stats.skills.FirstOrDefault();
        if (skill == null || !IsTargetValid())
        {
            ChangeState(State.Idle);
            yield break;
        }

        animator.SetTrigger("2_Attack");

        var primaryTarget = target.GetComponent<IDamageable>();
        if (primaryTarget != null)
        {
            skill.Use(stats, primaryTarget);
            OnSkillUsed?.Invoke(skill);
            Debug.Log($"[SkillRoutine] {gameObject.name} 가 {primaryTarget} 에게 스킬 피해를 입혔습니다.");
        }

        lastSkillTime = Time.time;
        yield return new WaitForSeconds(1.5f); // 스킬 연출 대기

        attackRoutine = null;

        // 상태 복귀
        if (!IsTargetValid()) ChangeState(State.Idle);
        else if (IsTargetInAttackRange()) ChangeState(State.Attack);
        else ChangeState(State.Move);
    }

    // 타겟 방향에 따라 좌우 반전
    private void UpdateLookByTarget()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        transform.localScale = (dir.x > 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    private void UpdateLookByMovement(Vector3 moveDir)
    {
        if (moveDir.x == 0) return;
        transform.localScale = (moveDir.x > 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    // 목표까지 경로 재계산 (A* 사용)
    private void RecalculatePathToTarget()
    {
        if (!IsTargetValid()) return;

        int maxAttempts = 3;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            float buffer = 0.2f;
            float attackRange = PartyManager.Instance.attackRange;
            Vector3 destination = target.position - dir * (attackRange - buffer);

            currentPath = AStarPathfinding.Instance.FindPath(transform.position, destination);

            if (currentPath != null && currentPath.Count > 0)
            {
                if (moveRoutine != null) StopCoroutine(moveRoutine);
                moveRoutine = StartCoroutine(FollowPath());
                return;
            }

            attempts++;
            if (!FindNextBestEnemy())
            {
                ChangeState(State.Idle);
                return;
            }
        }

        ChangeState(State.Idle);
    }

    // 대상 자동 탐색 루프 시작
    private void StartFindTargetLoop()
    {
        if (findTargetRoutine != null) StopCoroutine(findTargetRoutine);
        findTargetRoutine = StartCoroutine(FindTargetLoop());
    }

    private void StopFindTargetLoop()
    {
        if (findTargetRoutine != null)
        {
            StopCoroutine(findTargetRoutine);
            findTargetRoutine = null;
        }
    }

    // 가장 가까운 적 지속 탐색
    private IEnumerator FindTargetLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        while (true)
        {
            if (!IsTargetValid())
            {
                FindClosestEnemy();
                if (IsTargetValid() && currentState == State.Idle)
                {
                    currentPath = null;
                    ChangeState(State.Move);
                }
            }
            yield return wait;
        }
    }

    // A* 경로 따라 이동
    private IEnumerator FollowPath()
    {
        foreach (var targetPos in currentPath)
        {
            Vector3 fixedTargetPos = new Vector3(targetPos.x, targetPos.y, 0f);

            while (Vector2.Distance(transform.position, fixedTargetPos) > 0.1f)
            {
                if (!IsTargetValid())
                {
                    ChangeState(State.Idle);
                    currentPath = null;
                    yield break;
                }

                if (IsTargetInAttackRange())
                {
                    ChangeState(State.Attack);
                    currentPath = null;
                    yield break;
                }

                Vector3 moveDir = (fixedTargetPos - transform.position).normalized;
                UpdateLookByMovement(moveDir);

                float moveSpeed = PartyManager.Instance.moveSpeed;
                transform.position = Vector3.MoveTowards(
                    transform.position, fixedTargetPos,
                    moveSpeed * Time.deltaTime
                );

                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                yield return null;
            }
        }

        currentPath = null;

        if (IsTargetValid())
        {
            if (IsTargetInAttackRange())
            {
                ChangeState(State.Attack);
            }
            else
            {
                Vector3 fixedTargetPos = target.position;

                Vector3 moveDir = (fixedTargetPos - transform.position).normalized;
                UpdateLookByMovement(moveDir);

                float moveSpeed = PartyManager.Instance.moveSpeed;
                transform.position = Vector3.MoveTowards(
                    transform.position, fixedTargetPos,
                    moveSpeed * Time.deltaTime
                );

                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                yield return null;
                ChangeState(State.Move);
            }
        }
        else
            ChangeState(State.Idle);
    }

    // 가장 가까운 적 탐색
    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0) return;

        float minDist = float.MaxValue;
        Transform best = null;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            var monster = e.GetComponent<JHT_BaseMonsterFSM>();
            if (monster == null || monster.CurHP <= 0) continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = e.transform;
            }
        }

        target = best;
    }

    // 현재 타겟 외에 새로운 적 탐색
    private bool FindNextBestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0) return false;

        float minDist = float.MaxValue;
        Transform best = null;

        foreach (var e in enemies)
        {
            if (e == null || e.transform == target) continue;

            var monster = e.GetComponent<JHT_BaseMonsterFSM>();
            if (monster == null || monster.CurHP <= 0) continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = e.transform;
            }
        }

        target = best;
        return target != null;
    }

    // 타겟 유효성 검사
    private bool IsTargetValid()
    {
        bool valid = target != null && !target.Equals(null) &&
                     target.GetComponent<JHT_BaseMonsterFSM>()?.CurHP > 0;

        if (!valid)
        {
            currentPath = null;
            target = null;
            return false;
        }
        return true;
    }

    // 공격 사거리 내에 있는지 확인
    private bool IsTargetInAttackRange()
    {
        if (!IsTargetValid()) return false;
        float attackRange = PartyManager.Instance.attackRange;
        return Vector3.Distance(transform.position, target.position) <= attackRange + 0.1f;
    }

    // 사거리 시각화 (디버깅용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float attackRange = PartyManager.Instance?.attackRange ?? 1f;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
