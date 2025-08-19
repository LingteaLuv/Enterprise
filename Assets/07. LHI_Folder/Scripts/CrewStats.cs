using UnityEngine;

namespace LHI
{
    public class CrewStats : StatBehaviour
    {
        // 아군 캐릭터를 크루 라는 이름으로 정의하여 사용 예정
        // 캐릭터는 적군과 아군을 포함하는 용어로서 사용


        private void Start()
        {
            CurrentHP = MaxHP;
            CurrentSkillGauge = 0;
        }

        public override void TakeDamage(float damage)
        {
            CurrentHP = Mathf.Max(CurrentHP - damage, 0);
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        public override void Die()
        {
            
        }
    }
}