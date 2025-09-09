using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CombatCharacter : MonoBehaviour
{
    public PlayerCharacterData CharacterStats { get; private set; } // 원본 데이터 참조

    [SerializeField] private string charName;
    // --- 전투 기본 스탯 (Initialize에서 복사) ---
    [SerializeField] private float baseAttack;
    [SerializeField] private float baseHealth;
    [SerializeField] private float baseDefense;
    [SerializeField] private float baseCritChance;
    [SerializeField] private float baseCritDamage;
    [SerializeField] private float baseAttackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;

    // --- 버프/디버프로 인한 추가 스탯 ---
    private float bonusAttack;
    private float bonusHealth;
    private float bonusDefense;
    private float bonusCritChance;
    private float bonusCritDamage;
    private float bonusAttackSpeed;

    // --- 컴포넌트 참조 ---
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        // 자기 자신의 SpriteRenderer 컴포넌트를 미리 찾아둡니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(PlayerCharacterData data)
    {
        this.CharacterStats = data;

        charName = data.characterdata.characterName;

        // PlayerData의 영구 스탯을 '전투 기본 스탯' 변수들로 복사합니다.
        baseAttack = data.finalStats.GetValueOrDefault(Stat.Attack, 0);
        baseHealth = data.finalStats.GetValueOrDefault(Stat.Health, 0);
        baseDefense = data.finalStats.GetValueOrDefault(Stat.Defense, 0);
        baseCritChance = data.finalStats.GetValueOrDefault(Stat.CritChance, 0);
        baseCritDamage = data.finalStats.GetValueOrDefault(Stat.CritDamage, 0);
        baseAttackSpeed = data.finalStats.GetValueOrDefault(Stat.AttackSpeed, 0);

        // 이동 속도와 사거리 등 전투에서만 필요한 스탯들은 가져오지 않고 여기서 처리
        moveSpeed = PartyManager.Instance.moveSpeed;// 캐릭터마다 동일 속도니까
        // 원거리와 근거리 
        attackRange = data.characterdata.atkRangeType == AtkRangeType.Ranged_Attack ?
            PartyManager.Instance.attackRange : PartyManager.Instance.attackRange2;
        

        // '추가 스탯'은 모두 0으로 초기화합니다.
        bonusAttack = 0;
        bonusHealth = 0;
        bonusDefense = 0;
        bonusCritChance = 0;
        bonusCritDamage = 0;
        bonusAttackSpeed = 0;

        if (spriteRenderer != null && data.characterdata != null)
        {
            spriteRenderer.sprite = data.characterdata.characterSprite;
        }

        Debug.Log($"'{CharacterStats.characterdata.characterName}' 데이터 적용 완료.");

        if (healthSystem != null)
        {
            healthSystem.Initialize();
        }
    }

    /// <summary>
    /// 전투 중 실제 스탯이 필요할 때 이 함수를 호출합니다.
    /// </summary>
    public float GetCurrentStat(Stat stat)
    {
        switch (stat)
        {
            case Stat.Attack: return baseAttack + bonusAttack;
            case Stat.Health: return baseHealth + bonusHealth;
            case Stat.Defense: return baseDefense + bonusDefense;
            case Stat.CritChance: return baseCritChance + bonusCritChance;
            case Stat.CritDamage: return baseCritDamage + bonusCritDamage;
            case Stat.AttackSpeed: return baseAttackSpeed + bonusAttackSpeed;
            default: return 0;
        }
    }

    /// <summary>
    /// (예시) 버프 등으로 일시적인 스탯 보너스를 적용합니다.
    /// </summary>
    public void ApplyStatBonus(Stat stat, float amount)
    {
        switch (stat)
        {
            case Stat.Attack: bonusAttack += amount; break;
            case Stat.Health: bonusHealth += amount; break;
            case Stat.Defense: bonusDefense += amount; break;
            case Stat.CritChance: bonusCritChance += amount; break;
            case Stat.CritDamage: bonusCritDamage += amount; break;
            case Stat.AttackSpeed: bonusAttackSpeed += amount; break;
        }
        Debug.Log($"{CharacterStats.characterdata.characterName} {stat} 스탯 보너스 적용: +{amount}");
    }

    void OnEnable()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;
    }

    void OnDisable()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
    }

    private void HandleCharacterUpdate(PlayerCharacterData updatedData)
    {
        if (this.CharacterStats == updatedData)
        {
            Debug.Log($"전투 중인 '{CharacterStats.characterdata.characterName}'의 영구 스탯 변경 감지.");
            // 영구 스탯이 변경되었으므로, 전투 기본 스탯을 새로 복사합니다.
            baseAttack = updatedData.finalStats.GetValueOrDefault(Stat.Attack, 0);
            baseHealth = updatedData.finalStats.GetValueOrDefault(Stat.Health, 0);
            baseDefense = updatedData.finalStats.GetValueOrDefault(Stat.Defense, 0);
            baseCritChance = updatedData.finalStats.GetValueOrDefault(Stat.CritChance, 0);
            baseCritDamage = updatedData.finalStats.GetValueOrDefault(Stat.CritDamage, 0);
            baseAttackSpeed = updatedData.finalStats.GetValueOrDefault(Stat.AttackSpeed, 0);

            if (healthSystem != null)
            {
                healthSystem.OnStatUpdate();
            }
        }
    }
}