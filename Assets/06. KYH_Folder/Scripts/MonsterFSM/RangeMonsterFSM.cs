using UnityEngine;
using System.Collections;
public class RangeMonsterFSM : BaseMonsterFSM
{
    public Transform target;
    public GameObject projectilePrefab;
    public Transform firePoint;

    protected override void HandleIdle()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange)
        {
            ChangeState(State.Attack);
        }
        else
        {
            ChangeState(State.Move);
        }
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
            Debug.Log($"{gameObject.name}이(가) 원거리 투사체 발사!");

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Init(target.position); // 추적 발사

            yield return new WaitForSeconds(attackDelay);
        }
    }
}
