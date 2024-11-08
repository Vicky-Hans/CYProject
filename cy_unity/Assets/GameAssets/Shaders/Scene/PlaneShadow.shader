Shader "DH/Scene/PlaneShadow"
{
	Properties
	{
		_shdowCol("Shadow Color", Color) = (0,0,0,0)
		_AmbientStr("Ambient Strength",Range(0,1)) = 0.2

	}

	SubShader
	{

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

		Cull Back

	
		
		Pass
		{
			//Blend DstAlpha SrcAlpha
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }
			
			ZWrite On
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma multi_compile_instancing



			#pragma shader_feature _ _SAMPLE_GI

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"			


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD2;
				#endif
				float3 normalWS : TEXCOORD1;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 3);

				float3 color : TEXCOORD4;

				UNITY_VERTEX_INPUT_INSTANCE_ID

			};

			CBUFFER_START(UnityPerMaterial)
			float4 _shdowCol;
			float _AmbientStr;
			CBUFFER_END


			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 shadowPos;
				shadowPos.y = min(positionWS.y , 0.01);
				shadowPos.xz = positionWS.xz - _MainLightPosition.xz *  max( 0 , positionWS.y ) / _MainLightPosition.y;

				v.normal = normalize(v.normal);
				o.normalWS = mul(GetObjectToWorldMatrix(),float4( v.normal.xyz , 0.0 )).xyz;

				o.color = saturate(length(shadowPos) / 3);
				
				float4 positionCS = TransformWorldToHClip( shadowPos );

				o.worldPos = positionWS;


				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				OUTPUT_LIGHTMAP_UV(v.uv, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);

				o.clipPos = positionCS;

				return o;			
				
				}

			half4 frag ( VertexOutput IN  ) : COLOR
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 WorldPosition = IN.worldPos;
				
				float3 ambientcol = saturate(SAMPLE_GI(IN.lightmapUV, IN.vertexSH, IN.normalWS)) * _AmbientStr;
				
				float3 Color = _shdowCol.rgb + ambientcol;
				float Alpha = 1;
				
				//float3 Color = saturate(SAMPLE_GI(IN.lightmapUV, IN.vertexSH, IN.normalWS)) * 0.2 * (1 - IN.color.rgb);
				
				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

	
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	
}
