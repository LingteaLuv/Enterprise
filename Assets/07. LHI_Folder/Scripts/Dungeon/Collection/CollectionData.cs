using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectionData", menuName = "ScriptableObjects/CollectionData", order = 1)]
/// <summary>
/// 유물의 기본 정보를 담는 ScriptableObject
/// </summary>
public class CollectionData : ScriptableObject
{
    [Header("기본 정보")]
    public int id;
    public string relicName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;

    [Header("획득 조건")]
    public AcquisitionType acquisitionType;
    public string acquisitionValue;

    [Header("효과 정보")]
    public BuffDebuffType effectType;
    public RangeBuffDebuffType targetRange;
    public float baseEffectValue;

    /// <summary>
    /// 레벨에 따른 최종 효과값 계산 (베이스 + 레벨당 2% 증가)
    /// </summary>
    public float CalculateEffectValue(int level)
    {
        float levelMultiplier = 1f + (level - 1) * 0.02f;
        return baseEffectValue * levelMultiplier;
    }
}

/// <summary>
/// 유물 획득 조건의 종류를 정의
/// </summary>
public enum AcquisitionType
{
    CrewLevel,
    CrewGrade,
    StageClear,
    DungeonClear
}

/// <summary>
/// 유물이 제공하는 효과의 종류
/// </summary>
public enum BuffDebuffType
{
    AttackPower,    // 공격력
    Defense,        // 방어력
    AttackSpeed,    // 공격속도
    ExtraAttack,    // 추가 공격력
    Health,         // 최대 체력
}

/// <summary>
/// 유물 효과가 적용되는 대상 범위
/// </summary>
public enum RangeBuffDebuffType
{
    // Pirate (해적), Marine (해군), Monster (몬스터), Team (팀)
    PirateAlly,           // 해적 한 명
    MarineAlly,           // 해군 한 명
    MonsterAlly,          // 몬스터 한 명
    PirateAllies          // 팀 아군 전체
}

/// <summary>
/// 실제 게임에서 사용되는 유물 인스턴스, 레벨 정보와 획득 상태를 관리
/// </summary>
[System.Serializable]
public class RelicInstance
{
    public CollectionData data;
    public int level = 1;
    public bool isAcquired = false; // 획득 여부
    public DateTime acquiredTime; // 획득 시각 기록

    public const int MAX_LEVEL = 15;

    /// <summary>
    /// 현재 레벨에서의 효과값 반환
    /// </summary>
    public float GetCurrentEffectValue()
    {
        return data.CalculateEffectValue(level);
    }

    /// <summary>
    /// 유물 레벨업 (최대 레벨 체크 포함)
    /// </summary>
    public bool LevelUp()
    {
        if (level >= MAX_LEVEL) return false;

        level++;
        return true;
    }

    /// <summary>
    /// 유물 획득 처리
    /// </summary>
    public void Acquire()
    {
        if (!isAcquired)
        {
            isAcquired = true;
            acquiredTime = DateTime.Now;
        }
    }
}