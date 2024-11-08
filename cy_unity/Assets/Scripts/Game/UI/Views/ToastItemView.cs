using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;

namespace DH.Game.UIViews
{
    public class ToastItemView : BaseView
    {
        public override bool FullScreen => false;
        private float timer;

        public RectTransform selfRectTransform;
        public float duration = 5;
        public float speed = 5;
        public TextMeshProUGUI desc;

        public bool IsComplete => timer >= duration;

        public void Awake()
        {
            ApplySortingOrder(10000);
        }

        public void UpdateInfo(string text)
        {
            timer = 0;
            desc.text = text;
        }

        public void UpdatePosition(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= duration)
            {
                return;
            }
            
            selfRectTransform.anchoredPosition += new Vector2(0, speed * deltaTime);
        }
        
    }
}