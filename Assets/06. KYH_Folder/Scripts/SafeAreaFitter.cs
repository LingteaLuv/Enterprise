using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    Rect last;
    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        Apply(); // ✅ Start에서 안전하게 호출
    }

    void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    void Apply()
    {
        if (rt == null) return;

        var sa = Screen.safeArea;
        if (sa == last) return; // 같은 값이면 스킵
        last = sa;

        // 화면이 아직 준비 안 된 경우 방어
        if (Screen.width <= 0 || Screen.height <= 0) return;

        var min = sa.position;
        var max = sa.position + sa.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
    }
}
