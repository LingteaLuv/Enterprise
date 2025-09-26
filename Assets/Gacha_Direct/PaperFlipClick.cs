using UnityEngine;

/// <summary>
/// 이 컴포넌트를 카드(PaperFlip)와 함께 붙이면
/// 마우스/터치로 해당 카드만 1회 뒤집을 수 있습니다.
/// </summary>
[RequireComponent(typeof(PaperFlip))]
public class PaperFlipClick : MonoBehaviour
{
    public PaperFlip target;           // 비우면 자동 할당
    public Camera rayCamera;           // 비우면 Camera.main
    public LayerMask layerMask = ~0;   // 전 레이어

    void Awake()
    {
        if (!target) target = GetComponent<PaperFlip>();

        // 클릭용 Collider 보장
        if (!GetComponent<Collider>())
        {
            var bc = gameObject.AddComponent<BoxCollider>();
            var sz = (target != null) ? new Vector3(target.size.x, target.size.y, 0.1f)
                                      : new Vector3(1f, 1.5f, 0.1f);
            bc.size = sz;
        }
        if (!rayCamera) rayCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!rayCamera) return;
            Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, layerMask))
            {
                if (hit.collider && hit.collider.gameObject == gameObject)
                {
                    target?.TryPlayFlip(); // 1회 가드 내장
                }
            }
        }
    }
}
