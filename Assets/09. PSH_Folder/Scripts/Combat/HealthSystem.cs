using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

    public event Action<float, float> OnHealthChanged;

    private IHealthOwner healthOwner; // CombatCharacter 대신 IHealthOwner 인터페이스 사용

    void Awake()
    {
        // 이제 IHealthOwner 자격증을 가진 컴포넌트를 찾습니다.
        healthOwner = GetComponent<IHealthOwner>();
        if (healthOwner == null)
        {
            Debug.LogError("HealthSystem이 붙어있는 오브젝트에 IHealthOwner를 구현하는 컴포넌트가 없습니다!", gameObject);
        }
    }

    /// <summary>
    /// 전투 시작 시 Health Owner가 호출하여 체력을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (healthOwner == null) return;

        this.maxHealth = healthOwner.GetCurrentStat(Stat.Health);
        this.currentHealth = this.maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"'{healthOwner.name}' 체력 초기화: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Health Owner가 스탯 변경 이벤트를 받았을 때 호출합니다.
    /// </summary>
    public void OnStatUpdate()
    {
        if (healthOwner == null) return;

        float newMaxHealth = healthOwner.GetCurrentStat(Stat.Health);
        float healthDifference = newMaxHealth - this.maxHealth;

        this.maxHealth = newMaxHealth;

        if (healthDifference > 0)
        {
            this.currentHealth += healthDifference;
        }

        this.currentHealth = Mathf.Min(this.currentHealth, this.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"'{healthOwner.name}' 체력 스탯 변경 적용. 현재 체력: {currentHealth}/{maxHealth}");
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"💀 {healthOwner.name}이(가) 쓰러졌습니다.");
            // TODO: 사망 처리 로직 호출
        }
    }

    public void TakeDamage(IAttacker attacker)
    {
        if (healthOwner == null) return;

        // 1. 공격자와 방어자의 스탯 가져오기
        float attackerAttack = attacker.GetCurrentStat(Stat.Attack);
        float attackerCritChance = attacker.GetCurrentStat(Stat.CritChance);
        float attackerCritDamage = attacker.GetCurrentStat(Stat.CritDamage);
        float defenderDefense = healthOwner.GetCurrentStat(Stat.Defense);

        // 2. 데미지 계산
        // 기본 데미지 = 공격력 * (100 / (100 + 방어력))
        float baseDamage = attackerAttack * (100f / (100f + defenderDefense));

        // 치명타 계산 (치명타 확률은 0과 1 사이의 값으로 가정)
        bool isCritical = UnityEngine.Random.value < attackerCritChance;
        float finalDamage = baseDamage;

        if (isCritical)
        {
            finalDamage *= attackerCritDamage;
            Debug.Log("✨ 치명타 발생! ✨");
        }

        // 3. 최종 데미지 적용
        currentHealth -= finalDamage;
        Debug.Log($"💥 {attacker.name}이(가) {healthOwner.name}에게 {finalDamage:F1}의 데미지를 입혔습니다! (기본: {baseDamage:F1})");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"💀 {healthOwner.name}이(가) 쓰러졌습니다.");
            // TODO: 사망 처리 로직 호출
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}