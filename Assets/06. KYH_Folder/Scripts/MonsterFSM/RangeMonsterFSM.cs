using UnityEngine;
using System.Collections;
public class RangeMonsterFSM : BaseMonsterFSM
{
    public Transform target;
    public GameObject projectilePrefab;
    public Transform firePoint;

    protected override void Start()
    {
        base.Start();

        FindClosestCrew();
    }

    protected override void HandleIdle()
    {
        if (target == null)
        {
            FindClosestCrew(); // 실시간 타겟 탐색
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
                ChangeState(State.Move); // 사거리 벗어나면 다시 이동
                yield break; // 코루틴 종료
            }

            Debug.Log($"{gameObject.name}이(가) 원거리 투사체 발사!");

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // 안전하게 Projectile 컴포넌트 확인
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Init(target.position);

            yield return new WaitForSeconds(attackDelay);
        }
    }

    private void FindClosestCrew()
    {
        GameObject[] crews = GameObject.FindGameObjectsWithTag("Crew");
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
            FindClosestCrew();
    }
}
