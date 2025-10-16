using System;
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
        public Action Clicked;

        private RectTransform self;


        void Awake()
        {
            self = (RectTransform)transform;
            rootCanvas = GetComponentInParent<Canvas>();
            
            var img = GetComponent<Image>();
            img.color = new Color(255, 255, 255, 0.4f);
            img.raycastTarget = true;
        }

        private void OnDisable()
        {
            target = null;
            button = null;
            Clicked = null;
        }

        void LateUpdate()
        {
            if (!target || !rootCanvas) return;
            
            Vector3[] world = new Vector3[4];
            target.GetWorldCorners(world);
            
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, world[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, world[2]);
            
            Vector2 size = max - min;
            Vector2 center = min + size * 0.5f;
            
            size += padding * 2f;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform, 
                center,
                cam, 
                out var local);
            
            self.anchoredPosition = local;
            self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button)
            {
                button?.onClick.Invoke();
            }
            
            Clicked?.Invoke();
        }
    }
}