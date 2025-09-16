using JHT;
using UnityEngine;

namespace JHT
{
    public class JHT_NormalMonster : JHT_BaseMonsterFSM
    {

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void Init(JHT_MonsterDataSO so)
        {
            base.Init(so);

            if (monsterUI != null)
            {
                monsterUI.gameObject.SetActive(true);
            }

            switch (monsterStat.monsterRarity)
            {
                case MonsterRarity.Elite:
                    gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1);
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.2f, 0);
                    break;
                case MonsterRarity.Normal:
                    gameObject.transform.localScale = Vector3.one;
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.15f, 0);
                    break;
                case MonsterRarity.Boss:
                    gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
                    monsterPrefab.transform.localScale = gameObject.transform.localScale;
                    monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.3f, 0);
                    break;
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
            if (monsterStat.monsterType == MonsterType.close)
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, monsterStat.attackRange, targetLayer);
                
                foreach (var c in cols)
                {
                    IDamageable targetDamageable = c.GetComponent<IDamageable>();
                    if (targetDamageable != null)
                    {
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
