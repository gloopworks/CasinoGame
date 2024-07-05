using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using System.Collections.Generic;

namespace MixJam13.Graphics.RendererFeatures.VertexColors
{
    public class VertexColorsPass : ScriptableRenderPass
    {
        public VertexColorsPass(Material overrideMaterial)
        {
            this.overrideMaterial = overrideMaterial;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            shaderTags = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward")
            };
        }

        private List<ShaderTagId> shaderTags;

        private FilteringSettings filteringSettings;
        private Material overrideMaterial;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

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
    }
}