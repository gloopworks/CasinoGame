Shader "Unlit/VertexColorOnly"
{
	SubShader
	{
		Tags { "RenderPipeline" = "UniversalRenderPipeline" }
		LOD 100

		Pass
		{
			Name "UniversalForward"
			Tags { "LightMode"  = "UniversalForward" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct Attributes
            {
	            float4 color : COLOR;
                float3 positionOS : POSITION;
			};

			struct Interpolators
            {
                float4 positionCS : SV_POSITION;
				float4 color : COLOR;
			};

			Interpolators Vertex(Attributes input)
			{
				Interpolators output;
                
                float3 posOS = input.positionOS.xyz;

				output.positionCS = TransformObjectToHClip(posOS);
				output.color = input.color;

				return output;
			}

			float4 Fragment(Interpolators input) : SV_TARGET
            {
				return float4(input.color.rgb, 1.0f);
			}

			ENDHLSL
		}
	}
}
