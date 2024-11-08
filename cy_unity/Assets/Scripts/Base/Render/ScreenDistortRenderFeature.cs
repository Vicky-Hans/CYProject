using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace QD.Base
{
    public class ScreenDistortRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class ScreenDistortSettings
        {
            public RenderPassEvent passEvent;
            public Material material;
        }

        public ScreenDistortSettings settings = new ScreenDistortSettings();
        private ScreenDistortRenderPass renderPass;
        
        public override void Create()
        {
            renderPass = new ScreenDistortRenderPass(settings.passEvent, settings.material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderPass.Setup(renderer);
            renderer.EnqueuePass(renderPass);
        }

        private class ScreenDistortRenderPass : ScriptableRenderPass
        {
            public Material blitMaterial;
            private RenderTargetHandle temporaryColorTexture;
            private string profilerTag;
            private ScriptableRenderer scriptableRenderer;

            public ScreenDistortRenderPass(RenderPassEvent renderPassEvent, Material blitMaterial)
            {
                this.renderPassEvent = renderPassEvent;
                this.blitMaterial = blitMaterial;
                profilerTag = "ScreenDistort";
                temporaryColorTexture.Init("_TemporaryColorTexture");
            }

            public void Setup(ScriptableRenderer renderer)
            {
                scriptableRenderer = renderer;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(profilerTag);

                var opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;

                cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDesc);
                Blit(cmd, scriptableRenderer.cameraColorTargetHandle, temporaryColorTexture.Identifier(), blitMaterial);
                Blit(cmd, temporaryColorTexture.Identifier(), scriptableRenderer.cameraColorTargetHandle);
                cmd.ReleaseTemporaryRT(temporaryColorTexture.id);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
