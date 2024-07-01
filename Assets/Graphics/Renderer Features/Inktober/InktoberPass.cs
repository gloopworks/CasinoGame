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

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtLuminance, descriptor, name: "_LuminanceTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtGradientIntensity, descriptor, name: "_GradientIntensityTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtMagnitudeSuppression, descriptor, name: "_MagnitudeSuppressionTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtDoubleThreshold, descriptor, name: "_DoubleThresholdTexture");
            _ = RenderingUtils.ReAllocateIfNeeded(ref rtHysteresis, descriptor, name: "_HysterisisTexture");

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

            material.SetFloat("_LowThreshold", settings.LowThreshold);
            material.SetFloat("_HighThreshold", settings.HighThreshold);

            material.SetFloat("_SampleRange", settings.SampleRange);

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
                Blitter.BlitCameraTexture(cmd, rtHysteresis, camTarget);
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
        }
    }
}