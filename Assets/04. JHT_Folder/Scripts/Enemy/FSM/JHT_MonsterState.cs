using UnityEngine;

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
            if (fsm.currentState == PlayerState.DEATH)
                return;

            if (fsm.curHP <= 0)
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.DEATH]);

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

            float dist = Mathf.Abs(Vector2.Distance(fsm.target.transform.position,
                    fsm.gameObject.transform.position));

            if (dist < fsm.monsterStat.chaseRange)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.MOVE]);
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
            fsm.HandleMove();

            if (fsm.target == null)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.IDLE]);
                return;
            }

            float dist = Mathf.Abs(Vector2.Distance(fsm.target.transform.position, fsm.gameObject.transform.position));

            if (dist < fsm.monsterStat.attackRange)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.ATTACK]);
            }

            if (dist > fsm.monsterStat.chaseRange)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.IDLE]);
            }
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
            fsm.HandleAttack();
        }

        public override void Update()
        {
            base.Update();

            if (fsm.target == null)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.IDLE]);
                return;
            }

            fsm.Rotate();

            if (Mathf.Abs(Vector2.Distance(fsm.target.transform.position,
                fsm.gameObject.transform.position)) > fsm.monsterStat.attackRange + safeZone)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.MOVE]);
                return;
            }

            if (fsm.canAttack == false)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.OTHER]);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Monster_AttackDelay : JHT_MonsterState
    {
        public Monster_AttackDelay(JHT_BaseMonsterFSM _fsm) : base(_fsm) { }

        public override void Enter()
        {
            base.Enter();
            fsm.HandleAttackDelay();
        }
        public override void Update()
        {
            base.Update();
            if (fsm.canAttack == true)
            {
                fsm.stateMachine.ChangeState(fsm.stateMachine.stateDic[PlayerState.ATTACK]);
            }
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
            //fsm.HandleDie();
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