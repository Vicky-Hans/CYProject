Shader "DH/Effects/ScreenGradient"
{
	Properties
	{
		_Color("Color ", Color) = (1,0,0,1)
		_Range("Range", Range( 0 , 1)) = 0
		_Fade("Fade", Range( 0 , 2)) = 1
		_Noise("Noise", 2D) = "white" {}
	}

	SubShader
	{

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Back
		AlphaToMask Off

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM


			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			half4 _Noise_ST;
			half _Range;
			half _Fade;
			CBUFFER_END

			sampler2D _Noise;


			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.clipPos = positionCS;

				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif


				float2 uv_Noise = IN.ase_texcoord3.xy * _Noise_ST.xy + _Noise_ST.zw;
				half temp_output_52_0 = ( _Range * tex2D( _Noise, uv_Noise ).r );
				half2 temp_cast_0 = (temp_output_52_0).xx;
				half2 temp_cast_1 = (( temp_output_52_0 + _Fade )).xx;
				float4 screenPos = IN.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				half4 temp_cast_2 = (1.0).xxxx;
				half2 smoothstepResult49 = smoothstep( temp_cast_0 , temp_cast_1 , (abs( ( ( ase_screenPosNorm * 2.0 ) - temp_cast_2 ) )).xy);
				half dotResult50 = dot( smoothstepResult49 , smoothstepResult49 );
				half4 temp_output_21_0 = ( _Color * dotResult50 );
				
				float3 Color = (temp_output_21_0).rgb;
				float Alpha = (temp_output_21_0).a;


				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif


				return half4( Color, Alpha );
			}
			ENDHLSL
		}
		
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	
}
