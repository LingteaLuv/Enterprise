using UnityEngine;

namespace LHI
{
    public class Crew : Character
    {
        // 크루 아군 케릭터 스크립트를 작성하고, 몬스터도 작성후에 공통된 부분을 캐릭터 클래스로 옮길예정

        public CharacterData characterData;

        // 스킬 데이터 추가


        public void Awake()
        {
            // characterData 가져오기
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            // 캐릭터 데이터를 스테이터스에 적용


        }
        // MaxHP = characterData.health;
        // CurrentHP = MaxHP;
        //CurrentSkillGauge = 0;
        //MaxSkillGauge = 100; // 예시로 100으로 설정

        public override void TakeDamage(float damage)
        {
            currentHP = Mathf.Max(currentHP - damage, 0);
            if (currentHP <= 0)
            {
                Die();
            }
        }

        public override void Die()
        {
            gameObject.SetActive(false); // 캐릭터를 비활성화
        }

        public override void Heal(float amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
        }

        public override void Move()
        {
            // 캐릭터 
        }

        public override void TargetFind()
        {
            
        }
    }
}