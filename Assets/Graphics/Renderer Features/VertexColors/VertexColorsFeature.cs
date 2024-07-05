using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.VertexColors
{
    public class VertexColorsFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent renderPassEvent;
        [SerializeField] private bool runInSceneView;

        private Material material;
        private VertexColorsPass mainPass;

        public override void Create()
        {
            material = CoreUtils.CreateEngineMaterial("Unlit/VertexColorOnly");

            mainPass = new VertexColorsPass(material)
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
    }
}