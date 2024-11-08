using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Control
{
    public class NoBounceScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public ScrollRect scrollRect;
        public float bounceThreshold = 0.1f;

        private bool isDragging = false;

        private void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            ClampScrollPosition();
        }

        private void ClampScrollPosition()
        {
            if(scrollRect==null) return;
            Vector2 normalizedPosition = scrollRect.normalizedPosition;

            if (normalizedPosition.x < bounceThreshold)
            {
                normalizedPosition.x = bounceThreshold;
            }
            else if (normalizedPosition.x > 1f - bounceThreshold)
            {
                normalizedPosition.x = 1f - bounceThreshold;
            }

            if (normalizedPosition.y < bounceThreshold)
            {
                normalizedPosition.y = bounceThreshold;
            }
            else if (normalizedPosition.y > 1f - bounceThreshold)
            {
                normalizedPosition.y = 1f - bounceThreshold;
            }

            scrollRect.normalizedPosition = normalizedPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                ClampScrollPosition();
            }
        }
    }
}