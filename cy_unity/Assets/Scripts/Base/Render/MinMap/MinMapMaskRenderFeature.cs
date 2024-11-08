using System;
using DH.Base;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TF.Base
{
    public class MinMapMaskRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public RenderPassEvent PassEvent;
            public Material MaskMaterial;
            [Range(1, 4)]
            public int MaskSize = 1;
            
            [Header("Tile")]
            public LayerMask TileLayer;
            public int IterateCount = 4;
            
            [RenderingLayersMaskProperty]
            public uint TileRenderingLayer;
            
            [Header("Background")]
            public LayerMask BgLayer;
            [RenderingLayersMaskProperty]
            public uint BgRenderingLayer = uint.MaxValue;
        }

        public Settings Setting;
        
        private MinMapMaskRenderPass renderPass;
        

        public override void Create()
        {
            renderPass = new MinMapMaskRenderPass(Setting);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderPass.EnablePass())
            {
                renderer.EnqueuePass(renderPass);
            }
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            base.SetupRenderPasses(renderer, in renderingData);

            renderPass.Setup(renderer.cameraColorTarget);
        }

        protected override void Dispose(bool disposing)
        {
            if (renderPass != null)
            {
                renderPass.Dispose();
            }
            
            base.Dispose(disposing);
        }

        private class MinMapMaskRenderPass : ScriptableRenderPass
        {
            private static readonly int MapRangePropertyId = Shader.PropertyToID("_MapRange");
            private static readonly int PlayerPosPropertyId = Shader.PropertyToID("_PlayerPos");
            private static readonly int LightFactorPropertyId = Shader.PropertyToID("_LightFactor");
            private static readonly int FogCutRadiusPropertyId = Shader.PropertyToID("_FogCutRadius");
            
            private readonly ProfilingSampler mProfilingSampler = new ProfilingSampler(nameof(MinMapMaskRenderPass));
            
            private readonly Settings settings;
            
            private RenderTargetHandle maskRTHandle;
            private RenderTargetHandle edgeRTHandle;
            private RenderTargetHandle innerEdgeRTHandle;
            private RenderTargetHandle[] edgeRTHandles;

            private RenderTargetHandle cameraColorCopyRTHandle;
            private RenderTargetIdentifier cameraColorTargetHandle;

            public MinMapMaskRenderPass(Settings settings)
            {
                renderPassEvent = settings.PassEvent;
                
                this.settings = settings;
                
                maskRTHandle.Init("_DH_MapMaskRT");
                edgeRTHandle.Init("_DH_EdgeMaskRT");
                innerEdgeRTHandle.Init("_DH_InnerEdgeMaskRT");
                cameraColorCopyRTHandle.Init("_DH_CameraColorCopyRT");

                edgeRTHandles = new RenderTargetHandle[settings.IterateCount];
                for (int i = 0; i < settings.IterateCount; i++)
                {
                    var item = new RenderTargetHandle();
                    item.Init($"_DH_DualEdgeMaskRT{i}");
                    edgeRTHandles[i] = item;
                }
            }

            public bool EnablePass()
            {
                return MinMapMaskSettings.Enable;
            }
            
            public void Setup(RenderTargetIdentifier cameraColorTargetHandle)
            {
                this.cameraColorTargetHandle = cameraColorTargetHandle;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                base.Configure(cmd, cameraTextureDescriptor);

                var maskDesc = cameraTextureDescriptor;

                int factor = maskDesc.width / 360;
                factor = Mathf.Max(2, factor);
                if (factor < settings.MaskSize)
                {
                    factor = settings.MaskSize;
                }
                maskDesc.width /= factor;
                maskDesc.height /= factor;

                maskDesc.depthBufferBits = 0;
                /*maskDesc.colorFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
                    ? RenderTextureFormat.R8
                    : maskDesc.colorFormat;*/

                maskDesc.sRGB = true;
                maskDesc.useMipMap = false;
                maskDesc.autoGenerateMips = false;
                
                cmd.GetTemporaryRT(maskRTHandle.id, maskDesc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(edgeRTHandle.id, maskDesc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(cameraColorCopyRTHandle.id, cameraTextureDescriptor, FilterMode.Bilinear);
                cmd.GetTemporaryRT(innerEdgeRTHandle.id, maskDesc, FilterMode.Bilinear);

                for (int i = 0; i < edgeRTHandles.Length; i++)
                {
                    maskDesc.width = Mathf.Max(maskDesc.width / 2, 1);
                    maskDesc.height = Mathf.Max(maskDesc.height / 2, 1);
                    var item = edgeRTHandles[i];
                    cmd.GetTemporaryRT(item.id, maskDesc, FilterMode.Bilinear);
                }
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(maskRTHandle.id);
                cmd.ReleaseTemporaryRT(edgeRTHandle.id);
                cmd.ReleaseTemporaryRT(cameraColorCopyRTHandle.id);
                cmd.ReleaseTemporaryRT(innerEdgeRTHandle.id);

                for (int i = 0; i < edgeRTHandles.Length; i++)
                {
                    cmd.ReleaseTemporaryRT(edgeRTHandles[i].id);
                }
                base.OnCameraCleanup(cmd);
            }

            public void Dispose()
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, mProfilingSampler))
                {
                    //渲染当前地图的Mask
                    cmd.SetRenderTarget(maskRTHandle.Identifier());
                    cmd.ClearRenderTarget(true, true, Color.clear);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("MinMapMaskCast"),
                        new SortingSettings(renderingData.cameraData.camera){criteria = SortingCriteria.SortingLayer});
                    FilteringSettings filteringSettings =
                        new FilteringSettings(RenderQueueRange.all, settings.TileLayer, settings.TileRenderingLayer);
                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    //down sample
                    for (int i = 0; i < edgeRTHandles.Length; i++)
                    {
                        if (i == 0)
                        {
                            Blit(cmd, maskRTHandle.Identifier(), edgeRTHandles[i].Identifier(), settings.MaskMaterial, 1);
                            Blit(cmd, edgeRTHandles[i].Identifier(), innerEdgeRTHandle.Identifier(), settings.MaskMaterial, 1);
                            Blit(cmd, maskRTHandle.Identifier(), edgeRTHandles[i].Identifier(), settings.MaskMaterial, 1);
                            Blit(cmd, edgeRTHandles[i].Identifier(), innerEdgeRTHandle.Identifier(), settings.MaskMaterial, 4);
                        }
                        else
                        {
                            Blit(cmd, edgeRTHandles[i - 1].Identifier(), edgeRTHandles[i].Identifier(), settings.MaskMaterial, 1);
                        }
                    }
                    
                    //up sample
                    for (int i = edgeRTHandles.Length - 1; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            Blit(cmd, edgeRTHandles[i].Identifier(), edgeRTHandle.Identifier(), settings.MaskMaterial, 2);
                        }
                        else
                        {
                            Blit(cmd, edgeRTHandles[i].Identifier(), edgeRTHandles[i - 1].Identifier(), settings.MaskMaterial, 2);
                        }
                    }
                    
                    cmd.SetGlobalVector(MapRangePropertyId, MinMapMaskSettings.MapRange);
                    cmd.SetGlobalVector(PlayerPosPropertyId, MinMapMaskSettings.PlayerPosition);
                    cmd.SetGlobalVector(LightFactorPropertyId, MinMapMaskSettings.PlayerLightRadius);
                    cmd.SetGlobalFloat(FogCutRadiusPropertyId, MinMapMaskSettings.FogCutRadius);
                    Blit(cmd, cameraColorTargetHandle, cameraColorCopyRTHandle.Identifier(), settings.MaskMaterial, 3);
                    
                    Blit(cmd, cameraColorCopyRTHandle.Identifier(), cameraColorTargetHandle);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

        }
        
    }
}