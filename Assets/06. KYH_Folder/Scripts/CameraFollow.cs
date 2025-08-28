using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    [SerializeField] private float uiYOffset = -3f;

    [Header("UI 패널 (활성화 여부 감지)")]
    [SerializeField] private GameObject GatchaPanel;
    [SerializeField] private GameObject InventoryPanel;

    [SerializeField] private Vector3 baseOffset = new Vector3(0, 0, -10f); // 2D용 Z 거리
    [SerializeField] private float followSpeed = 5f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 패널이 하나라도 켜져 있으면 Y축 추가 오프셋 적용
        float extraYOffset = (GatchaPanel.activeSelf || InventoryPanel.activeSelf) ? uiYOffset : 0f;

        // 최종 오프셋 계산
        Vector3 totalOffset = baseOffset + new Vector3(0, extraYOffset, 0);

        Vector3 desiredPosition = target.position + totalOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}
