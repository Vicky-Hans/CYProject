using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace DH.Game
{

    public enum EStickType
    {
        /// <summary>
        /// 遥杆
        /// </summary>
        Stick,
        /// <summary>
        /// 方向键
        /// </summary>
        Arrow,
    }

    /// <summary>
    /// A stick control displayed on screen and moved around by touch or other pointer
    /// input.
    /// </summary>
    [AddComponentMenu("Input/Custom On-Screen Stick")]
    public class CustomScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        
        
        [Header("摇杆类型")]
        public EStickType stickType = EStickType.Stick;
        [Header("摇杆可以移动的部分")]
        public RectTransform stick;
        public RectTransform stickPanel;
        public float movementRange;
        [Header("摇杆灵敏距离")]
        public float moveThreshold;
        [InputControl(layout = "Vector2")]
        public string customControlPath;
        public Color normalColor;
        public Color dragColor;
        public RectTransform arrowPanel;
        public RectTransform arrowTop;
        public RectTransform arrowRight;
        public RectTransform arrowDown;
        public RectTransform arrowLeft;

        private Vector3 startPosition;
        private Vector2 pointerDownPos;
        private Vector3 initPosition;
        private Image stickImg;
        private Image stickPanelImg;
        private Image arrowPanelImg;
        private static readonly Color TransColor1 = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TransColor2 = new Color(1f, 1f, 1f, 0f);

        protected override string controlPathInternal
        {
            get => customControlPath;
            set => customControlPath = value;
        }

        private void Start()
        {
            var position = stick.position;
            stickPanel.position = position;
            stick.anchorMin = new Vector2(0.5f,0.5f);
            stick.anchorMax = new Vector2(0.5f,0.5f);
            stick.position = position;
            initPosition = stick.anchoredPosition;
            stickImg = stick.GetComponent<Image>();
            stickPanelImg = stickPanel.GetComponent<Image>();
            arrowPanelImg = arrowPanel.GetComponent<Image>();
            OnStickActive(false);
            UpdateArrow(Vector2.zero);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out pointerDownPos);

            stick.anchoredPosition = pointerDownPos;
            startPosition = pointerDownPos;
            stickPanel.anchoredPosition = startPosition;
            arrowPanel.anchoredPosition = startPosition;
            OnStickActive(true);
            UpdateArrow(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
            var delta = position - pointerDownPos;

            delta = Vector2.ClampMagnitude(delta, movementRange);
            stick.anchoredPosition = startPosition + (Vector3)delta;

            var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
            if (delta.magnitude < movementRange / 3)
            {
                newPos = Vector2.zero;
            }
            UpdateArrow(newPos);
            SendValueToControl(newPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            stick.anchoredPosition = initPosition;
            stickPanel.anchoredPosition = initPosition;
            OnStickActive(false);
            UpdateArrow(Vector2.zero);
            SendValueToControl(Vector2.zero);
        }

        private void UpdateArrow(Vector2 delta)
        {
            arrowTop.gameObject.SetActive(delta.y > 0 && Mathf.Abs(delta.y) > Mathf.Abs(delta.x));
            arrowRight.gameObject.SetActive(delta.x > 0 && Mathf.Abs(delta.y) < Mathf.Abs(delta.x));
            arrowDown.gameObject.SetActive(delta.y < 0 && Mathf.Abs(delta.y) > Mathf.Abs(delta.x));
            arrowLeft.gameObject.SetActive(delta.x < 0 && Mathf.Abs(delta.y) < Mathf.Abs(delta.x));
        }

        private void OnStickActive(bool active)
        {
            if (stickType == EStickType.Stick)
            {
                stickImg.SetColor(active?TransColor1:TransColor2);
                stickPanelImg.SetColor(active?TransColor1:TransColor2);
                arrowPanelImg.SetColor(TransColor2);
                arrowPanelImg.gameObject.SetActive(false);
            }
            else
            {
                arrowPanelImg.gameObject.SetActive(active);
                stickImg.SetColor(TransColor2);
                stickPanelImg.SetColor(TransColor2);
                arrowPanelImg.SetColor(active?TransColor1:TransColor2);    
            }
            
        }
    }
}

