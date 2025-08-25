using UnityEngine;
using System.Collections;
public class MeleeMonsterFSM : BaseMonsterFSM
{
    public Transform target;

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
            Debug.Log($"{gameObject.name}이(가) 근접 공격!");

            // 이펙트, 애니메이션, 데미지 적용 등
            // target.GetComponent<Player>()?.TakeDamage(공격력);

            yield return new WaitForSeconds(attackDelay);
        }
    }
}
