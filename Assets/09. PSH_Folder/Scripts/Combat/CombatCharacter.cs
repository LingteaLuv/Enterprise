using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 지속 시간을 가지는 버프 정보를 저장하는 클래스입니다.
/// </summary>
public class Buff
{
    public Stat Stat { get; private set; }
    public BuffType BuffType { get; private set; } // 버프 타입 추가
    public float Value { get; private set; }
    public float Duration { get; set; }

    public Buff(Stat stat, float value, float duration, BuffType buffType)
    {
        this.Stat = stat;
        this.Value = value;
        this.Duration = duration;
        this.BuffType = buffType;
    }
}

[RequireComponent(typeof(HealthSystem))]
public class CombatCharacter : MonoBehaviour, IAttacker, IDamageable
{
    public PlayerCharacterData CharacterStats { get; private set; } // 원본 데이터 참조

    [SerializeField] public string charName;
    // --- 전투 기본 스탯 (Initialize에서 복사) ---
    [SerializeField] private float baseAttack;
    [SerializeField] private float baseHealth;
    [SerializeField] private float baseDefense;
    [SerializeField] private float baseCritChance;
    [SerializeField] private float baseCritDamage;
    [SerializeField] private float baseAttackSpeed;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float attackRange;

    // --- 현재 적용중인 버프 리스트 ---
    [SerializeField] private List<Buff> activeBuffs = new List<Buff>();

    [Header("보유 스킬")]
    public List<SkillSO> skills = new List<SkillSO>();

    // --- 컴포넌트 참조 ---
    private HealthSystem healthSystem;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void Update()
    {
        // 버프 지속시간 처리
        ProcessBuffs();
    }

    public void Initialize(PlayerCharacterData data)
    {
        this.CharacterStats = data;
        charName = data.characterdata.characterName;

        // 영구 스탯을 기본 스탯으로 복사
        baseAttack = data.finalStats.GetValueOrDefault(Stat.Attack, 0);
        baseHealth = data.finalStats.GetValueOrDefault(Stat.Health, 0);
        baseDefense = data.finalStats.GetValueOrDefault(Stat.Defense, 0);
        baseCritChance = data.finalStats.GetValueOrDefault(Stat.CritChance, 0) / 100;
        baseCritDamage = data.finalStats.GetValueOrDefault(Stat.CritDamage, 0) / 100;
        baseAttackSpeed = data.finalStats.GetValueOrDefault(Stat.AttackSpeed, 0) / 100;

        //Debug.LogError($"{PartyManager.Instance.moveSpeed}");
        moveSpeed = PartyManager.Instance.moveSpeed;
        attackRange = data.characterdata.atkRangeType == AtkRangeType.Ranged_Attack ?
            PartyManager.Instance.attackRange : PartyManager.Instance.attackRange2;

        // 버프 리스트 초기화
        activeBuffs.Clear();

        Debug.Log($"'{charName}' 데이터 적용 완료.");

        if (healthSystem != null)
        {
            healthSystem.Initialize();
        }

        // 스킬 등록
        RegisterSkills(data);
    }

    // IDamageable 인터페이스 구현
    public void TakeDamage(IAttacker attacker, float powerRatio = 1f)
    {
        healthSystem.CalculateAndApplyDamage(attacker, powerRatio);
    }

    public float GetCurrentStat(Stat stat)
    {
        float baseValue = 0;
        switch (stat)
        {
            case Stat.Attack: baseValue = baseAttack; break;
            case Stat.Health: baseValue = baseHealth; break;
            case Stat.Defense: baseValue = baseDefense; break;
            case Stat.CritChance: baseValue = baseCritChance; break;
            case Stat.CritDamage: baseValue = baseCritDamage; break;
            case Stat.AttackSpeed: baseValue = baseAttackSpeed; break;
        }

        float flatBonus = activeBuffs.Where(b => b.Stat == stat && b.BuffType == BuffType.Flat).Sum(b => b.Value);
        float percentBonus = activeBuffs.Where(b => b.Stat == stat && b.BuffType == BuffType.Percent).Sum(b => b.Value);

        float finalValue = (baseValue + flatBonus) * (1 + percentBonus);

       // Debug.Log($"[CombatCharacter] {charName}의 {stat} 최종 스탯: {finalValue} (기본: {baseValue}, 고정 버프: {flatBonus}, 퍼센트 버프: {percentBonus})");
        return finalValue;
    }

    /// <summary>
    /// 새로운 버프를 캐릭터에게 적용합니다.
    /// </summary>
    public void ApplyBuff(Stat stat, float value, float duration, BuffType buffType)
    {
        Buff newBuff = new Buff(stat, value, duration, buffType);
        activeBuffs.Add(newBuff);
        Debug.Log($"'{charName}'에게 버프 적용: {stat}, 타입: {buffType}, 수치: {value}, 지속시간: {duration}초");

        // 체력 관련 스탯이 변경되면 HealthSystem에 즉시 반영
        if (stat == Stat.Health)
        {
            healthSystem?.OnStatUpdate();
        }
    }

    /// <summary>
    /// 매 프레임 버프의 지속시간을 감소시키고 만료된 버프를 제거합니다.
    /// </summary>
    private void ProcessBuffs()
    {
        if (activeBuffs.Count == 0) return;

        bool healthBuffRemoved = false;

        // 뒤에서부터 순회해야 안전하게 리스트 아이템을 삭제할 수 있습니다.
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = activeBuffs[i];
            buff.Duration -= Time.deltaTime;

            if (buff.Duration <= 0)
            {
                Debug.Log($"'{charName}'의 {buff.Stat} 버프 만료.");
                if (buff.Stat == Stat.Health) healthBuffRemoved = true;
                activeBuffs.RemoveAt(i);
            }
        }

        // 체력 버프가 제거되었다면 HealthSystem을 업데이트합니다.
        if (healthBuffRemoved)
        {
            healthSystem?.OnStatUpdate();
        }
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
            Debug.Log($"전투 중인 '{charName}'의 영구 스탯 변경 감지.");
            baseAttack = updatedData.finalStats.GetValueOrDefault(Stat.Attack, 0);
            baseHealth = updatedData.finalStats.GetValueOrDefault(Stat.Health, 0);
            baseDefense = updatedData.finalStats.GetValueOrDefault(Stat.Defense, 0);
            baseCritChance = updatedData.finalStats.GetValueOrDefault(Stat.CritChance, 0);
            baseCritDamage = updatedData.finalStats.GetValueOrDefault(Stat.CritDamage, 0);
            baseAttackSpeed = updatedData.finalStats.GetValueOrDefault(Stat.AttackSpeed, 0);

            healthSystem?.OnStatUpdate();
            RegisterSkills(updatedData); // 스킬 정보도 갱신
        }
    }

    /// <summary>
    /// 캐릭터 데이터에 명시된 스킬 ID를 기반으로 스킬 리스트를 설정합니다.
    /// </summary>
    private void RegisterSkills(PlayerCharacterData data)
    {
        skills.Clear();

        // 패시브 스킬 등록
        if (data.characterdata.skillPassiveID != 0)
        {
            SkillSO passiveSkill = SkillDatabase.GetSkillByID(data.characterdata.skillPassiveID);
            if (passiveSkill != null)
            {
                skills.Add(passiveSkill);
              //  Debug.Log($"'{charName}'에게 패시브 스킬 '{passiveSkill.skillName}'을(를) 등록했습니다.");
            }
        }

        // TODO: 액티브 스킬 등 다른 스킬 ID가 있다면 여기에 추가로 등록하는 로직을 구현할 수 있습니다.
    }

    /// <param name="skillIndex">사용할 스킬의 리스트 인덱스</param>
    /// <param name="target">스킬 대상</param>
    public void UseSkill(int skillIndex, IDamageable target)
    {
        UseSkill(skillIndex, new List<IDamageable> { target });
    }

    /// <summary>
    /// 지정된 인덱스의 스킬을 여러 대상에게 사용합니다.
    /// </summary>
    /// <param name="skillIndex">사용할 스킬의 리스트 인덱스</param>
    /// <param name="targets">스킬 대상 리스트</param>
    public void UseSkill(int skillIndex, List<IDamageable> targets)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count)
        {
            Debug.LogError($"잘못된 스킬 인덱스: {skillIndex}");
            return;
        }

        SkillSO skillToUse = skills[skillIndex];
        if (skillToUse != null)
        {
            skillToUse.Use(this);
        }
    }

    public string GetName()
    {
        return charName;
    }
}