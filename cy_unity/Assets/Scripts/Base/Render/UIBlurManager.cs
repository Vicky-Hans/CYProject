using System;
using System.Collections;
using UnityEngine;

namespace Base.Render
{
    public class UIBlurManager
    {
        public bool NeedGrabScreen { get; internal set; }

        internal RenderTexture screenRenderTexture;

        private static UIBlurManager instance;
        public static UIBlurManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIBlurManager();
                }

                return instance;
            }
        }

        private UIBlurManager()
        {
            
        }

        public void Release()
        {
            if (screenRenderTexture != null)
            {
                screenRenderTexture.Release();
                screenRenderTexture = null;
            }
        }

        public IEnumerator GrabScreenBlur()
        {
            NeedGrabScreen = true;
            yield return null;
        }

        internal void CreateRenderTexture(RenderTextureDescriptor desc)
        {
            if (screenRenderTexture == null)
            {
                screenRenderTexture = new RenderTexture(desc);
            }
        }

    }
}