using System.Collections.Generic;

/// <summary>
/// 각 획득 조건별 처리 로직을 정의하는 인터페이스
/// </summary>
public interface IAcquisitionCondition
{
    bool CheckCondition(string conditionValue, object currentValue);
    string GetConditionDescription(string conditionValue);
}

/// <summary>
/// 크루원 레벨 조건 처리
/// </summary>
public class CrewLevelCondition : IAcquisitionCondition
{
    public bool CheckCondition(string conditionValue, object currentValue)
    {
        if (int.TryParse(conditionValue, out int requiredCrewLevel) && currentValue is int currentCrewLevel)
        {
            return currentCrewLevel >= requiredCrewLevel;
        }
        return false;
    }
    
    public string GetConditionDescription(string conditionValue)
    {
        return $"크루 레벨 {conditionValue} 이상";
    }
}

/// <summary>
/// 크루원 등급 조건 처리
/// </summary>
public class CrewGradeCondition : IAcquisitionCondition
{
    public bool CheckCondition(string conditionValue, object currentValue)
    {
        if (int.TryParse(conditionValue, out int requiredCrewGrade) && currentValue is int currentCrewGrade)
        {
            return currentCrewGrade >= requiredCrewGrade;
        }
        return false;
    }
    public string GetConditionDescription(string conditionValue)
    {
        return $"크루 등급 {conditionValue} 이상";
    }
}

/// <summary>
/// 스테이지 클리어 조건 처리
/// </summary>
public class CompleteStageCondition : IAcquisitionCondition
{
    public bool CheckCondition(string conditionValue, object currentValue)
    {
        if (int.TryParse(conditionValue, out int requiredStage) && currentValue is int currentStage)
        {
            return currentStage >= requiredStage;
        }
        return false;
    }

    public string GetConditionDescription(string conditionValue)
    {
        return $"스테이지 {conditionValue} 클리어";
    }
}

/// <summary>
/// 던전 클리어 조건 처리
/// </summary>
public class CompleteDungeonCondition : IAcquisitionCondition
{
    public bool CheckCondition(string conditionValue, object currentValue)
    {
        if (int.TryParse(conditionValue, out int requiredDungeon) && currentValue is int currentDungeon)
        {
            return currentDungeon >= requiredDungeon;
        }
        return false;
    }
    public string GetConditionDescription(string conditionValue)
    {
        return $"던전 {conditionValue} 클리어";
    }
}

/// <summary>
/// 획득 조건별 처리기를 생성하는 팩토리 클래스
/// </summary>
public static class AcquisitionConditionFactory
{
    private static Dictionary<AcquisitionType, IAcquisitionCondition> _conditions;

    static AcquisitionConditionFactory()
    {
        _conditions = new Dictionary<AcquisitionType, IAcquisitionCondition>
        {
            { AcquisitionType.CrewLevel, new CrewLevelCondition() },
            { AcquisitionType.CrewGrade, new CrewGradeCondition() },
            { AcquisitionType.StageClear, new CompleteStageCondition() },
            { AcquisitionType.DungeonClear, new CompleteDungeonCondition() },
            // 새로운 조건들을 여기에 추가
        };
    }

    /// <summary>
    /// 지정된 타입에 해당하는 획득 조건 처리기 반환
    /// </summary>
    public static IAcquisitionCondition GetCondition(AcquisitionType type)
    {
        _conditions.TryGetValue(type, out IAcquisitionCondition condition);
        return condition;
    }
}