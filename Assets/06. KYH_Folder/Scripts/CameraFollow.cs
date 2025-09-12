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
    [SerializeField] private List<Transform> multipleTargets = new();

    /// <summary>
    /// 외부에서 추적할 여러 유닛을 설정
    /// </summary>
    public void SetTargets(List<Transform> targets)
    {
        multipleTargets = targets;
    }

    private void LateUpdate()
    {
        if (multipleTargets == null || multipleTargets.Count == 0)
            return;

        // 중심점 계산
        Vector3 center = GetCenterPoint();

        // UI 오프셋 계산
        float extraYOffset = (GatchaPanel.activeSelf || InventoryPanel.activeSelf) ? uiYOffset : 0f;
        Vector3 totalOffset = baseOffset + new Vector3(0, extraYOffset, 0);

        // 최종 위치 계산 및 이동
        Vector3 desiredPosition = center + totalOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 현재 유닛들의 중심 위치 계산
    /// </summary>
    private Vector3 GetCenterPoint()
    {
        if (multipleTargets.Count == 1)
            return multipleTargets[0].position;

        Bounds bounds = new Bounds(multipleTargets[0].position, Vector3.zero);
        for (int i = 1; i < multipleTargets.Count; i++)
            bounds.Encapsulate(multipleTargets[i].position);

        return bounds.center;
    }
}
