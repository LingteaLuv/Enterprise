using UnityEngine;
using System.Collections;
public class MeleeCharacter : BaseCharacterFSM
{
    public Transform target;

    protected override void Start()
    {
        base.Start();
        FindClosestEnemy();
    }

    protected override void HandleIdle()
    {
        if (target == null)
        {
            FindClosestEnemy();
            return;
        }

        float attackRange = stats.GetCurrentStat(Stat.AttackRange);
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < attackRange)
            ChangeState(State.Attack);
        else
            ChangeState(State.Move);
    }

    protected override void HandleMove()
    {
        float moveSpeed = stats.GetCurrentStat(Stat.MoveSpeed);
        float attackRange = stats.GetCurrentStat(Stat.AttackRange);
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
            if (target == null)
            {
                ChangeState(State.Idle);
                yield break;
            }

            float attackRange = stats.GetCurrentStat(Stat.AttackRange);
            float attackDelay = stats.GetCurrentStat(Stat.AttackSpeed); // 공격속도 → 딜레이 계산은 별도

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                ChangeState(State.Move);
                yield break;
            }

            Debug.Log($"{gameObject.name}이(가) 근접 공격!");

            var targetScript = target.GetComponent<BaseMonsterFSM>();
            if (targetScript != null)
            {
                float attackPower = stats.GetCurrentStat(Stat.Attack);
                targetScript.TakeDamage(attackPower);
            }

            yield return new WaitForSeconds(attackDelay); // 공격 속도에 따라 딜레이
        }
    }

    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDist = float.MaxValue;
        GameObject closest = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        if (closest != null)
            target = closest.transform;
    }
}
