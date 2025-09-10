using UnityEngine;
using System.Collections;
using JHT;
public class MeleeCharacter : BaseCharacterFSM
{
    private Transform target;
    private Coroutine findTargetRoutine;

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
        if (!IsTargetValid())
            return; // 타겟 탐색은 코루틴에서 자동 실행 중

        float attackRange = PartyManager.Instance.attackRange;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < attackRange)
            ChangeState(State.Attack);
        else
            ChangeState(State.Move);
    }

    protected override void HandleMove()
    {
        if (!IsTargetValid())
        {
            ChangeState(State.Idle);
            return;
        }

        float moveSpeed = PartyManager.Instance.moveSpeed;
        float attackRange = PartyManager.Instance.attackRange;
        float distance = Vector3.Distance(transform.position, target.position);

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (distance <= attackRange)
        {
            ChangeState(State.Attack);
        }
    }

    protected override void HandleAttack()
    {
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
                Debug.Log($"MeleeCharacter AttackRoutine : {targetScript.monsterSO.name}");
            }

            yield return new WaitForSeconds(5f);
        }
    }

    // 🟡 타겟 감시 루프 (null되면 다시 탐색)
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
                FindClosestEnemy();

            yield return wait;
        }
    }

    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            return;
        }

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
        return target != null && !target.Equals(null) && target.GetComponent<JHT_BaseMonsterFSM>().curHP > 0;
    }
}
