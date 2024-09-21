using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.VertexColors
{
    public class VertexColorsFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material screenMaterial;

        [Space]

        [SerializeField] private bool runInSceneView;
        [SerializeField] private RenderPassEvent renderPassEvent;

        private Material overrideMaterial;
        private VertexColorsPass mainPass;

        public override void Create()
        {
            overrideMaterial = CoreUtils.CreateEngineMaterial("Unlit/VertexColorOnly");

            mainPass = new VertexColorsPass(overrideMaterial, screenMaterial)
            {
                renderPassEvent = renderPassEvent
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
            renderer.EnqueuePass(mainPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(overrideMaterial);
            mainPass.Dispose();

            base.Dispose(disposing);
        }
    }
}