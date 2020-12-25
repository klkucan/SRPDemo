Shader "SRP1/Standard"{
	Properties{
		_Color1("Color", Color) = (1,1,1,1)
		_MainTex("Main Tex", 2D) = "white" {}
	}
		SubShader{
			//Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline"}
			//Tags { "RenderType" = "Opaque" }

			// LightMode 的设置导致了URP无法使用这个shader
			Tags { "RenderType" = "Opaque" "LightMode" = "SRP1Shader"}
			Pass{
				
			
				HLSLPROGRAM
				#pragma vertex RP1Vertex
				#pragma fragment RP1Fragment
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

				CBUFFER_START(UnityPerMaterial)
				float4 _Color1;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				CBUFFER_END

				CBUFFER_START(UnityPerFrame)
				float4x4 unity_MatrixVP;
				CBUFFER_END
	
				CBUFFER_START(UnityPerDraw)					
				float4x4 unity_ObjectToWorld;
				float4x4 unity_WorldToObject;
				float4 unity_LODFade;
				float4 unity_WorldTransformParams;
				CBUFFER_END
				
				struct Attributes {
					float3 position : POSITION;
					float4 texcoord : TEXCOORD0;
				};
				struct Varyings {
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
				Varyings RP1Vertex(Attributes input) {
					Varyings output;
					float4 world_pos = mul(unity_ObjectToWorld,float4(input.position,1.0));
					output.position = mul(unity_MatrixVP,world_pos);
					output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
					return output;
				}
				float4 RP1Fragment(Varyings input) :SV_TARGET {
					float4 texColor = tex2D(_MainTex, input.uv);
					return _Color1 * texColor;
					//return float4(1.0,1.0,1.0,1.0);
				}
				ENDHLSL
			}
	}
		//FallBack "Diffuse"
		//FallBack "Hidden/Universal Render Pipeline/FallbackError"
		
}