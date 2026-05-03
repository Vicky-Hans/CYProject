using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    public class CircularScrollItemAdapting : MonoBehaviour
    {
        private int direction;
        private Action<int, float> onItemSizeChangedCallback;
        private int circularItemIdx;
        private RectTransform transRt;

        public void Init(int idx, int dir, Action<int, float> callback)
        {
            direction = dir;
            onItemSizeChangedCallback = callback;
            circularItemIdx = idx;
        }

        private void Awake()
        {
            transRt = transform as RectTransform;
        }

        private void OnRectTransformDimensionsChange()
        {
            onItemSizeChangedCallback?.Invoke(circularItemIdx, GetItemSize());
        }

        public float GetItemSize()
        {
            return direction == 0 ? transRt.sizeDelta.x : transRt.sizeDelta.y;
        }
    }
}