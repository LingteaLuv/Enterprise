using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class TutorialClickForwarder : MonoBehaviour, IPointerClickHandler
    {
        [Header("Target")]
        public RectTransform target;
        public Canvas rootCanvas;

        [Header("Hole")] 
        public Vector2 padding = new(12, 12);
        
        
        public Button button;

        private RectTransform self;


        void Awake()
        {
            self = (RectTransform)transform;
            rootCanvas = GetComponentInParent<Canvas>();
            
            var img = GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = true;
        }

        void LateUpdate()
        {
            if (!target || !rootCanvas) return;
            
            Vector3[] world = new Vector3[4];
            target.GetWorldCorners(world);
            
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, world[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, world[2]);
            
            Vector2 size = max - min;
            Vector2 center = min + size * 0.5f;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform, 
                center,
                null, 
                out var local);
            
            self.anchoredPosition = local;
            self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            button?.onClick.Invoke();
        }
    }
}