using Base.Render;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DH.Base
{
    public class UIBlurFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Range(1, 8)] public int DownscaleAmount = 2;
            
            [Range(0, 8)] public int BlurIterations = 4;
            
            public Shader BlurShader;

            public RenderPassEvent PassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        class BlurSceneColorPass : ScriptableRenderPass
        {
            private ScriptableRenderer renderer;
            private RenderTargetHandle tempColorTarget1;
            private RenderTargetHandle tempColorTarget2;
            private Settings settings;
            private Material blurMaterial;

            public BlurSceneColorPass(Settings s)
            {
                settings = s;
                renderPassEvent = s.PassEvent;
                tempColorTarget1.Init("dhTempColorTarget1");
                tempColorTarget1.Init("dhTempColorTarget2");
                blurMaterial = CoreUtils.CreateEngineMaterial(settings.BlurShader);
            }

            public void Setup(ScriptableRenderer renderer)
            {
                this.renderer = renderer;
            }

            public void Dispose()
            {
                CoreUtils.Destroy(blurMaterial);
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                var downscaleDesc = cameraTextureDescriptor;
                downscaleDesc.width = (int) ((float) downscaleDesc.width / (float) settings.DownscaleAmount);
                downscaleDesc.height = (int) ((float) downscaleDesc.height / (float) settings.DownscaleAmount);
                cmd.GetTemporaryRT(tempColorTarget1.id, downscaleDesc);
                cmd.GetTemporaryRT(tempColorTarget2.id, downscaleDesc);
                
                UIBlurManager.Instance.CreateRenderTexture(downscaleDesc);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                cmd.Blit(renderer.cameraColorTarget, tempColorTarget2.Identifier());
                for (int i = 0; i < settings.BlurIterations; i++)
                {
                    cmd.Blit(tempColorTarget2.Identifier(), tempColorTarget1.Identifier(), blurMaterial, 0);
                    cmd.Blit(tempColorTarget1.Identifier(), tempColorTarget2.Identifier(), blurMaterial, 1);
                }

                if (UIBlurManager.Instance.NeedGrabScreen)
                {
                    cmd.Blit(tempColorTarget2.Identifier(), UIBlurManager.Instance.screenRenderTexture);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(tempColorTarget1.id);
                cmd.ReleaseTemporaryRT(tempColorTarget2.id);
                UIBlurManager.Instance.NeedGrabScreen = false;
            }
        }

        BlurSceneColorPass blurSceneColorPass;
        [SerializeField] Settings settings;

        public override void Create()
        {
            blurSceneColorPass = new BlurSceneColorPass(settings);
        }

        protected override void Dispose(bool disposing)
        {
            blurSceneColorPass?.Dispose();
            blurSceneColorPass = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!UIBlurManager.Instance.NeedGrabScreen)
            {
                return;
            }
            
            blurSceneColorPass.Setup(renderer);
            renderer.EnqueuePass(blurSceneColorPass);
        }
    }
}