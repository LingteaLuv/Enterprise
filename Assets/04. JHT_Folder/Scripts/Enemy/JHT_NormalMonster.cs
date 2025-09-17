using JHT;
using UnityEngine;

namespace JHT
{
    public class JHT_NormalMonster : JHT_BaseMonsterFSM
    {

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void Init(JHT_BaseMonsterStat stat)
        {
            base.Init(stat);

            if (monsterUI != null)
            {
                monsterUI.gameObject.SetActive(true);
            }

        }
        protected override void Update()
        {
            base.Update();
        }

        public override void HandleAttack()
        {
            //애니메이션 실행(이벤트 사용할거임)
            base.HandleAttack();
        }

        public override void HandleMove()
        {
            base.HandleMove();
        }

        public override void HandleIdle()
        {
            base.HandleIdle();
        }

        // Attack애니메이션 이벤트로 실행할 함수
        public void NormalMonsterAttack()
        {
            // 근접 공격일경우
            if (monsterStat.monsterAttackType == AtkRangeType.Melee_Attack)
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.attackRange, targetLayer);
                
                foreach (var c in cols)
                {
                    IDamageable targetDamageable = c.GetComponent<IDamageable>();
                    if (targetDamageable != null)
                    {
                        //Pool 파티클 사용
                        //hs.TakeDamage(monsterStat.totalAttackPower);
                        targetDamageable.TakeDamage(this, 1f);

                    }
                }
            }
            else //원거리 일경우
            {
                if (target == null) return;

                IDamageable targetDamageable = target.GetComponent<IDamageable>();
                if (targetDamageable == null)
                    return;

                JHT_MonsterProjectile obj = JHT_MonsterSpawnManager.Instance.projectilePool.GetPooled() as JHT_MonsterProjectile;

                if (obj == null)
                    return;

                Vector2 startPos = transform.position;
                Vector2 targetPos = (Vector2)target.transform.position;


                //if (this != null)
                //    obj.Init(targetPos, startPos, monsterStat.totalAttackPower, monsterStat.totalAttackPower,monsterStat.projectileSprite);

                float projectileSpeed = 3f;
                obj.Init(this, targetPos, startPos, projectileSpeed, monsterStat.attackPower,monsterStat.projectileSprite);

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
