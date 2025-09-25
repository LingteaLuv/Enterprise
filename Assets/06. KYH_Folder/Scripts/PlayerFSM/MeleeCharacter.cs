using JHT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeCharacter : BaseCharacterFSM
{
    private Transform target;
    private Coroutine findTargetRoutine;
    private Coroutine moveRoutine;
    private Animator animator;
    private SPUM_Prefabs spum;
    private List<Vector3> currentPath;

    private float skillCooldown;
    private float lastSkillTime = -999f;
    private bool isSkillReady => Time.time >= lastSkillTime + skillCooldown;

    private float repathCooldown = 1f;
    private float lastRepathTime = -999f;

    public event System.Action<SkillSO> OnSkillUsed;

    protected override void Start()
    {
        base.Start();
        skillCooldown = stats.skills.FirstOrDefault()?.cooldown ?? 5f;

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

    // ---------------- FSM HANDLERS ----------------

    protected override void HandleIdle()
    {
        animator.SetBool("0_Idle", false); // Idle
        UpdateLookDirection();

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

    protected override void HandleMove()
    {
        animator.SetBool("1_Move", true); // Move
        UpdateLookDirection();

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

    protected override void HandleAttack()
    {
        if (attackRoutine == null && currentState == State.Attack)
        {
            UpdateLookDirection();
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    protected override void HandleSkill()
    {
        if (attackRoutine == null && currentState == State.Skill)
        {
            UpdateLookDirection();
            attackRoutine = StartCoroutine(SkillRoutine());
        }
    }

    // ---------------- ROUTINES ----------------

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

            // 사거리 안일 때만 공격
            animator.SetTrigger("2_Attack");

            var targetScript = target.GetComponent<JHT_BaseMonsterFSM>();
            if (targetScript != null)
            {
                float attackPower = stats.GetCurrentStat(Stat.Attack);
                targetScript.TakeDamage(stats);
            }

            float delay = 1f / stats.GetCurrentStat(Stat.AttackSpeed);
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator SkillRoutine()
    {
        var skill = stats.skills.FirstOrDefault();
        if (skill == null || !IsTargetValid())
        {
            ChangeState(State.Idle);
            yield break;
        }

      //  Debug.Log($"{stats.charName}이/가 {skill.skillName} 스킬 발동");

        // 스킬도 Attack 애니메이션 트리거로 처리
        animator.SetTrigger("2_Attack");

        var primaryTarget = target.GetComponent<IDamageable>();
        if (primaryTarget != null)
        {
            skill.Use(stats, primaryTarget);
            OnSkillUsed?.Invoke(skill);
        }

        lastSkillTime = Time.time;

        yield return new WaitForSeconds(1.5f);

        attackRoutine = null;

        if (!IsTargetValid()) ChangeState(State.Idle);
        else if (IsTargetInAttackRange()) ChangeState(State.Attack);
        else ChangeState(State.Move);
    }

    // ---------------- HELPERS ----------------

    private void UpdateLookDirection()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        transform.localScale = (dir.x > 0) ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    private void RecalculatePathToTarget()
    {
        if (!IsTargetValid()) return;

        // 최대 시도 횟수 (예: 3번까지 다른 적 탐색)
        int maxAttempts = 3;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            float buffer = 0.2f;
            float attackRange = PartyManager.Instance.attackRange;
            Vector3 destination = target.position - dir * (attackRange - buffer);

            currentPath = AStarPathfinding.Instance.FindPath(transform.position, destination);

            // 경로를 찾은 경우 → 이동 시작
            if (currentPath != null && currentPath.Count > 0)
            {
                if (moveRoutine != null) StopCoroutine(moveRoutine);
                moveRoutine = StartCoroutine(FollowPath());
                return;
            }

            // 실패 → 차선 타겟 찾기
            attempts++;
            if (!FindNextBestEnemy())
            {
                // 더 이상 다른 적 없음 → Idle 전환
                ChangeState(State.Idle);
                return;
            }
        }

        // 여기까지 왔으면 여러 번 실패 → Idle로 전환
        ChangeState(State.Idle);
    }

    // 타겟 찾기 루프 (변경 없음)
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

    // ---------------- TARGET / CHECKS ----------------
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
            ChangeState(IsTargetInAttackRange() ? State.Attack : State.Move);
        else
            ChangeState(State.Idle);
    }
    /// <summary>
    /// 가장 가까운 살아있는 적 찾기 (LINQ 제거)
    /// </summary>
    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0)
            return;

        float minDist = float.MaxValue;
        Transform best = null;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject e = enemies[i];
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

    /// <summary>
    /// 현재 타겟이 막혔을 때 차선의 타겟 탐색 (LINQ 제거)
    /// </summary>
    private bool FindNextBestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0)
            return false;

        float minDist = float.MaxValue;
        Transform best = null;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject e = enemies[i];
            if (e == null) continue;

            var monster = e.GetComponent<JHT_BaseMonsterFSM>();
            if (monster == null || monster.CurHP <= 0) continue;

            // 현재 타겟은 제외
            if (target != null && e.transform == target) continue;

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

    private bool IsTargetInAttackRange()
    {
        if (!IsTargetValid()) return false;
        float attackRange = PartyManager.Instance.attackRange;
        return Vector3.Distance(transform.position, target.position) <= attackRange + 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float attackRange = PartyManager.Instance?.attackRange ?? 1f;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
