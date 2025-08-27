using UnityEngine;
using System.Collections;
public class MeleeCharacter : BaseCharecterFSM
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
            FindClosestEnemy(); // 실시간 타겟 탐색
            return; // 한 프레임 쉬고 다음 프레임부터 다시 체크
        }

        if (Vector3.Distance(transform.position, target.position) < attackRange)
            ChangeState(State.Attack);
        else
            ChangeState(State.Move);
    }

    protected override void HandleMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
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

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                ChangeState(State.Move); // 사거리 밖이면 다시 이동 상태로
                yield break; // 코루틴 종료
            }

            Debug.Log($"{gameObject.name}이(가) 근접 공격!");

            // 실제 공격 처리
            var targetScript = target.GetComponent<BaseMonsterFSM>();
            if (targetScript != null)
            {
                targetScript.TakeDamage(attack); // 데미지 전달
            }

            yield return new WaitForSeconds(attackDelay);
        }
    }

    private void FindClosestEnemy()
    {
        GameObject[] crews = GameObject.FindGameObjectsWithTag("Enemy");
        float minDist = float.MaxValue;
        GameObject closest = null;

        foreach (GameObject crew in crews)
        {
            float dist = Vector3.Distance(transform.position, crew.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = crew;
            }
        }

        if (closest != null)
            target = closest.transform;
    }

    public override void TargetFind()
    {
        if (target == null)
            FindClosestEnemy();
    }
}
