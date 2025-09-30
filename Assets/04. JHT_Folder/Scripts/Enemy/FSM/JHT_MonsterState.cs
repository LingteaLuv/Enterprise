using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace JHT
{
    public class JHT_MonsterState : JHT_BaseState
    {
        protected JHT_BaseMonsterFSM fsm;

        public JHT_MonsterState(JHT_BaseMonsterFSM _fsm)
        {
            fsm = _fsm;
        }

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Update()
        {
            if (fsm.monsterStat != null && fsm.CurHP <= 0)
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.DEATH]);

            if (fsm.currentState == JHT_BaseMonsterFSM.MonsterState.DEATH)
                return;

            if(fsm.isStun)
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.STUN]);


        }

    }

    public class Monster_Idle : JHT_MonsterState
    {
        public Monster_Idle(JHT_BaseMonsterFSM _fsm) : base(_fsm)
        {

        }

        public override void Enter()
        {
            base.Enter();
            fsm.HandleIdle();
            //if(fsm.animator != null)
            //    fsm.animator.Play(fsm.IDLE);
        }

        public override void Update()
        {
            base.Update();

            if (fsm.target == null)
            {
                fsm.TryAcquireNextTarget();
                return;
            }

            float dist = Vector2.Distance(fsm.target.transform.position,
                    fsm.gameObject.transform.position);

            if (dist < fsm.monsterStat.chaseRange)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.MOVE]);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Monster_Move : JHT_MonsterState
    {
        public Monster_Move(JHT_BaseMonsterFSM _fsm) : base(_fsm)
        {

        }
        public override void Enter()
        {
            base.Enter();
            fsm.HandleMove();
            //if (fsm.animator != null)
            //    fsm.animator.Play(fsm.MOVE);
        }

        public override void Update()
        {
            base.Update();

            if (fsm.target == null)
            {
                fsm.monsterSearch.StopRoutine();
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.IDLE]);
                return;
            }

            if(fsm.monsterSpawnType == SpawnType.IslandStage)
                fsm.monsterSearch.SearchTarget();
            else if(fsm.monsterSpawnType == SpawnType.BossStage)
                fsm.transform.position = Vector3.MoveTowards(fsm.transform.position, fsm.target.transform.position, 1 * Time.deltaTime);
            
            fsm.Rotate();
            
            float dist = Vector2.Distance(fsm.target.transform.position, fsm.gameObject.transform.position);

            if (dist <= fsm.monsterStat.attackRange)
            {
                fsm.monsterSearch.StopRoutine();
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.ATTACK]);
            }

            //if (dist > fsm.monsterStat.chaseRange)
            //{
            //    fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.IDLE]);
            //}
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Monster_Attack : JHT_MonsterState
    {
        float safeZone = 0.1f;
        public Monster_Attack(JHT_BaseMonsterFSM _fsm) : base(_fsm)
        {

        }
        public override void Enter()
        {
            base.Enter();
            fsm.HandleAttackAsync();
        }

        public override void Update()
        {
            base.Update();

            if (fsm.target == null)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.IDLE]);
                return;
            }

            fsm.Rotate();

            if (Vector2.Distance(fsm.target.transform.position,
                fsm.gameObject.transform.position) > fsm.monsterStat.attackRange + safeZone)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.MOVE]);
                return;
            }

        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Monster_Stun : JHT_MonsterState
    {
        public Monster_Stun(JHT_BaseMonsterFSM _fsm) : base(_fsm)
        {
        }

        public override void Enter()
        {
            base.Enter();
            fsm.HandleStun();
        }

        public override void Update()
        {
            base.Update();
            //idle로 갔다가 이상이 생길경우 ATTACK으로 변경
            if(!fsm.isStun)
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[JHT_BaseMonsterFSM.MonsterState.IDLE]);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Monster_TakeDamage : JHT_MonsterState
    {
        public Monster_TakeDamage(JHT_BaseMonsterFSM _fsm) : base(_fsm) { }

        public override void Enter()
        {
            base.Enter();
            //fsm.animator.Play(fsm.DEATH);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }


    public class Monster_Die : JHT_MonsterState
    {
        public Monster_Die(JHT_BaseMonsterFSM _fsm) : base(_fsm)
        {

        }
        public override void Enter()
        {
            //fsm.animator.Play(fsm.DEATH);
            fsm.HandleDie();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}