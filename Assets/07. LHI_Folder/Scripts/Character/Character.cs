using UnityEngine;

namespace LHI
{
    public abstract class Character : MonoBehaviour
    {
        public float currentHP;
        public float maxHP;
        public float attack;
        public float defense;

        public float criticalChance;
        public float criticalDamage;

        public abstract void TakeDamage(float damage);

        public abstract void Die();

        public abstract void Heal(float amount);

        public abstract void Move();

        public abstract void TargetFind();

        // 스킬 시전의 방식에 관련해서 추후에 작성, 파악 필요
    }
}