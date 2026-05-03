using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DH.UIFramework
{
    public class DragEventTriggerListener : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler,IBeginDragHandler,IDragHandler,IDropHandler,IEndDragHandler
    {
        /// <summary>
        /// 调回Lua点击按下句柄
        /// </summary>
        public Action<PointerEventData> PointerDownHandler;
        /// <summary>
        /// 调回Lua点击抬起句柄
        /// </summary>
        public Action<PointerEventData> PointerUpHandler;
        /// <summary>
        /// 调回Lua开始点击句柄
        /// </summary>
        public Action<PointerEventData> PointerClickHandle;
        /// <summary>
        /// 调回Lua开始拖拽句柄
        /// </summary>
        public Action<PointerEventData> BeginDragHandle;
        /// <summary>
        /// 调回Lua拖拽中句柄
        /// </summary>
        public Action<PointerEventData> OnDragHandle;
        /// <summary>
        /// 调回Lua结束拖拽句柄
        /// </summary>
        public Action<PointerEventData> EndDragHandle;
        //调回Lua结束拖拽句柄
        public Action<PointerEventData> DropDragHandle;
        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickHandle?.Invoke(eventData);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDragHandle?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragHandle?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragHandle?.Invoke(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            DropDragHandle?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownHandler?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpHandler?.Invoke(eventData);
        }
    }
}