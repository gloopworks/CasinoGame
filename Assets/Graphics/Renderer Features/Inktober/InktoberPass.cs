using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    public class InktoberPass : ScriptableRenderPass
    {
        public InktoberPass(Material screenMaterial, Material overrideMaterial, InktoberSettings settings)
        {
            this.screenMaterial = screenMaterial;
            this.overrideMaterial = overrideMaterial;

            this.settings = settings;

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            shaderTags = new List<ShaderTagId>
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };
        }

        private List<ShaderTagId> shaderTags;
        private FilteringSettings filteringSettings;

        private Material screenMaterial;
        private Material overrideMaterial;

        private InktoberSettings settings;

        private RTHandle rtLuminance;

        private RTHandle rtGradientIntensity;
        private RTHandle rtMagnitudeSuppression;
        private RTHandle rtDoubleThreshold;
        private RTHandle rtHysteresis;

        private RTHandle rtStipple;

        private RTHandle rtCombination;
        private RTHandle rtOverlay;

        private RTHandle rtVertexColor;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtLuminance, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtGradientIntensity, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtMagnitudeSuppression, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtDoubleThreshold, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtHysteresis, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtStipple, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtCombination, descriptor);
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtOverlay, descriptor);

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtVertexColor, descriptor);

            RTHandle camTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RTHandle depthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            ConfigureTarget(camTarget, depthTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            RTHandle camTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            screenMaterial.SetFloat("_SampleRange", settings.SampleRange);

            screenMaterial.SetFloat("_LowThreshold", settings.LowThreshold);
            screenMaterial.SetFloat("_HighThreshold", settings.HighThreshold);

            screenMaterial.SetTexture("_NoiseTex", settings.StippleTexture);
            screenMaterial.SetFloat("_StippleSize", settings.StippleSize);

            screenMaterial.SetFloat("_LuminanceContrast", settings.LuminanceContrast);
            screenMaterial.SetFloat("_LuminanceCorrection", settings.LuminanceCorrection);

            screenMaterial.SetFloat("_InvertedEdgeLuminanceThreshold", settings.InvertedEdgeLuminanceThreshold);
            screenMaterial.SetFloat("_InvertedEdgeBrightness", settings.InvertedEdgeBrightness);

            screenMaterial.SetTexture("_OverlayTex", settings.PaperOverlayTexture);
            screenMaterial.SetColor("_OverlayTint", settings.PaperTint);
            screenMaterial.SetColor("_InkColor", settings.InkColor);

            using (new ProfilingScope(cmd, new ProfilingSampler("Luminance Pass")))
            {
                Blitter.BlitCameraTexture(cmd, camTarget, rtLuminance, screenMaterial, 0);
                screenMaterial.SetTexture("_LuminanceTex", rtLuminance);
            }
            using (new ProfilingScope(cmd, new ProfilingSampler("Gradient Intensity Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtLuminance, rtGradientIntensity, screenMaterial, 2);
            }
            using (new ProfilingScope(cmd, new ProfilingSampler("Magnitude Suppression Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtGradientIntensity, rtMagnitudeSuppression, screenMaterial, 3);
            }
            using (new ProfilingScope(cmd, new ProfilingSampler("Double Threshold Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtMagnitudeSuppression, rtDoubleThreshold, screenMaterial, 4);
            }
            using (new ProfilingScope(cmd, new ProfilingSampler("Hysteresis Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtDoubleThreshold, rtHysteresis, screenMaterial, 5);
                screenMaterial.SetTexture("_EdgeTex", rtHysteresis);
            }
            using (new ProfilingScope(cmd, new ProfilingSampler("Stippling Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtLuminance, rtStipple, screenMaterial, 6);
                screenMaterial.SetTexture("_StippleTex", rtStipple);
            }

            Blitter.BlitCameraTexture(cmd, camTarget, rtVertexColor);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            using (new ProfilingScope(cmd, new ProfilingSampler("Vertex Color Pass")))
            {
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;

                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortingCriteria);
                drawingSettings.overrideMaterial = overrideMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Combination Pass")))
            {
                screenMaterial.SetTexture("_VertexColorTex", rtVertexColor);
                Blitter.BlitCameraTexture(cmd, rtStipple, rtCombination, screenMaterial, 7);
                Blitter.BlitCameraTexture(cmd, rtCombination, camTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            rtLuminance?.Release();

            rtGradientIntensity?.Release();
            rtMagnitudeSuppression?.Release();
            rtDoubleThreshold?.Release();
            rtHysteresis?.Release();

            rtStipple?.Release();

            rtCombination?.Release();
            rtOverlay?.Release();

            rtVertexColor?.Release();
        }
    }
}