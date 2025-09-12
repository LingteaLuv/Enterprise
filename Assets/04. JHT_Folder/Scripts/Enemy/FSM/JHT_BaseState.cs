using UnityEngine;

namespace JHT
{
    public abstract class JHT_BaseState
    {
        public abstract void Enter();

        public abstract void Update();

        public abstract void Exit();
    }
}
