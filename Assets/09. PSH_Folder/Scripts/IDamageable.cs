/// <summary>
/// 데미지를 받을 수 있는 모든 오브젝트가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 지정된 공격자로부터 데미지를 받습니다.
    /// </summary>
    /// <param name="attacker">데미지를 가하는 공격자</param>
    void TakeDamage(IAttacker attacker);
}
