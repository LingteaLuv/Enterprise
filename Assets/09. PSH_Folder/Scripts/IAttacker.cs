/// <summary>
/// 공격자의 기본 스탯 정보를 제공하기 위한 인터페이스입니다.
/// CombatCharacter, EnemyStats 등이 이 인터페이스를 구현할 수 있습니다.
/// </summary>
public interface IAttacker
{
    /// <summary>
    /// 버프 등이 모두 적용된 최종 스탯 값을 가져옵니다.
    /// </summary>
    float GetCurrentStat(Stat stat);

    /// <summary>
    /// 공격자의 이름입니다. (MonoBehaviour의 name 속성을 사용합니다)
    /// </summary>
    string name { get; }
}
