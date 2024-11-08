Shader "DH/Effects/BlendBase"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("[Cull Mode]", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("[Src]", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("[Dst]", Float) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("[ZTest]",Range(0,7)) = 4 
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle] _UseAlphaChannel("UseAlphaChannel", float) = 1
		[Toggle] _UseParticleData("UseParticleData", float) = 0
		_YHeight("YHeight", Float) = 0

	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull [_CullMode]
		AlphaToMask Off
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend [_Src] [_Dst]
			ZWrite Off
			ZTest [_Ztest]
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float4 uv : TEXCOORD0;
				float4 vertexCol : TEXCOORD2;
				#ifdef FOG
				float fogFactor : TEXCOORD3;
				#endif
				
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			int _UseAlphaChannel;
			int _UseParticleData;
			float4 _Color;
			float _YHeight;
			CBUFFER_END
			sampler2D _MainTex;

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;

				o.uv.xy = v.uv.xy;
				float2 particleDataOn = v.uv.zw;
				float2 particleDataOff = 0;
				o.uv.zw = lerp(particleDataOff,particleDataOn,_UseParticleData);
				o.vertexCol = v.color;
				
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz);
				float4 positionCS = TransformWorldToHClip( positionWS);

				o.worldPos.xyz = positionWS;
				
				#ifdef FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{

				float3 WorldPosition = IN.worldPos;
				
				float2 uv = IN.uv.xy;
				float Time = fmod(_Time.y,1000);

				float2 uv_MainTex = uv * _MainTex_ST.xy + (frac(_MainTex_ST.zw * Time + 0.0001) * 2 - 1) + IN.uv.zw + float2(1,1);
				float4 mainTex = tex2D( _MainTex,uv_MainTex);
				float mainTexGray = lerp(mainTex.r,mainTex.a,_UseAlphaChannel);
				
				float Alpha = mainTexGray * IN.vertexCol.a * _Color.a;
				float3 Color = (mainTex * _Color * IN.vertexCol).rgb;

				Alpha *= saturate(WorldPosition.y * 2 - _YHeight);

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif
				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}
}