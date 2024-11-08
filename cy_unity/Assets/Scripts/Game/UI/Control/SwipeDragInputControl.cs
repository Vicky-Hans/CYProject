using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Control
{
    public class SwipeDragInputControl : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public enum LerpType
        {
            None,
            ToMax,
            ToMin,
        }
        
        [Header("拖拽方向")]
        public Vector2 direction;
        [Header("拖拽阈值")]
        public float threshold;

        public float lerpTime = 0.1f;

        public Vector2 sizeDeltaMax;
        public Vector2 sizeDeltaMin;

        private bool maxState;
        private Vector2 startPosition;
        private RectTransform rectTransform;
        private LerpType lerpType;
        private float lerpTimer;

        private void Start()
        {
            rectTransform = (RectTransform)transform;
            sizeDeltaMax.x = sizeDeltaMin.x = rectTransform.sizeDelta.x;
        }

        private void Update()
        {
            if (lerpType == LerpType.None)
            {
                return;
            }
            
            lerpTimer += Time.deltaTime;
            if (lerpTimer > lerpTime)
            {
                rectTransform.sizeDelta = lerpType == LerpType.ToMax ? sizeDeltaMax : sizeDeltaMin;
                lerpType = LerpType.None;
                return;
            }
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta,
                lerpType == LerpType.ToMax ? sizeDeltaMax : sizeDeltaMin, lerpTimer);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var uiCamera = AppGlobal.Instance.UICamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                eventData.position, uiCamera, out startPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var uiCamera = AppGlobal.Instance.UICamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                eventData.position, uiCamera, out var currentPosition);
            // 从小切换为大状态
            var dotResult = Vector2.Dot(currentPosition - startPosition, maxState ? -direction : direction);
            if (maxState)
            {
                if (dotResult < 0)
                {
                    rectTransform.sizeDelta = sizeDeltaMax;
                }
                else
                {
                    var size = new Vector2(sizeDeltaMax.x, sizeDeltaMax.y - dotResult);
                    if (size.y < sizeDeltaMin.y)
                    {
                        size.y = sizeDeltaMin.y;
                    }

                    rectTransform.sizeDelta = size;
                }
            }
            else
            {
                if (dotResult < 0)
                {
                    rectTransform.sizeDelta = sizeDeltaMin;
                }
                else
                {
                    var size = new Vector2(sizeDeltaMin.x, sizeDeltaMin.y + dotResult);
                    if (size.y > sizeDeltaMax.y)
                    {
                        size.y = sizeDeltaMax.y;
                    }

                    rectTransform.sizeDelta = size;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var uiCamera = AppGlobal.Instance.UICamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                eventData.position, uiCamera, out var currentPosition);
            // 从小切换为大状态
            var dotResult = Vector2.Dot(currentPosition - startPosition, maxState ? -direction : direction);
            if (maxState)
            {
                if (dotResult < 0)
                {
                    rectTransform.sizeDelta = sizeDeltaMax;
                }
                else
                {
                    var size = new Vector2(sizeDeltaMax.x, sizeDeltaMax.y - dotResult);
                    if (size.y < sizeDeltaMin.y)
                    {
                        size.y = sizeDeltaMin.y;
                        lerpType = LerpType.None;
                        maxState = false;
                    }
                    else if (dotResult < threshold)
                    {
                        lerpType = LerpType.ToMax;
                        maxState = true;
                    }
                    else
                    {
                        lerpType = LerpType.ToMin;
                        maxState = false;
                    }
                    rectTransform.sizeDelta = size;
                }
            }
            else
            {
                // 从小切换为大状态
                if (dotResult < 0)
                {
                    rectTransform.sizeDelta = sizeDeltaMin;
                }
                else
                {
                    var size = new Vector2(sizeDeltaMin.x, sizeDeltaMin.y + dotResult);
                    if (size.y > sizeDeltaMax.y)
                    {
                        size.y = sizeDeltaMax.y;
                        lerpType = LerpType.None;
                        maxState = true;
                    }
                    else if (dotResult < threshold)
                    {
                        lerpType = LerpType.ToMin;
                        maxState = false;
                    }
                    else
                    {
                        lerpType = LerpType.ToMax;
                        maxState = true;
                    }

                    lerpTimer = 0;
                    rectTransform.sizeDelta = size;
                }
            }
        }
    }
}