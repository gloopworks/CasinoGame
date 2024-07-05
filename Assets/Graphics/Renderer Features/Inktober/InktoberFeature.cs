using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    public class InktoberFeature : ScriptableRendererFeature
    {
        [SerializeField] private bool runInSceneView;
        [SerializeField] private InktoberSettings settings;
        
        private Material screenMaterial;
        private Material overrideMaterial;

        private InktoberPass pass;

        public override void Create()
        {
            screenMaterial = CoreUtils.CreateEngineMaterial("Screen/Inktober");
            overrideMaterial = CoreUtils.CreateEngineMaterial("Unlit/VertexColorOnly");

            pass = new InktoberPass(screenMaterial, overrideMaterial, settings)
            {
                renderPassEvent = settings.RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {

#if UNITY_EDITOR
            CameraType cameraType = renderingData.cameraData.cameraType;
            if (cameraType == CameraType.Preview)
            {
                return;
            }
            if (!runInSceneView && cameraType == CameraType.SceneView)
            {
                return;
            }
#endif
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            pass.Dispose();
        }
    }
}