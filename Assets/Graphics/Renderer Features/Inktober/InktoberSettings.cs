using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MixJam13.Graphics.RendererFeatures.Inktober
{
    [System.Serializable]
    public class InktoberSettings
    {
        [field:SerializeField] public RenderPassEvent RenderPassEvent { get; set; } = RenderPassEvent.AfterRenderingTransparents;
    }
}