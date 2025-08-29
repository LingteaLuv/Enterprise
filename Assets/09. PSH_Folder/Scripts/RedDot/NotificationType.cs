// 어떤 종류의 알림인지를 나타내는 열거형 (enum)
public enum NotificationType
{
    // 캐릭터 개별 알림
    CharacterStarUpgrade, // 캐릭터 성급 업그레이드
    CharacterLevelUp,     // 캐릭터 레벨업

    // 전체(상위) 알림
    OverallCharacter      // 캐릭터 관련 전체 알림 (승급, 레벨업 등)
}
