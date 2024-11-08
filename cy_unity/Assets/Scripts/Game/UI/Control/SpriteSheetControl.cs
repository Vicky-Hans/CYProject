using System;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game
{
    
    /// <summary>
    /// 用于多个Sprite切换的UI组件
    /// </summary>
    public class SpriteSheetControl : MonoBehaviour
    {
        public Sprite[] factionSprites;
        public long minValue = 2;
        
        private Image spriteIcon;
        public long Index
        {
            set
            {
                if (!spriteIcon)
                {
                    spriteIcon = GetComponent<Image>();
                    if (!spriteIcon)
                    {
                        throw new Exception("Require component UnityEngine.UI.Image");
                    }
                }
                
                spriteIcon.gameObject.SetActive(value >= minValue);
                if (value < minValue)
                {
                    return;
                }

                value -= minValue;
                var index = value >= factionSprites.Length ? factionSprites.Length - 1 : value;
                if(index < 0)
                {
                    index = 0;
                }
                
                spriteIcon.sprite = factionSprites[index];
                spriteIcon.sprite = factionSprites[index];
            }

            get => 0;
        }
    }
}