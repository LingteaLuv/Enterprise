using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{


    public class TutorialHighlighterV2 : MonoBehaviour
    {
        [Header("Rects")] public RectTransform leftDim, rightDim, topDim, bottomDim;
        public RectTransform border; 
        
        public Camera uiCam; 
        
        [Header("하이라이트 최대값")]
        [SerializeField] private float extendX = 40f;
        [SerializeField] private float extendY = 30f;
        
        [Header("패딩")]
        [SerializeField] private Vector2 padding = new(12, 12);

        RectTransform self;
        RectTransform _target;
        void Awake() => self = (RectTransform)transform;

        public void Enable(bool v) => gameObject.SetActive(v);

        public void SetTarget(RectTransform target)
        {
            _target = target;
            RefreshLayout();
        }
        

        public void RefreshLayout()
        {
            if (_target == null)
            {
                Enable(false);
                return;
            }

            Enable(true);

            var canvas = GetComponentInParent<Canvas>();
            var cam = canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (uiCam ?? canvas.worldCamera);

            // 타깃 화면 사각형 계산
            var w = new Vector3[4];
            _target.GetWorldCorners(w);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, w[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, w[2]);
            Vector2 size = max - min;
            
            size.x += padding.x * 2f;
            size.y += padding.y * 2f;
            Vector2 center = min + (max - min) * 0.5f;

            // HighLighter(=self) 로컬좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(self, center, cam, out var localCenter);

            // 캔버스 크기
            var canvasRT = (RectTransform)canvas.transform;
            float halfW = canvasRT.rect.width * 0.5f;
            float halfH = canvasRT.rect.height * 0.5f;

            // 타깃 로컬 좌표계 사각형
            float left = localCenter.x - size.x * 0.5f;
            float right = localCenter.x + size.x * 0.5f;
            float top = localCenter.y + size.y * 0.5f;
            float bottom = localCenter.y - size.y * 0.5f;

            // 좌/우 패널의 세로 범위: 타깃 높이 + 2*extendY (캔버스 안으로 clamp)
            float vSpan     = Mathf.Min(canvasRT.rect.height,size.y + 2f * extendY);
            float vMinLocal = Mathf.Clamp(localCenter.y - vSpan * 0.5f, -halfH, halfH);
            float vMaxLocal = Mathf.Clamp(localCenter.y + vSpan * 0.5f, -halfH, halfH);

            // 좌 패널의 가로 범위: [left - extendX, left]
            float leftMinX  = Mathf.Max(-halfW, left - extendX);
            SetRect(leftDim,  new Vector2(leftMinX, vMinLocal), new Vector2(left, vMaxLocal));

            // 우 패널의 가로 범위: [right, right + extendX]
            float rightMaxX = Mathf.Min( halfW, right + extendX);
            SetRect(rightDim, new Vector2(right, vMinLocal), new Vector2(rightMaxX, vMaxLocal));

            // 상/하 패널의 가로 범위: 타깃 너비 + 2*extendX (캔버스 안으로 clamp)
            float hSpan     = Mathf.Min(canvasRT.rect.width,size.x + 2f * extendX);
            float hMinLocal = Mathf.Clamp(localCenter.x - hSpan * 0.5f, -halfW, halfW);
            float hMaxLocal = Mathf.Clamp(localCenter.x + hSpan * 0.5f, -halfW, halfW);

            // 상 패널의 세로 범위: [top, top + extendY]
            float topMaxY   = Mathf.Min( halfH, top + extendY);
            SetRect(topDim,    new Vector2(hMinLocal, top),    new Vector2(hMaxLocal, topMaxY));

            // 하 패널의 세로 범위: [bottom - extendY, bottom]
            float botMinY   = Mathf.Max(-halfH, bottom - extendY);
            SetRect(bottomDim, new Vector2(hMinLocal, botMinY), new Vector2(hMaxLocal, bottom));
            if (border)
            {
                border.anchoredPosition = localCenter;
                border.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                border.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }

        static void SetRect(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = (min + max) * 0.5f;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(0, max.x - min.x));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(0, max.y - min.y));
            rt.gameObject.SetActive((max.x - min.x) > 0.5f && (max.y - min.y) > 0.5f);
        }

    }
}