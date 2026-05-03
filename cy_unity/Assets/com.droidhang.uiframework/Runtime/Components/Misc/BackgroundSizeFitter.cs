using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    public class BackgroundSizeFitter : MonoBehaviour
    {
        [Serializable]
        public class SizeData
        {
            public RectTransform rectTransform;
            public Vector2 original;

            internal OnTransformDimensionChanged rectChangeListener;
        }

        public SizeData left;
        public SizeData right;
        public bool horizontal;
        
        private Vector2 originalSizeDelta;
        private RectTransform rectTransform;
        private Vector2 sizeChanged;
        private bool sizeDirty;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            originalSizeDelta = rectTransform.sizeDelta;
            left.rectChangeListener = left.rectTransform.GetComponent<OnTransformDimensionChanged>();
            left.rectChangeListener.OnSizeChangedRect += OnSizeChanged;
            right.rectChangeListener = right.rectTransform.GetComponent<OnTransformDimensionChanged>();
            right.rectChangeListener.OnSizeChangedRect += OnSizeChanged;
        }

        private void LateUpdate()
        {
            if (!sizeDirty)
            {
                return;
            }

            rectTransform.sizeDelta = originalSizeDelta + sizeChanged;
            sizeDirty = false;
        }

        private void OnSizeChanged(RectTransform rectTrans)
        {
            sizeChanged = Vector2.zero;
            Vector2 maxSize = Vector2.zero;
            sizeDirty = true;
            if (horizontal)
            {
                float leftX = left.rectTransform.sizeDelta.x;
                float rightX = right.rectTransform.sizeDelta.x;
                maxSize.x = leftX > rightX ? leftX : rightX;
                sizeChanged = (maxSize - left.original) * 2;
                sizeChanged.y = 0;
            }
            else
            {
                float leftY = left.rectTransform.sizeDelta.y;
                float rightY = right.rectTransform.sizeDelta.y;
                maxSize.x = leftY > rightY ? leftY : rightY;
                sizeChanged = (maxSize - left.original) * 2;
                sizeChanged.x = 0;
            }
        }
    }
}