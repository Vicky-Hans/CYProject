using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DH.Game.UIViews
{
    public class ButtonComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        /// <summary>
        /// 按钮被按下的回调
        /// </summary>
        public Action OnButtonPressed;
        /// <summary>
        /// 按钮释放的回调
        /// </summary>
        public Action OnButtonReleased;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnButtonPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnButtonReleased?.Invoke(); 
        }
    }
}