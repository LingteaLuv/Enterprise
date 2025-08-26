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
            maxHP = characterData.health;
            attack = characterData.attack;
            defense = characterData.defense;
            criticalChance = characterData.criticalChance;
            criticalDamage = characterData.criticalDamage;

            currentHP = maxHP;
        }

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
            // 이동과 타겟은 전투 시스템과 연결되기 때문에 용호님과 상의 후 작성
        }

        public override void TargetFind()
        {
            
        }
    }
}