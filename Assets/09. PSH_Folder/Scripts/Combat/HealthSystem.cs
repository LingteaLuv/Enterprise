using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    // 자기 자신의 스탯 제공자 (IAttacker가 스탯 제공의 역할도 겸함)
    private IAttacker statProvider;

    void Awake()
    {
        // 자기 자신의 스탯 제공자 컴포넌트를 찾습니다.
        statProvider = GetComponent<IAttacker>();
        if (statProvider == null)
        {
            Debug.LogError("HealthSystem이 붙어있는 오브젝트에 IAttacker를 구현하는 컴포넌트가 없습니다!", gameObject);
        }
    }

    /// <summary>
    /// 스탯 제공자가 호출하여 체력을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (statProvider == null) return;

        this.maxHealth = statProvider.GetCurrentStat(Stat.Health);
        this.currentHealth = this.maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        //Debug.Log($"'{statProvider.GetName()}' 체력 초기화: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 스탯 제공자가 스탯 변경 이벤트를 받았을 때 호출합니다.
    /// </summary>
    public void OnStatUpdate()
    {
        if (statProvider == null) return;

        float newMaxHealth = statProvider.GetCurrentStat(Stat.Health);
        float healthDifference = newMaxHealth - this.maxHealth;

        this.maxHealth = newMaxHealth;

        if (healthDifference > 0)
        {
            this.currentHealth += healthDifference;
        }

        this.currentHealth = Mathf.Min(this.currentHealth, this.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"'{statProvider.GetName()}' 체력 스탯 변경 적용. 현재 체력: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// IDamageable 인터페이스를 통해 호출되며, 실제 데미지 계산 및 적용을 담당합니다.
    /// </summary>
    public void CalculateAndApplyDamage(IAttacker attacker, float powerRatio)
    {
        if (statProvider == null) return;

        // 1. 공격자와 방어자의 스탯 가져오기
        float attackerAttack = attacker.GetCurrentStat(Stat.Attack) * powerRatio;
        float attackerCritChance = attacker.GetCurrentStat(Stat.CritChance);
        float attackerCritDamage = attacker.GetCurrentStat(Stat.CritDamage);
        float defenderDefense = statProvider.GetCurrentStat(Stat.Defense); // 자신의 스탯 사용

        // 2. 데미지 계산
        float baseDamage = attackerAttack * (100f / (100f + defenderDefense));
        bool isCritical = UnityEngine.Random.value < attackerCritChance;
        float finalDamage = baseDamage;

        // 속성 상성 데미지
        Faction attackerFaction = attacker.GetFaction();
        Faction defenderFaction = statProvider.GetFaction();
        int a = (attackerFaction - defenderFaction) % 3;
        float factionDamage = 1f;

        switch (a)
        {
            case 0:
                factionDamage = 1f;
                break;
            case 1:
                factionDamage = 0.8f;
                break;
            case 2:
                factionDamage = 1.2f;
                break;
            default:
                break;
        }

        finalDamage *= factionDamage;

        if (isCritical)
        {
            finalDamage *= attackerCritDamage;
            Debug.Log("✨ 치명타 발생! ✨");
        }

        // 3. [수정] 버프에 의한 추가 데미지를 계산에 포함합니다.
        var attackerCombatChar = attacker as CombatCharacter;
        if (attackerCombatChar != null)
        {
            float extraDamage = attackerCombatChar.GetOnHitDamageBonus();
            if (extraDamage > 0)
            {
                finalDamage += extraDamage;
                Debug.Log($"🔥 버프 효과! 추가 데미지 {extraDamage} 적용!");
            }
        }

        // 4. 최종 데미지 적용
        currentHealth -= finalDamage;
      //  Debug.Log($"💥 {attacker.GetName()}이(가) {statProvider.GetName()}에게 {finalDamage:F1}의 데미지를 입혔습니다!");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"💀 {statProvider.GetName()}이(가) 쓰러졌습니다.");
            OnDeath?.Invoke();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 체력을 회복시킵니다.
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"❤️ {statProvider.GetName()}'이(가) {amount:F1}만큼 체력을 회복했습니다. 현재 체력: {currentHealth:F1}/{maxHealth:F1}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


}
