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

    protected override void HandleAttack()
    {
        if (attackRoutine == null && currentState == State.Attack)
        {
            UpdateLookByTarget();
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    protected override void HandleSkill()
    {
        Debug.Log("스킬 사용 루틴 진입");
        if (attackRoutine == null && currentState == State.Skill)
        {
            UpdateLookByTarget();
            attackRoutine = StartCoroutine(SkillRoutine());
        }
    }

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

            animator.SetTrigger("8_normal");

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

        animator.SetTrigger("2_Attack");

        var primaryTarget = target.GetComponent<IDamageable>();

        if (primaryTarget != null)
        {
            skill.Use(stats, primaryTarget);
            OnSkillUsed?.Invoke(skill);

            Debug.Log($"[SkillRoutine] {gameObject.name} 가 {primaryTarget} 에게 스킬 피해 을(를) 입혔습니다.");
        }

        lastSkillTime = Time.time;

        yield return new WaitForSeconds(1.5f);

        attackRoutine = null;

        if (!IsTargetValid()) ChangeState(State.Idle);
        else if (IsTargetInAttackRange()) ChangeState(State.Attack);
        else ChangeState(State.Move);
    }

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
                UpdateLookByMovement(moveDir); // ← 이동 방향으로 바라보도록 처리

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
