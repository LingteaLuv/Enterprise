using JHT;
using System.Collections;
using System.Linq;
using UnityEngine;
public class MeleeCharacter : BaseCharacterFSM
{
    private Transform target;
    private Coroutine findTargetRoutine;

    //-----------
    // 차후 수현님이 연결 할 케릭터에서 관리 될 변수들 목록 예시 -- 이 스크립트에서가 아닌 다른곳에서 참조하여 사용해도 됩니다.
    // HandleSkill 내의 SkillRoutine 매서드 내에서 작업
    private float skillCooldown = 8f; // 쿨타임 간격 (스탯에서 가져와도 됨)
    private float lastSkillTime = -999f;
    
    private bool isSkillReady => Time.time >= lastSkillTime + skillCooldown;
    //-----------

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
        if (isSkillReady)
        {
            ChangeState(State.Skill);
            return;
        }

        if (!IsTargetValid()) return;

        if (IsTargetInAttackRange())
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

        if (isSkillReady && IsTargetInAttackRange())
        {
            ChangeState(State.Skill);
            return;
        }

        float moveSpeed = stats.moveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (IsTargetInAttackRange())
            ChangeState(State.Attack);
    }

    protected override void HandleAttack()
    {
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

            float attackRange = stats.attackRange;
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
                //Debug.Log($"MeleeCharacter AttackRoutine : {targetScript.monsterSO.name}");
            }
            // todo : Attack Delay에 따라 () 내부 시간 연산, 대입
            yield return new WaitForSeconds(2f);
        }
    }

    protected override void HandleSkill()
    {
        if (attackRoutine == null)
            attackRoutine = StartCoroutine(SkillRoutine());
    }

    // 루틴 내부에서 스킬에 관련된 이펙트,애니메이션 등등의 세부적인 부분 불러오면 됨
    private IEnumerator SkillRoutine()
    {
        // 애니메이션 재생 (있다면)
        // animator?.SetTrigger("Skill");

        // 스킬 이펙트, 데미지, 상태 이상 등 구현
        var skill = stats.skills.FirstOrDefault();

        if (skill == null)
        {
            Debug.LogWarning("사용할 스킬이 없어 Idle 상태로 돌아갑니다.");
            ChangeState(State.Idle);
            yield break;
        }

        Debug.Log($"{stats.charName}이/가 {skill.skillName} 스킬을 발동");
        skill.Use(stats);

        // 쿨타임 초기화
        lastSkillTime = Time.time;

        yield return new WaitForSeconds(1.5f); // 스킬 모션 시간 등

        ChangeState(State.Idle); // 스킬 후 상태 복귀
    }

    //  타겟 감시 루프 (null되면 다시 탐색)
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

    private bool IsTargetInAttackRange()
    {
        if (!IsTargetValid()) return false;

        float attackRange = PartyManager.Instance.attackRange;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= attackRange;
    }
}
