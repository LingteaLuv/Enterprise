using UnityEngine;

/// <summary>
/// 유물 시스템 사용 예시
/// </summary>
public class RelicUsageExample : MonoBehaviour
{
    private void Start()
    {
        // 유물 획득 조건 체크 (예: 크루 레벨 10 달성)
        RelicCollectionManager.Instance.CheckAcquisitionCondition(AcquisitionType.CrewLevel, 10);

        // 특정 유물 직접 획득
        RelicCollectionManager.Instance.AcquireRelic(1001);

        // 유물 레벨업
        RelicCollectionManager.Instance.LevelUpRelic(1001);

        // 컬렉션 진행률 확인
        float progress = RelicCollectionManager.Instance.GetCollectionProgress();
        Debug.Log($"컬렉션 진행률: {progress * 100:F1}%");
    }

    private void OnEnable()
    {
        // 이벤트 구독
        RelicCollectionManager.Instance.OnRelicAcquired += OnRelicAcquired;
        RelicCollectionManager.Instance.OnRelicLevelUp += OnRelicLevelUp;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (RelicCollectionManager.Instance != null)
        {
            RelicCollectionManager.Instance.OnRelicAcquired -= OnRelicAcquired;
            RelicCollectionManager.Instance.OnRelicLevelUp -= OnRelicLevelUp;
        }
    }

    /// <summary>
    /// 유물 획득시 호출되는 콜백
    /// </summary>
    private void OnRelicAcquired(RelicInstance relic)
    {
        Debug.Log($"새로운 유물 획득: {relic.data.relicName}");
        // UI 업데이트, 사운드 재생 등
    }

    /// <summary>
    /// 유물 레벨업시 호출되는 콜백
    /// </summary>
    private void OnRelicLevelUp(RelicInstance relic)
    {
        Debug.Log($"유물 레벨업: {relic.data.relicName} (Lv.{relic.level})");
        // UI 업데이트, 이펙트 재생 등
    }
}
