using System;
using UnityEngine;

namespace DH.Game
{
    public class PlayerHpBar : MonoBehaviour
    {
        public SpriteRenderer bar;
        public SpriteRenderer armorBar;
        private const float Bar_w = 2.48f;
        public CharacterController PlayerMono { get; set; }

        private void Start()
        {
            if (bar == null)
            {
                return;
            }
            bar.size = new Vector2(Bar_w, bar.size.y);
            armorBar.size = new Vector2(Bar_w, armorBar.size.y);
        }

        public void SetPercent(float percent)
        {
            if (bar == null)
            {
                return;
            }
            bar.size = new Vector2(Bar_w * percent, bar.size.y);
        }

        public void SetArmorPercent(float percent)
        {
            if(armorBar == null)return;
            armorBar.size = new Vector2(Bar_w * percent, armorBar.size.y);
        }

        public void OnUpdate(float deltaTime)
        {
            if(PlayerMono == null)return;
            SetPercent(PlayerMono.Player.resource.Progress);
            SetArmorPercent(PlayerMono.Player.resource.ArmorProgress);
        }
    }
}