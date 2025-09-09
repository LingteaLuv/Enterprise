using JHT;
using UnityEngine;

namespace JHT
{
    public class JHT_NormalMonster : JHT_BaseMonsterFSM
    {

        protected override void Start()
        {
            base.Start();
        }

        public override void Init(JHT_MonsterDataSO so)
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
            base.HandleAttack();
        }

        protected override void HandleMove()
        {
            base.HandleMove();
        }

        protected override void HandleIdle()
        {
            base.HandleIdle();
        }

        // Attack애니메이션 이벤트로 실행할 함수
        public void NormalMonsterAttack()
        {
            Debug.LogError("Attack!");
            // 근접 공격일경우
            if (monsterStat.monsterType == MonsterType.close)
            {
                //if (monsterStat.attackAngle == 0)
                //    return;
                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.attackRange, targetLayer);

                foreach (var c in cols)
                {
                    //일단은 일방적인 플레이어 스크립트
                    BaseCharacterFSM target = c.GetComponent<BaseCharacterFSM>();

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
                obj.Init(target.position, transform.position, monsterStat.attackSpeed, monsterStat.attackPower,monsterSO);
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
