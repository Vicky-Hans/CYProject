using TMPro;
using UnityEngine;

namespace Game.UI.Control
{
    public class BackgroundAdaptation : MonoBehaviour
    {
        public TextMeshProUGUI text;
        private RectTransform background;

        public float interval;
        private void Start()
        {
            background = gameObject.GetComponent<RectTransform>();
            if(interval==0)
                interval = background.sizeDelta.x - text.preferredWidth;
            AdaptBackgroundSize();
        }

        private void Update()
        {
            AdaptBackgroundSize();
        }

        private void AdaptBackgroundSize()
        {
            var textWidth = text.preferredWidth;
            var backgroundSize = new Vector2(textWidth+interval, background.sizeDelta.y);
            background.sizeDelta = backgroundSize;
        }
    }
}