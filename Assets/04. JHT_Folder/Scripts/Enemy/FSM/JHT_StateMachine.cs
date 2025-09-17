using System.Collections.Generic;
using UnityEngine;
using static JHT.JHT_BaseMonsterFSM;

namespace JHT
{
    public class JHT_StateMachine
    {
        public Dictionary<MonsterState, JHT_BaseState> stateDic;
        public JHT_BaseState curState;

        public JHT_StateMachine()
        {
            stateDic = new();
        }

        public void ChangeState(JHT_BaseState changedState)
        {
            if (curState == changedState)
                return;

            if (curState != null)
                curState.Exit();

            curState = changedState;
            curState.Enter();
        }

        public void Update() => curState.Update();

        public void Exit() => curState.Exit();
    }
}
