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

        private RTHandle rtTempColor;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            _ = RenderingUtils.ReAllocateIfNeeded(ref rtTempColor, descriptor, name: "_TemporaryColorTexture");

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

            using (new ProfilingScope(cmd, new ProfilingSampler("Luminance Pass")))
            {
                Blitter.BlitCameraTexture(cmd, camTarget, rtTempColor, material, 0);
                Blitter.BlitCameraTexture(cmd, rtTempColor, camTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            rtTempColor?.Release();
        }
    }
}