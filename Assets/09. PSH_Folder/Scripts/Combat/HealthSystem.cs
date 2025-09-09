using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

    // 체력이 변경될 때 호출되는 이벤트입니다. (현재 체력, 최대 체력)
    public event Action<float, float> OnHealthChanged;

    private CombatCharacter combatCharacter;

    void Awake()
    {
        combatCharacter = GetComponent<CombatCharacter>();
    }

    /// <summary>
    /// 전투 시작 시 CombatCharacter가 호출하여 체력을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        // GetCurrentStat을 사용하여 최대 체력을 가져와 설정합니다.
        this.maxHealth = combatCharacter.GetCurrentStat(Stat.Health);
        this.currentHealth = this.maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"'{combatCharacter.name}' 체력 초기화: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// CombatCharacter가 스탯 변경 이벤트를 받았을 때 호출합니다.
    /// </summary>
    public void OnStatUpdate()
    {
        float newMaxHealth = combatCharacter.GetCurrentStat(Stat.Health);
        float healthDifference = newMaxHealth - this.maxHealth;

        // 새로운 최대 체력으로 업데이트
        this.maxHealth = newMaxHealth;

        // 최대 체력이 증가했다면, 현재 체력도 그만큼 더해줍니다.
        if (healthDifference > 0)
        {
            this.currentHealth += healthDifference;
        }

        // 현재 체력이 새로운 최대 체력을 넘지 않도록 보정합니다.
        this.currentHealth = Mathf.Min(this.currentHealth, this.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"'{combatCharacter.name}' 체력 스탯 변경 적용. 현재 체력: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // TODO: 사망 처리 로직 호출
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
