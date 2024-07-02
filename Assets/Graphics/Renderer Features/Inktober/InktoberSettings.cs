using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    [System.Serializable]
    public class InktoberSettings
    {
        [field:SerializeField] public RenderPassEvent RenderPassEvent { get; set; } = RenderPassEvent.AfterRenderingTransparents;


        [field: SerializeField, Header("Canny Edge Detection Settings"), Range(0.0f, 1.0f), Tooltip("For Double Thresholding step. Low threshold edges will only be drawn when next to high threshold edges.")]
        public float LowThreshold { get; set; } = 0.1f;
        

        [field: SerializeField, Range(0.0f, 1.0f), Tooltip("For Double Thresholding. High threshold edges will always be drawn.")]
        public float HighThreshold { get; set; } = 0.8f;


        [field: Space, SerializeField, Range(1.0f, 10.0f), Tooltip("Convolution matrix will sample pixels at this specified distance. This affects the width of the lines.")]
        public float SampleRange { get; set; } = 1.0f;


        [field: Space, SerializeField, Range(0.0f, 1.0f), Tooltip("Required luminance of a pixel for edges to still be drawn (using inverted colours) on dark pixels.")]
        public float InvertedEdgeLuminanceThreshold { get; set; } = 0.1f;


        [field: SerializeField, Header("Stippling")]
        public Texture2D StippleTexture { get; set; }


        [field: SerializeField, Range(0.01f, 1.0f)]
        public float StippleSize { get; set; } = 1.0f;


        [field: SerializeField, Range(0.01f, 5.0f)]
        public float LuminanceContrast { get; set; } = 1.0f;


        [field: SerializeField, Range(0.0f, 10.0f)]
        public float LuminanceCorrection { get; set; } = 1.0f;


        [field: SerializeField, Header("Paper Overlay")]
        public Texture2D PaperOverlayTexture { get; set; }


        [field: SerializeField]
        public Color PaperTint { get; set; }


        [field: SerializeField, Space]
        public Color InkColor { get; set; }
    }
}