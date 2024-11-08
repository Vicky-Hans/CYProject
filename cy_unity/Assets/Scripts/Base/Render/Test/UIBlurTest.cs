using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Base.Render.Test
{
    public class UIBlurTest : MonoBehaviour
    {
        public RawImage bg;

        private void OnEnable()
        {
            bg.color = new Color(1, 1, 1, 0);
            StartCoroutine(OpenBG());
        }

        IEnumerator OpenBG()
        {
            yield return UIBlurManager.Instance.GrabScreenBlur();

            yield return null;
            
            bg.texture = UIBlurManager.Instance.screenRenderTexture;
            bg.color = Color.white;
        }
        
        private void OnDestroy()
        {
            UIBlurManager.Instance.Release();
        }
    }
}