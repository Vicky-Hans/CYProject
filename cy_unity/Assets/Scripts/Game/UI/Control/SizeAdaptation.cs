using UnityEngine;

namespace Game.UI.Control
{
    public class SizeAdaptation : MonoBehaviour
    {
        public RectTransform node;
        public float interval;

        private RectTransform rectTf;
        private void Start()
        {
            rectTf = GetComponent<RectTransform>();
            AdaptBackgroundSize();
        }

        private void Update()
        {
            AdaptBackgroundSize();
        }

        private void AdaptBackgroundSize()
        {
            if(rectTf==null || node==null) return;
            rectTf.sizeDelta = new Vector2(rectTf.sizeDelta.x, node.sizeDelta.y + interval);
        }
    }
}