using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TutorialHighlighter : MonoBehaviour
{
    [Header("강조할 대상 UI")]
    public RectTransform target;

    [Header("구멍 주변 여백")]
    public Vector2 padding = new Vector2(10f, 10f);

    private Image overlayImage;
    private Material materialInstance;
    private Canvas rootCanvas;

    void Awake()
    {
        overlayImage = GetComponent<Image>();

        // 머티리얼 복제본 사용
        materialInstance = new Material(overlayImage.material);
        overlayImage.material = materialInstance;

        // 가장 가까운 Canvas 찾기
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (target != null)
        {
            UpdateHole();
        }
    }

    /// <summary>
    /// 강조할 대상을 바꿀 때 호출
    /// </summary>
    public void SetTarget(RectTransform newTarget)
    {
        target = newTarget;
        UpdateHole(); // 즉시 갱신
    }

    private void UpdateHole()
    {
        if (target == null || rootCanvas == null) return;

        // 목표 UI의 월드 좌표 모서리
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // 중심점 (월드 → 스크린 → UV)
        Vector3 centerWorld = (corners[0] + corners[2]) / 2f;
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(null, centerWorld);

        Rect canvasRect = rootCanvas.pixelRect;

        // UV(0~1)로 변환
        Vector2 uvCenter = new Vector2(
            screenCenter.x / canvasRect.width,
            screenCenter.y / canvasRect.height
        );

        // 크기도 UV 비율로 변환
        float width = (Mathf.Abs(corners[2].x - corners[0].x) + padding.x) / canvasRect.width;
        float height = (Mathf.Abs(corners[2].y - corners[0].y) + padding.y) / canvasRect.height;

        // 셰이더에 전달
        materialInstance.SetVector("_HoleCenter", uvCenter);
        materialInstance.SetVector("_HoleSize", new Vector2(width, height));
    }

    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
