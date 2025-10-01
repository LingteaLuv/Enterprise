using UnityEngine;

/// <summary>
/// Caster에서 Target으로 이동하는 효과를 구현하는 모든 스크립트가 상속받아야 하는 인터페이스입니다.
/// </summary>
public interface IMoveEffect
{
    /// <summary>
    /// 이동 효과를 초기화하고 애니메이션을 시작합니다.
    /// </summary>
    /// <param name="caster">시전자 트랜스폼</param>
    /// <param name="target">대상 트랜스폼</param>
    void Init( Transform target);
}
