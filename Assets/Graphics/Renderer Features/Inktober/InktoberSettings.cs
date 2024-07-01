using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    [System.Serializable]
    public class InktoberSettings
    {
        [field:SerializeField] public RenderPassEvent RenderPassEvent { get; set; } = RenderPassEvent.AfterRenderingTransparents;


        [field: SerializeField, Header("Canny Edge Detection Settings"), Range(0.0f, 1.0f)]
        public float LowThreshold { get; set; } = 0.1f;
        [field: SerializeField, Range(0.0f, 1.0f)]
        public float HighThreshold { get; set; } = 0.8f;

        [field: Space, SerializeField, Range(1.0f, 10.0f)] public float SampleRange { get; set; } = 1.0f;
    }
}