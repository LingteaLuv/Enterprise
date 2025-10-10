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

        public override void Init(JHT_BaseMonsterStat stat, SpawnType type)
        {
            base.Init(stat, type);

            if (monsterUI != null)
            {
                monsterUI.gameObject.SetActive(true);
            }

        }

        protected override void MonsterSizeSetting(SpawnType type)
        {
            if (type == SpawnType.BossStage)
            {
                switch (monsterStat.monsterCrewRole)
                {
                    case CrewRole.Sailor:
                    case CrewRole.Deckhand:
                    case CrewRole.Cook:
                        gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                        monsterPrefab.transform.localScale = gameObject.transform.localScale;
                        monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.10f, 0);
                        break;
                    case CrewRole.Captain:
                        gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
                        monsterPrefab.transform.localScale = gameObject.transform.localScale;
                        monsterUI.transform.localPosition = new Vector3(0, monsterPrefab.transform.localScale.y - 0.15f, 0);
                        break;
                }
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
