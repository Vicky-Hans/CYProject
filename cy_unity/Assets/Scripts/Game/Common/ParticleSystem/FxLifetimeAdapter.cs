using System;
using UnityEngine;

namespace DH.Game
{
    public class FxLifetimeAdapter : MonoBehaviour
    {
        /// <summary>
        /// 方便编辑器查看
        /// </summary>
        #if UNITY_EDITOR
        [Serializable]
        #endif
        internal class FxItem
        {
            public ParticleSystem fx;
            public float refLifetime;
        }

        internal FxItem[] fxItems;
        /// <summary>
        /// 特效制作时设计的持续时间
        /// </summary>
        internal float preferLifetime;

        public void SetFxLifetime(float lifetime,bool autoPlay = true)
        {
            if (fxItems == null)
            {
                var fxs = GetComponentsInChildren<ParticleSystem>();
                fxItems = new FxItem[fxs.Length];
                for (int index = 0; index < fxs.Length; index++)
                {
                    var duration = fxs[index].main.duration;
                    if (duration > preferLifetime)
                    {
                        preferLifetime = duration;
                    }

                    fxItems[index] = new FxItem()
                    {
                        fx = fxs[index],
                        refLifetime = duration,
                    };
                }
            }
            fxItems[0].fx.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            
            float scale = lifetime / preferLifetime;
            foreach (var item in fxItems)
            {
                var main = item.fx.main;
                main.duration = item.refLifetime * scale;
            }

            if (autoPlay && fxItems.Length > 0)
            {
                fxItems[0].fx.Play();
            }
        }
    }
}