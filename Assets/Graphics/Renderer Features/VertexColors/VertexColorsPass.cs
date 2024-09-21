using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using System.Collections.Generic;

namespace MixJam13.Graphics.RendererFeatures.VertexColors
{
    public class VertexColorsPass : ScriptableRenderPass
    {
        public VertexColorsPass(Material overrideMaterial, Material screenMaterial)
        {
            this.overrideMaterial = overrideMaterial;
            this.screenMaterial = screenMaterial;

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            shaderTags = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward")
            };
        }

        private List<ShaderTagId> shaderTags;

        private FilteringSettings filteringSettings;
        private Material overrideMaterial;

        private Material screenMaterial;// Screen Material to write property _VertexColorTex to, not to actually Blit
        // Other passes (e.g. Inktober pass) should use this property for their own shaders

        private RTHandle rtVertexColor;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtVertexColor, descriptor);
            RTHandle depthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            ConfigureTarget(rtVertexColor, depthTarget);
            ConfigureClear(ClearFlag.Color, Color.white);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            screenMaterial.SetTexture("_VertexColorTex", rtVertexColor);

            using (new ProfilingScope(cmd, new ProfilingSampler("Draw Vertex Colors")))
            {
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;

                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortingCriteria);
                drawingSettings.overrideMaterial = overrideMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            rtVertexColor?.Release();
        }
    }
}