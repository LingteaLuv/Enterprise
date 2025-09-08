using JHT;
using UnityEngine;

namespace JHT
{
    public class JHT_NormalMonster : JHT_BaseMonsterFSM
    {

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();


        }

        public override void Init(JHT_BaseMonsterStat so)
        {
            base.Init(so);
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
            if (monsterStat.monsterType == MonsterType.close)
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
                JHT_MonsterProjectile obj = JHT_MonsterSpawnManager.Instance.projectilePool.GetPooled() as JHT_MonsterProjectile;
                obj.Init(target.position, transform.position, monsterStat.attackSpeed, monsterStat.attackPower,monsterStat);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (monsterStat != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, monsterStat.attackRange);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, monsterStat.chaseRange);
            }
        }
    }
}
