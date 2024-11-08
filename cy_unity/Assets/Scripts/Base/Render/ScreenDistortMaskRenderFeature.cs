using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace QD.Base
{
    public class ScreenDistortMaskRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class ScreenDistortMaskSettings
        {
            public RenderPassEvent passEvent;
            public LayerMask layerMask;
        }

        public ScreenDistortMaskSettings settings = new ScreenDistortMaskSettings();
        private ScreenDistortMaskRenderPass renderPass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderPass.Setup())
            {
                renderer.EnqueuePass(renderPass);
            }
        }

        public override void Create()
        {
            renderPass = new ScreenDistortMaskRenderPass(settings)
            {
                renderPassEvent = settings.passEvent,
            };
        }

        private class ScreenDistortMaskRenderPass : ScriptableRenderPass
        {
            private ShaderTagId shaderTag = new ShaderTagId("DistortMask");
            private ScreenDistortMaskSettings settings;
            private FilteringSettings filteringSettings;
            private RenderTargetHandle maskSoildColor;

            public ScreenDistortMaskRenderPass(ScreenDistortMaskSettings settings)
            {
                this.settings = settings;
                filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.layerMask);
                
                maskSoildColor.Init("_MaskSoildColor");
            }

            public bool Setup()
            {
                if (ReferenceEquals(null, settings))
                {
                    return false;
                }

                return true;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                var desc = cameraTextureDescriptor;
                desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                desc.width = desc.width >>1;
                desc.height = desc.height >>1;
                cmd.GetTemporaryRT(maskSoildColor.id, desc);
                
                ConfigureTarget(maskSoildColor.Identifier());
                ConfigureClear(ClearFlag.All, Color.clear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get();
                
                var drawMask = CreateDrawingSettings(shaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                context.DrawRenderers(renderingData.cullResults, ref drawMask, ref filteringSettings);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}