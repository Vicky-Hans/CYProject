using System;
using TMPro;
using UnityEngine;

namespace Game.UI.Control
{
    public class LayoutWithTextSize : TextChangeListener
    {
        public TextMeshProUGUI text;
        public RectTransform rightItem;
        public RectTransform selfRect;
        public float paddingLeft;
        public float paddingRight;

        [ContextMenu("SetSize")]
        public override void OnTextChanged()
        {
            var textRect = (text.transform as RectTransform);
            var textSize = text.GetPreferredValues().x;
            var offset = textSize * 0.5f + paddingLeft;
            var textComPosition = new Vector2(offset, textRect.anchoredPosition.y);
            offset += textSize * 0.5f + rightItem.sizeDelta.x * 0.5f;
            var rightItemPos = new Vector2(offset, rightItem.anchoredPosition.y);
            textRect.anchoredPosition = textComPosition;
            rightItem.anchoredPosition = rightItemPos;
            var size = textRect.sizeDelta;
            size.x = textSize;
            textRect.sizeDelta = size;

            size = selfRect.sizeDelta;
            size.x = textSize + paddingLeft + paddingRight + rightItem.sizeDelta.x;
            selfRect.sizeDelta = size;
        }
    }
}