using System;
using UnityEngine;

namespace DH.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    public class OnTransformDimensionChanged : MonoBehaviour
    {
        public Action<Vector2> OnSizeChanged;
        public Action<RectTransform> OnSizeChangedRect;
        
        private RectTransform rectTransform;
        
        private void OnRectTransformDimensionsChange()
        {
            if (!rectTransform)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            
            OnSizeChanged?.Invoke(rectTransform.sizeDelta);
            OnSizeChangedRect?.Invoke(rectTransform);
        }
    }
}