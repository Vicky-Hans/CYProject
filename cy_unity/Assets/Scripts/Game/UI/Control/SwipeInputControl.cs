using System;
using DHFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.UI.Control
{
    public class SwipeInputControl : MonoBehaviour,IBeginDragHandler,IEndDragHandler,IDragHandler
    {
        public Vector2 preferDirection;
        public float minMagnitude;

        public UnityEvent onSwipeEnd;
        public UnityEvent onSwipeInverseEnd;
        
        private Vector2 startPosition;
        private bool startSwipe;
        private RectTransform rectTransform;

        private void OnDisable()
        {
            startSwipe = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!rectTransform)
            {
                rectTransform = transform as RectTransform;
            }
            
            startSwipe = true;
            var uiCamera = AppGlobal.Instance.UICamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                uiCamera, out startPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!startSwipe)
            {
                return;
            }
            
            var uiCamera = AppGlobal.Instance.UICamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                uiCamera, out var endPosition);

            var delta = endPosition - startPosition;
            var temp = Vector3.Dot(preferDirection, delta);
            if (temp > minMagnitude && temp > 0)
            {
                onSwipeEnd.Invoke();
                DHLog.Debug("Swipe true");
            }
            else if (temp < -minMagnitude && temp < 0)
            {
                onSwipeInverseEnd.Invoke();
                DHLog.Debug("Swipe false");
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // do nothing
        }
    }
}