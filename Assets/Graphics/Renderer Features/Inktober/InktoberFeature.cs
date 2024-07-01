using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    public class InktoberFeature : ScriptableRendererFeature
    {
        [SerializeField] private bool runInSceneView;
        [SerializeField] private InktoberSettings settings;
        private Material material;
        private InktoberPass pass;

        public override void Create()
        {
            material = CoreUtils.CreateEngineMaterial("Screen/Inktober");
            pass = new InktoberPass(material, settings)
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