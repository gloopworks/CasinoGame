using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    public class InktoberPass : ScriptableRenderPass
    {
        public InktoberPass(Material material, InktoberSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }

        private Material material;
        private InktoberSettings settings;

        private RTHandle rtLuminance;
        private RTHandle rtGradientIntensity;
        private RTHandle rtMagnitudeSuppression;
        private RTHandle rtDoubleThreshold;
        private RTHandle rtHysteresis;
        private RTHandle rtStipple;
        private RTHandle rtCombination;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtLuminance, descriptor, name: "_LuminanceTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtGradientIntensity, descriptor, name: "_GradientIntensityTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtMagnitudeSuppression, descriptor, name: "_MagnitudeSuppressionTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtDoubleThreshold, descriptor, name: "_DoubleThresholdTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtHysteresis, descriptor, name: "_HysterisisTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtStipple, descriptor, name: "_StippleTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtCombination, descriptor, name: "_CombinationTexture");

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

            material.SetFloat("_SampleRange", settings.SampleRange);

            material.SetFloat("_LowThreshold", settings.LowThreshold);
            material.SetFloat("_HighThreshold", settings.HighThreshold);

            material.SetTexture("_StippleTex", settings.StippleTexture);
            material.SetFloat("_StippleSize", settings.StippleSize);

            material.SetFloat("_LuminanceContrast", settings.LuminanceContrast);
            material.SetFloat("_LuminanceCorrection", settings.LuminanceCorrection);

            using (new ProfilingScope(cmd, new ProfilingSampler("Luminance Pass")))
            {
                Blitter.BlitCameraTexture(cmd, camTarget, rtLuminance, material, 0);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Gradient Intensity Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtLuminance, rtGradientIntensity, material, 2);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Magnitude Suppression Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtGradientIntensity, rtMagnitudeSuppression, material, 3);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Double Threshold Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtMagnitudeSuppression, rtDoubleThreshold, material, 4);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Hysteresis Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtDoubleThreshold, rtHysteresis, material, 5);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Stippling Pass")))
            {
                Blitter.BlitCameraTexture(cmd, rtLuminance, rtStipple, material, 6);
            }

            using (new ProfilingScope(cmd, new ProfilingSampler("Combination Pass")))
            {
                material.SetTexture("_EdgeTex", rtHysteresis);
                Blitter.BlitCameraTexture(cmd, rtStipple, rtCombination, material, 7);
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
        }
    }
}