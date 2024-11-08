using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace QD.Base
{
    public class InlineGlowRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class InlineGlowSettings
        {
            public SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
            public RenderQueueRange renderQueueRange = RenderQueueRange.transparent;
            public RenderPassEvent PassEvent;
            public FilterMode RTFilterMode = FilterMode.Bilinear;
            public bool Use2048 = false;
            
            [Range(1, 5)]
            public float blurWidth = 1;

            [Range(1, 4)]
            public int iteration = 2;
            public Color glowColor = Color.red;
            public Material blurMaterial;
        }


        public InlineGlowSettings settings = new InlineGlowSettings();

        private InlineGlowRenderPass renderPass;

        public override void Create()
        {
            renderPass = new InlineGlowRenderPass()
            {
                settings = settings,
                renderPassEvent = settings.PassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderPass.Setup(renderer.cameraColorTarget))
            {
                renderer.EnqueuePass(renderPass);
            }
        }

        private class InlineGlowRenderPass : ScriptableRenderPass
        {
            private static readonly int inlineRenderTextureId = Shader.PropertyToID("_InlineRenderTexture");
            private static readonly int BlurRenderTextureId = Shader.PropertyToID("_BlurRT");
            private static readonly int BlurOffsetShaderId = Shader.PropertyToID("_BlurOffset");
            private static readonly int GlowColorShaderId = Shader.PropertyToID("_GlowColor");
            private static readonly int ShadowVPShaderId = Shader.PropertyToID("_ShadowVP");

            internal InlineGlowSettings settings;

            private ProfilingSampler pSampler = new ProfilingSampler("Inline Glow");

            private RenderTargetIdentifier inlineRT;
            private RenderTargetIdentifier blurRT;
            private ShaderTagId inlineMaskShaderTagId;

            public InlineGlowRenderPass()
            {
                inlineMaskShaderTagId = new ShaderTagId("InlineMask");
            }
            
            public bool Setup(RenderTargetIdentifier cameraColorRT)
            {
                if (settings == null || settings.blurMaterial == null)
                {
                    return false;
                }
                
                if (SpriteShadowmapSettings.Instance == null || !SpriteShadowmapSettings.Instance.IsEnable)
                {
                    return false;
                }
                
                return true;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {

            }

            // public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            // {
            //     
            //     
            //     ConfigureTarget(new RenderTargetIdentifier(inlineRenderTextureId));
            //     ConfigureClear(ClearFlag.Color, Color.black);
            // }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get();

                using (new ProfilingScope(cmd, pSampler))
                {

                    var inlineDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                    inlineDescriptor.width = 1024;
                    inlineDescriptor.height = 1024;
                    if (settings.Use2048)
                    {
                        inlineDescriptor.width = 2048;
                        inlineDescriptor.height = 2048;
                    }
                    

                    cmd.GetTemporaryRT(inlineRenderTextureId, inlineDescriptor, settings.RTFilterMode);
                    cmd.GetTemporaryRT(BlurRenderTextureId, inlineDescriptor, settings.RTFilterMode);
                    blurRT = new RenderTargetIdentifier(BlurRenderTextureId);
                    inlineRT = new RenderTargetIdentifier(inlineRenderTextureId);


                    DrawingSettings drawingSettings = CreateDrawingSettings(inlineMaskShaderTagId,
                        ref renderingData, settings.sortingCriteria);
                    FilteringSettings filteringSettings = new FilteringSettings(settings.renderQueueRange);

                    cmd.SetRenderTarget(new RenderTargetIdentifier(inlineRenderTextureId));
                    cmd.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var shadowmapSettings = SpriteShadowmapSettings.Instance;
                    cmd.SetViewProjectionMatrices(shadowmapSettings.ViewMatrix, shadowmapSettings.ProjMatrix);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    
                    cmd.SetViewProjectionMatrices(renderingData.cameraData.GetViewMatrix(), renderingData.cameraData.GetProjectionMatrix());
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    
                    for (int i = 0; i < settings.iteration; i++)
                    {
                        cmd.SetGlobalVector(BlurOffsetShaderId, new Vector4(settings.blurWidth / inlineDescriptor.width, 0, 0, 0));
                        cmd.Blit(inlineRT, blurRT, settings.blurMaterial, 0);

                        cmd.SetGlobalVector(BlurOffsetShaderId, new Vector4(0, settings.blurWidth / inlineDescriptor.height, 0, 0));
                        cmd.Blit(blurRT, inlineRT, settings.blurMaterial, 0);
                    }
                    
                    cmd.SetGlobalColor(GlowColorShaderId, settings.glowColor);
                    cmd.SetGlobalMatrix(ShadowVPShaderId, GetShadowTransform(shadowmapSettings.ProjMatrix, shadowmapSettings.ViewMatrix));
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(inlineRenderTextureId);
                cmd.ReleaseTemporaryRT(BlurRenderTextureId);
            }
            
            static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
            {
                // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
                // apply z reversal to projection matrix. We need to do it manually here.
                if (SystemInfo.usesReversedZBuffer)
                {
                    proj.m20 = -proj.m20;
                    proj.m21 = -proj.m21;
                    proj.m22 = -proj.m22;
                    proj.m23 = -proj.m23;
                }

                Matrix4x4 worldToShadow = proj * view;

                var textureScaleAndBias = Matrix4x4.identity;
                // textureScaleAndBias.m00 = 0.5f;
                // textureScaleAndBias.m11 = 0.5f;
                //textureScaleAndBias.m22 = 0.5f;
                // textureScaleAndBias.m03 = 0.5f;
                // textureScaleAndBias.m23 = 0.5f;
                // textureScaleAndBias.m13 = 0.5f;

                // Apply texture scale and offset to save a MAD in shader.
                return textureScaleAndBias * worldToShadow;
            }
            
        }
    }
}