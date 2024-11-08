using System;
using UIFramework.Binding;
using UnityEngine;

namespace Game.Common.TabGroup
{
    public class UIRootAdapter : MonoBehaviour
    {
        private void Awake()
        {
            var rectTrans = GetComponent<RectTransform>();
            AdaptTrans(rectTrans);
        }
        
        private void AdaptTrans(RectTransform rectTransform)
        {
            var screenRatio = Screen.width / (float)Screen.height;
            if(screenRatio > 1080f / 1920f)
            {
                rectTransform.sizeDelta = new Vector2(1080f, 1920f);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(1080f, 1920f * Screen.width/1080f);
            }
        }
    }
}