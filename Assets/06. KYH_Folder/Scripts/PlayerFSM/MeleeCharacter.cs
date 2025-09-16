using JHT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class MeleeCharacter : BaseCharacterFSM
{
    private Transform target;
    private Coroutine findTargetRoutine;
    private Coroutine moveRoutine;
    private Animator animator;
    private SPUM_Prefabs spum;

    private List<Vector3> currentPath;

    private float skillCooldown = 8f;
    private float lastSkillTime = -999f;
    private bool isSkillReady => Time.time >= lastSkillTime + skillCooldown;

  //  protected override void Awake()
  //  {
  //      animator = GetComponentInChildren<Animator>();
  //      if (animator == null)
  //          Debug.LogError("[MeleeCharacter] Animator가 연결되지 않았습니다.");
  //
  //      spum = GetComponent<SPUM_Prefabs>();
  //      if (spum == null)
  //          Debug.LogError("[MeleeCharacter] SPUM_Prefabs 컴포넌트가 없습니다.");
  //  }
    protected override void Start()
    {
        base.Start();
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
       // PlayIdleAnim(); // 추가

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
       // PlayMoveAnim(); // 추가

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

        if (currentPath == null || currentPath.Count == 0)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            Vector3 destination = target.position - dir * PartyManager.Instance.attackRange;

            currentPath = AStarPathfinding.Instance.FindPath(transform.position, destination);

            if (currentPath != null && currentPath.Count > 0)
            {
                if (moveRoutine != null)
                    StopCoroutine(moveRoutine);

                moveRoutine = StartCoroutine(FollowPath());
            }
        }

        if (IsTargetInAttackRange())
            ChangeState(State.Attack);
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

                float moveSpeed = PartyManager.Instance.moveSpeed;
                transform.position = Vector3.MoveTowards(transform.position, fixedTargetPos, moveSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

                yield return null;
            }
        }

        currentPath = null;

        if (IsTargetValid())
        {
            if (IsTargetInAttackRange())
                ChangeState(State.Attack);
            else
                ChangeState(State.Move);
        }
        else
        {
            ChangeState(State.Idle);
        }
    }

    protected override void HandleAttack()
    {
      //  PlayAttackAnim(); // 추가

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

        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackRoutine());
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

            float attackRange = PartyManager.Instance.attackRange;
            float attackDelay = stats.GetCurrentStat(Stat.AttackSpeed);
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                ChangeState(State.Move);
                yield break;
            }

            Debug.Log($"{gameObject.name}이(가) 근접 공격!");

            var targetScript = target.GetComponent<JHT_BaseMonsterFSM>();
            if (targetScript != null)
            {
                float attackPower = stats.GetCurrentStat(Stat.Attack);
                targetScript.TakeDamage(attackPower);
            }

            yield return new WaitForSeconds(attackDelay);
        }
    }

    protected override void HandleSkill()
    {
        // PlaySkillAnim(); // 추가

        if (attackRoutine == null)
            attackRoutine = StartCoroutine(SkillRoutine());
    }

    private IEnumerator SkillRoutine()
    {
        Debug.Log($"{gameObject.name} 스킬 발동!");

        if (IsTargetValid())
        {
            var targetScript = target.GetComponent<JHT_BaseMonsterFSM>();
            if (targetScript != null)
            {
                float skillDamage = stats.GetCurrentStat(Stat.Attack) * 2f;
                targetScript.TakeDamage(skillDamage);
                Debug.Log($"스킬 공격: {targetScript.monsterSO.name}에게 {skillDamage} 피해!");
            }
        }

        lastSkillTime = Time.time;

        yield return new WaitForSeconds(1.5f);

        attackRoutine = null;
        ChangeState(State.Attack);
    }

    private void StartFindTargetLoop()
    {
        if (findTargetRoutine != null)
            StopCoroutine(findTargetRoutine);

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

    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
            return;

        float minDist = float.MaxValue;
        GameObject closest = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        if (closest != null)
        {
            target = closest.transform;
            Debug.Log($"[FindClosestEnemy] 타겟 설정됨: {target.name}");
        }
    }

    private bool IsTargetValid()
    {
        bool valid = target != null && !target.Equals(null) &&
                     target.GetComponent<JHT_BaseMonsterFSM>()?.curHP > 0;

        if (!valid)
        {
            currentPath = null;
            target = null;
        }

        return valid;
    }

    private bool IsTargetInAttackRange()
    {
        if (!IsTargetValid()) return false;

        float attackRange = PartyManager.Instance.attackRange;
        float distance = Vector3.Distance(transform.position, target.position);

        return distance <= attackRange + 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float attackRange = (PartyManager.Instance != null)
            ? PartyManager.Instance.attackRange
            : 1f;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void PlayIdleAnim()
    {
        if (spum.IDLE_List != null && spum.IDLE_List.Count > 0)
            animator.Play("IDLE"); // "0_idle"
    }

    private void PlayMoveAnim()
    {
        if (spum.MOVE_List != null && spum.MOVE_List.Count > 0)
            animator.Play("MOVE"); // "0_move"
    }

    private void PlayAttackAnim()
    {
        if (spum.ATTACK_List != null && spum.ATTACK_List.Count > 0)
            animator.Play("ATTACK"); // "0_Attack_Normal"
    }

    private void PlaySkillAnim()
    {
        if (spum.ATTACK_List != null && spum.ATTACK_List.Count > 1)
            animator.Play("ATTACK"); // "1_Skill_Normal"
    }
}
