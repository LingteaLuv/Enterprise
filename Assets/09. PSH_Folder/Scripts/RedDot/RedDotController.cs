using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 레드닷 UI의 모든 로직을 담당하는 단일 컴포넌트.
/// 이 스크립트 하나만 레드닷이 필요한 곳에 붙이면 됩니다.
/// </summary>
public class RedDotController : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("실제 레드닷 이미지 게임 오브젝트")]
    public GameObject redDotObject;

    [Header("설정")]
    [Tooltip("이 레드닷이 확인할 알림 조건 목록입니다.")]
    public List<NotificationType> notificationTypes = new List<NotificationType>();

    // 캐릭터별 알림일 경우, 외부에서 주입해주는 데이터
    private PlayerCharacterData characterContext;

    void Awake()
    {
        if (redDotObject != null) redDotObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 전체(상위) 알림 타입의 경우, 데이터 변경 이벤트를 직접 구독합니다.
        if (notificationTypes.Contains(NotificationType.OverallCharacter))
        {
            // PlayerDataManager가 아직 준비되지 않았을 수 있으므로 null 체크
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnOwnedCharactersChanged += CheckNotifications;
                PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterUpdate;
            }
        }
        // 활성화될 때 현재 상태를 한 번 체크합니다.
        CheckNotifications();
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때, 구독했던 이벤트를 반드시 해제합니다.
        if (notificationTypes.Contains(NotificationType.OverallCharacter))
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnOwnedCharactersChanged -= CheckNotifications;
                PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterUpdate;
            }
        }
    }

    /// <summary>
    /// 알림 상태를 검사하고 레드닷 UI를 갱신합니다.
    /// 캐릭터별 알림의 경우, 반드시 이 메소드를 통해 캐릭터 데이터를 전달해야 합니다.
    /// </summary>
    /// <param name="character">검사 기준이 될 캐릭터 데이터</param>
    public void CheckNotifications(PlayerCharacterData character = null)
    {
        this.characterContext = character;
        if (redDotObject == null) return;

        bool shouldShow = false;
        foreach (var type in notificationTypes)
        {
            if (CheckCondition(type))
            {
                shouldShow = true;
                break;
            }
        }
        redDotObject.SetActive(shouldShow);
    }
    
    // PlayerDataManager.OnOwnedCharactersChanged 이벤트는 파라미터가 없으므로, 이 오버로드를 사용합니다.
    private void CheckNotifications()
    { 
        CheckNotifications(null);
    }

    // OnCharacterDataUpdated 이벤트는 파라미터가 있으므로, 이 핸들러를 사용합니다.
    private void HandleCharacterUpdate(PlayerCharacterData data)
    {
        CheckNotifications(null);
    }

    /// <summary>
    /// 개별 알림 타입에 대한 조건을 확인합니다.
    /// </summary>
    private bool CheckCondition(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.CharacterStarUpgrade:
                return characterContext != null && NotificationManager.Instance.CanUpgradeStar(characterContext);
            
            case NotificationType.CharacterLevelUp:
                return characterContext != null && NotificationManager.Instance.CanLevelUp(characterContext);

            case NotificationType.OverallCharacter:
                return NotificationManager.Instance.ShouldShowOverallCharacterRedDot();

            default:
                return false;
        }
    }
}
