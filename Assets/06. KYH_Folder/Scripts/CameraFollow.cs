using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 여러 유닛의 중심을 따라가는 카메라 컨트롤러
/// - 특정 UI가 켜졌을 때 Y축 오프셋 적용
/// - 플레이어가 움직이는 경우도 실시간 대응
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("UI 패널 (활성화 여부 감지)")]
    [SerializeField] private GameObject GatchaPanel;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private float uiYOffset = -3f;

    [Header("카메라 이동 설정")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 0, -10f);
    [SerializeField] private float followSpeed = 5f;

    [Header("추적 대상들 (여러 플레이어들)")]
    public List<Transform> multipleTargets = new();

    private bool isFollowing = false;

    private bool isBattleActive = false;  // 전투 중 여부
    /// <summary>
    /// 외부에서 추적할 여러 유닛을 설정
    /// </summary>
    public void SetTargets(List<Transform> targets)
    {
        multipleTargets = targets;
    }

    public void StartFollowing(List<Transform> targets)
    {
        multipleTargets = targets;
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }

    /// <summary> 전투 상태 토글 </summary>
    public void SetBattleActive(bool active)
    {
        isBattleActive = active;
    }

    private void LateUpdate()
    {
        // 전투 중 & 팔로우 활성화일 때만 추적
        if (!isBattleActive || !isFollowing || multipleTargets == null || multipleTargets.Count == 0)
            return;

        Vector3 center = GetCenterPoint();

        float extraYOffset = (GatchaPanel.activeSelf || InventoryPanel.activeSelf) ? uiYOffset : 0f;
        Vector3 totalOffset = baseOffset + new Vector3(0, extraYOffset, 0);

        Vector3 desiredPosition = center + totalOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 현재 유닛들의 중심 위치 계산
    /// </summary>
    private Vector3 GetCenterPoint()
    {
        // 살아있는(Active) 타겟만 필터링
        var validTargets = multipleTargets
            .Where(t => t != null && t.gameObject.activeInHierarchy)
            .ToList();

        if (validTargets.Count == 0)
            return transform.position; // 남은 대상 없으면 카메라 안 움직이게

        if (validTargets.Count == 1)
            return validTargets[0].position;

        Bounds bounds = new Bounds(validTargets[0].position, Vector3.zero);
        for (int i = 1; i < validTargets.Count; i++)
            bounds.Encapsulate(validTargets[i].position);

        return bounds.center;
    }

    private void OnDrawGizmos()
    {
        if (multipleTargets == null || multipleTargets.Count == 0)
            return;

        // 중심 좌표 계산
        Vector3 center = GetCenterPoint();

        // 색상 및 크기 설정
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(center, 0.3f); // 중심 위치에 구 표시
    }
}
