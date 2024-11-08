using System;
using UnityEngine;

namespace DH.Game
{
    public class EnemyHpBar : MonoBehaviour
    {
        public SpriteRenderer bar;
        private const float Bar_w = 2.48f;

        private void Start()
        {
            if (bar == null)
            {
                return;
            }
            bar.size = new Vector2(Bar_w, bar.size.y);
        }

        public void SetPercent(float percent)
        {
            if (bar == null)
            {
                return;
            }
            bar.size = new Vector2(Bar_w * percent, bar.size.y);
        }
    }
}