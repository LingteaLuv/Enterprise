using UnityEngine;

namespace LHI
{
    public abstract class StatBehaviour : MonoBehaviour
    {
        public float CurrentHP;
        public float MaxHP;

        public float CurrentSkillGauge;
        public float MaxSkillGauge;

        public abstract void TakeDamage(float damage);

        public abstract void Die();

        // 힐, 스킬 게이지 충전 등의 메소드를 추가할 수 있습니다.
    }
}