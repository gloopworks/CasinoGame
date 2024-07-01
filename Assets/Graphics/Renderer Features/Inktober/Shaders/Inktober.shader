Shader "Screen/Inktober"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "Luminance"

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag

			SamplerState sampler_point_clamp;
			
			// Copied from UNITYCG.cginc
			half LinearRgbToLuminance(half3 linearRgb)
			{
				return dot(linearRgb, half3(0.2126729f,  0.7151522f, 0.0721750f));
			}

			half4 Frag(Varyings input) : SV_Target
			{
				return LinearRgbToLuminance(_BlitTexture.Sample(sampler_point_clamp, input.texcoord).rgb);
			}

			ENDHLSL
		}

		Pass
		{
			// Edge Detection via Sobel-Feldman Operator
			Name "Sobel"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag

			float4 _BlitTexture_TexelSize;
			SamplerState sampler_point_clamp;

            static float2 samplePoints[9] =
            {
				float2(-1, 1), float2(0, 1), float2(1, 1),
				float2(-1, 0), float2(0, 0), float2(1, 0),
				float2(-1, -1), float2(0, -1), float2(1, -1)
			};

			static int Kx[9] =
            {
				1, 0, -1,
				2, 0, -2,
				1, 0, -1
			};

			static int Ky[9] =
            {
				1, 2, 1,
				0, 0, 0,
				-1, -2, -1
			};

			half4 Frag(Varyings input) : SV_Target
			{
				float Gx = 0.0f;
				float Gy = 0.0f;

				for (int i = 0; i < 9; i++)
				{
					float2 uv = input.texcoord + _BlitTexture_TexelSize.zw * samplePoints[i];

					half l = _BlitTexture.Sample(sampler_point_clamp, uv).a;

					Gx += Kx[i] * l;
					Gy += Ky[i] * l;
				}

				float mag = sqrt(Gx * Gx + Gy * Gy);
				return mag;
			}

			ENDHLSL
		}

		// Canny Intensity Pass (Sobel-Feldman)
		Pass
		{
			Name "Intensity"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag
						
			float4 _BlitTexture_TexelSize;
			SamplerState sampler_point_clamp;

            static float2 samplePoints[9] =
            {
				float2(-1, 1), float2(0, 1), float2(1, 1),
				float2(-1, 0), float2(0, 0), float2(1, 0),
				float2(-1, -1), float2(0, -1), float2(1, -1)
			};

			static int Kx[9] =
            {
				1, 0, -1,
				2, 0, -2,
				1, 0, -1
			};

			static int Ky[9] =
            {
				1, 2, 1,
				0, 0, 0,
				-1, -2, -1
			};

			float4 Frag(Varyings input) : SV_Target
			{
				float Gx = 0.0f;
				float Gy = 0.0f;

				for (int i = 0; i < 9; i++)
				{
					float2 uv = input.texcoord + _BlitTexture_TexelSize.zw * samplePoints[i];

					half l = _BlitTexture.Sample(sampler_point_clamp, uv).a;

					Gx += Kx[i] * l;
					Gy += Ky[i] * l;
				}

				float mag = sqrt(Gx * Gx + Gy * Gy);
				float theta = abs(atan2(Gy, Gx));

				return float4(Gx, Gy, theta, mag);
			}

			ENDHLSL
		}

		// Canny Magnitude Suppression Pass
		Pass
		{
			Name "Suppression"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag

			float4 _BlitTexture_TexelSize;
			SamplerState sampler_point_clamp;

			float CardinalMagnitude(float2 uv, float2 direction)
			{
				return _BlitTexture.Sample(sampler_point_clamp, uv + _BlitTexture_TexelSize.zw * direction).a;
			}

			float4 Frag(Varyings input) : SV_Target
			{
				float2 uv = input.texcoord;
				float4 canny = _BlitTexture.Sample(sampler_point_clamp, uv);

				float mag = canny.a;
				float theta = degrees(canny.b);

				if ((0.0f <= theta && theta <= 45.0f) || (135.0f <= theta && theta <= 180.0f))
				{
                    float northMag = CardinalMagnitude(uv, float2(0, -1));
                    float southMag = CardinalMagnitude(uv, float2(0, 1));

                    canny = mag >= northMag && mag >= southMag ? canny : 0.0f;
                }
				else if (45.0f <= theta && theta <= 135.0f)
				{
                    float westMag = CardinalMagnitude(uv, float2(-1, 0));
                    float eastMag = CardinalMagnitude(uv, float2(1, 0));

                    canny = mag >= westMag && mag >= eastMag ? canny : 0.0f;
                }

				return canny;
			}

			ENDHLSL
		}

		// Canny Double Threshold Pass
		Pass
		{
			Name "Thresholds"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag

			float _HighThreshold, _LowThreshold;
			SamplerState sampler_point_clamp;

			float4 Frag(Varyings input) : SV_Target
			{
				float mag = _BlitTexture.Sample(sampler_point_clamp, input.texcoord).a;

				float4 result = 0.0f;

				if (mag > _HighThreshold)
				{
					result = 1.0f;
				}
				else if (mag > _LowThreshold)
				{
					result = 0.5f;
				}

				return result;
			}

			ENDHLSL
		}

		// Canny Hysteresis Pass
		Pass
		{
			Name "Hysteresis"

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex Vert
			#pragma fragment Frag

			float4 _BlitTexture_TexelSize;
			SamplerState sampler_point_clamp;

			float preserve(float2 uv)
			{
				int x, y;

                [unroll]
	            for (x = -1; x <= 1; ++x)
				{
		            [unroll]
			        for (y = -1; y <= 1; ++y)
					{
				        if (x == 0 && y == 0)
						{
							continue;
						}

                        float2 nuv = uv + _BlitTexture_TexelSize.zw * float2(x, y);
	                    half neighborStrength = _BlitTexture.Sample(sampler_point_clamp, nuv).a;

		                if (neighborStrength == 1.0f)
						{
							return 1.0f;
						}
                    }
				}

				return 0.0f;
			}

            float4 Frag(Varyings input) : SV_Target
			{
		        float strength = _BlitTexture.Sample(sampler_point_clamp, input.texcoord).a;

			    float4 result = strength;

				if (strength == 0.5f)
				{
					result = preserve(input.texcoord);
				}

				return result;
			}

			ENDHLSL
		}
	}
}