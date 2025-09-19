using System.Threading.Tasks;
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

        public override void HandleAttackAsync()
        {
            base.HandleAttackAsync();
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
            monsterStat.normalSkill.Use(this);
        }

        public void Skill1MonsterAttack()
        {
            monsterStat.skill1.Use(this);
        }

        public void Skill2MonsterAttack()
        {
            monsterStat.skill2.Use(this);
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
