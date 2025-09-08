using JHT;
using UnityEngine;

public class JHT_NormalMonster : JHT_BaseMonsterFSM
{

    [SerializeField] private JHT_MonsterProjectile poolPrefab;
    JHT_ObjectPool pool;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();


        if (monsterStat.type == MonsterType.open)
        {
            poolPrefab = monsterStat.curSO.projectile;
            GameObject parent = new GameObject($"{poolPrefab.name} Pool_Parent");
            parent.transform.SetParent(this.transform);

            pool = new(poolPrefab, parent.transform, 10);
        }
        //JHT_PoolManager.Instance.PoolInit(monsterStat.curSO.particle, 10); // 풀 -> T로
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleAttack()
    {
        //애니메이션 실행(이벤트 사용할거임)
        NormalMonsterAttack();
    }

    protected override void HandleChase()
    {
        base.HandleChase();
    }

    protected override void HandleIdle()
    {
        
    }

    protected override void HandleMove()
    {
        
    }

    // Attack애니메이션 이벤트로 실행할 함수
    public void NormalMonsterAttack()
    {
        // 근접 공격일경우
        if(monsterStat.type == MonsterType.close)
        {
            //if (monsterStat.attackAngle == 0)
            //    return;
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.attackRange, targetLayer);

            foreach (var c in cols)
            {
                //일단은 일방적인 플레이어 스크립트
                BaseCharecterFSM target = c.GetComponent<BaseCharecterFSM>();

                if (target != null)
                {
                    //Pool 파티클 사용
                    target.TakeDamage(monsterStat.attackPower);
                }
            }
        }
        else //원거리 일경우
        {
            JHT_MonsterProjectile obj = pool.GetPooled() as JHT_MonsterProjectile;
            obj.Init(target.position, transform.position, monsterStat.attackSpeed, monsterStat.attackPower);
        }
    }
}
