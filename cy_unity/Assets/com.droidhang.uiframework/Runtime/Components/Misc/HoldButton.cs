using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public class HoldButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerMoveHandler
    {
        public enum HoldState
        {
            None,
            Down,
            Hold,
            Click, 
        }

        [Header("按钮长按时间(S)")]
        public float holdTime = 2;
        public bool enableDebug;
        
        public Button.ButtonClickedEvent OnClick
        {
            get { return onClick; }
            set { onClick = value; }
        }
        
        public Button.ButtonClickedEvent OnHoldClick
        {
            get { return onHoldClick; }
            set { onHoldClick = value; }
        }
        
        private Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();
        private Button.ButtonClickedEvent onHoldClick = new Button.ButtonClickedEvent();

        private HoldState state = HoldState.None;
        private float timer = 0;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            state = HoldState.Down;
            timer = 0;
            if (enableDebug)
            {
                Debug.Log("OnPointerDown");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (state == HoldState.Down)
            {
                state = HoldState.Click;
                onClick.Invoke();
                if (enableDebug)
                {
                    Debug.Log($"Normal click");
                }
            }
            
            if (enableDebug)
            {
                Debug.Log("OnPointerUp");
            }
        }

        private void Update()
        {
            if (state != HoldState.Down)
            {
                return;
            }

            timer += Time.unscaledDeltaTime;
            if (timer > holdTime)
            {
                state = HoldState.Hold;
                onHoldClick.Invoke();
                if (enableDebug)
                {
                    Debug.Log($"Hold click");
                }
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            state = HoldState.None;
            timer = 0;
        }
    }
}