using UnityEngine;

/// <summary>
/// HealthSystem을 소유할 수 있는 모든 컴포넌트가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IHealthOwner
{
    /// <summary>
    /// 현재 스탯 값을 가져옵니다. HealthSystem은 이 함수를 통해 Health 스탯을 얻습니다.
    /// </summary>
    float GetCurrentStat(Stat stat);

    /// <summary>
    /// 소유자의 이름입니다. 디버그 로그 등에 사용됩니다.
    /// </summary>
    string name { get; }

    /// <summary>
    /// 소유자의 게임 오브젝트입니다.
    /// </summary>
    GameObject gameObject { get; }
}
