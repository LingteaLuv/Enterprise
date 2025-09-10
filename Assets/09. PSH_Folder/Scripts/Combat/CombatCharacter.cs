
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 지속 시간을 가지는 버프 정보를 저장하는 클래스입니다.
/// </summary>
public class Buff
{
    public Stat Stat { get; private set; }
    public float Value { get; private set; }
    public float Duration { get; set; }

    public Buff(Stat stat, float value, float duration)
    {
        this.Stat = stat;
        this.Value = value;
        this.Duration = duration;
    }
}

[RequireComponent(typeof(SpriteRenderer))]
public class CombatCharacter : MonoBehaviour, IAttacker
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

    // --- 현재 적용중인 버프 리스트 ---
    [SerializeField] private List<Buff> activeBuffs = new List<Buff>();

    [Header("보유 스킬")]
    public List<SkillSO> skills = new List<SkillSO>();

    // --- 컴포넌트 참조 ---
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        baseCritChance = data.finalStats.GetValueOrDefault(Stat.CritChance, 0);
        baseCritDamage = data.finalStats.GetValueOrDefault(Stat.CritDamage, 0);
        baseAttackSpeed = data.finalStats.GetValueOrDefault(Stat.AttackSpeed, 0);

        moveSpeed = PartyManager.Instance.moveSpeed;
        attackRange = data.characterdata.atkRangeType == AtkRangeType.Ranged_Attack ?
            PartyManager.Instance.attackRange : PartyManager.Instance.attackRange2;

        // 버프 리스트 초기화
        activeBuffs.Clear();

        if (spriteRenderer != null && data.characterdata != null)
        {
            spriteRenderer.sprite = data.characterdata.characterSprite;
        }

        Debug.Log($"'{charName}' 데이터 적용 완료.");

        if (healthSystem != null)
        {
            healthSystem.Initialize();
        }
    }

    /// <summary>
    /// 전투 중 실제 스탯이 필요할 때 이 함수를 호출합니다.
    /// 기본 스탯과 모든 활성화된 버프의 합을 반환합니다.
    /// </summary>
    public float GetCurrentStat(Stat stat)
    {
        float bonusValue = activeBuffs.Where(b => b.Stat == stat).Sum(b => b.Value);
        float finalValue = 0; // 최종 스탯 값을 저장할 변수

        switch (stat)
        {
            case Stat.Attack: finalValue = baseAttack + bonusValue; break;
            case Stat.Health: finalValue = baseHealth + bonusValue; break;
            case Stat.Defense: finalValue = baseDefense + bonusValue; break;
            case Stat.CritChance: finalValue = baseCritChance + bonusValue; break;
            case Stat.CritDamage: finalValue = baseCritDamage + bonusValue; break;
            case Stat.AttackSpeed: finalValue = baseAttackSpeed + bonusValue; break;
            default: finalValue = 0; break; // 정의되지 않은 스탯은 0으로 처리
        }

        Debug.Log($"[CombatCharacter] {charName}의 {stat} 최종 스탯: {finalValue}"); // 최종 스탯 로그 출력
        return finalValue;
    }

    /// <summary>
    /// 새로운 버프를 캐릭터에게 적용합니다.
    /// </summary>
    public void ApplyBuff(Stat stat, float value, float duration)
    {
        Buff newBuff = new Buff(stat, value, duration);
        activeBuffs.Add(newBuff);
        Debug.Log($"'{charName}'에게 버프 적용: {stat}, 수치: {value}, 지속시간: {duration}초");

        // 체력 버프는 즉시 HealthSystem에 반영
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
        }
    }

    /// <summary>
    /// 지정된 인덱스의 스킬을 대상에게 사용합니다.
    /// </summary>
    /// <param name="skillIndex">사용할 스킬의 리스트 인덱스</param>
    /// <param name="target">스킬 대상 캐릭터</param>
    public void UseSkill(int skillIndex, CombatCharacter target)
    {
        if (skillIndex < 0 || skillIndex >= skills.Count)
        {
            Debug.LogError($"잘못된 스킬 인덱스: {skillIndex}");
            return;
        }

        SkillSO skillToUse = skills[skillIndex];
        if (skillToUse != null)
        {
            // TODO: 여기에 쿨다운, 마나 비용 체크 로직 추가 가능
            skillToUse.Use(this, target);
        }
    }
}
