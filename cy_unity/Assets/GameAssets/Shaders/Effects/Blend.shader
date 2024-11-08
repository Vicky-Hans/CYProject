Shader "DH/Effects/Blend"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("[Cull Mode]", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("[Src]", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("[Dst]", Float) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("[ZTest]",Range(0,7)) = 4 
		[IntRange]_UseParticleData("UseParticleData", Range(0,1)) = 0
		[HDR]_Color("Color", Color) = (1,1,1,1)
		[Toggle(_Desaturate)]_Desaturate("Desaturate",Float) = 0
		_MainTex("MainTex", 2D) = "white" {}
		[IntRange] _UseAlphaChannel("UseAlphaChannel", Range(0,1)) = 1


		[KeywordEnum(None,Add,Multiply)] _BlendTex("BlendTex", Float) = 0
		_Tex2Col("Tex2Color",color) = (1,1,1,1)
		_MainTex2("MainTex2", 2D) = "black" {}
		[IntRange] _UseAlphaChannel2("UseAlphaChannel", Range(0,1)) = 1
		_MainTexAnimation("MainTexAnimation",vector) = (0,0,0,0)


		[Space(15)]
		[Toggle(_Mask_ON)] _Mask_ON("Mask", Float) = 0
		_Mask("MaskTex", 2D) = "white" {}
		_MaskDistortionStrength("MaskDistortionStrength", Range( -1 , 1)) = 0
		[IntRange]_MaskAlphaChannel("MaskAlphaChannel", Range(0,1)) = 1
		[IntRange]_InvertMask("InvertMask", Range(0,1)) = 0

		
		[Space(15)]
		[Toggle(_Distortion_ON)] _Distortion_ON("Distortion", Float) = 0
		[Toggle(_loopAnima)] _loopAnima("FlowMapLoopAnimation", Float) = 0
		_loopSpeed("LoopSpeed", Range(0,1)) = 0
		_Noise("Noise", 2D) = "White" {}
		_DistortionStrength("DistortionStrength", Range( -1 , 1)) = 0
		[IntRange] _InvertNoise("InvertNoise", Range(0,1)) = 0

		_vertexAnima("VertexAnimation", Vector) = (0,0,0,0)
		
		// [Space(15)]
		//[MaterialToggle(_Dissolve_ON)]_Dissolve_ON("Light", Float) = 0
		[Toggle(_Dissolve_ON)] _Dissolve_ON("Dissolve", Float) = 0
		[Toggle(_SoftEdge_ON)] _SoftEdge("SoftEdge", Float) = 1
		[IntRange] _OnlyUseMask("OnlyUseMask", Range(0,1)) = 0
		_softedgestr("SoftEdgeStrength",Range(0,1)) = 1
		_EdgeWidth("EdgeWidth", Range( 0 , 1)) = 0
		[HDR]_EdgeColor("EdgeColor", Color) = (1,1,1,0)
		


		[Space(15)]
		//[Toggle(_YFade_ON)] _YFade_ON("YFade", Float) = 0
		_YHeight("YHeight", float) = 0
		

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

			// #pragma prefer_hlslcc gles
			// #pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"


			#pragma shader_feature_local _SoftEdge_ON
			#pragma shader_feature_local _Desaturate
			//#pragma shader_feature_local _YFade_ON
			#pragma shader_feature_local _BLENDTEX_NONE _BLENDTEX_ADD _BLENDTEX_MULTIPLY
			#pragma shader_feature_local _Mask_ON
			#pragma shader_feature_local _Distortion_ON
			#pragma shader_feature_local _loopAnima
			#pragma shader_feature_local _Dissolve_ON
			//#pragma shader_feature_local _UseParticleData


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
				float4 custom : TEXCOORD1;
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
				float4 particleData : TEXCOORD4;
				float3 normal : TEXCOORD5;
				
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainTex2_ST;
			int _UseAlphaChannel;
			int _UseAlphaChannel2;
			float4 _Noise_ST;
			int _InvertNoise;
			float4 _Color;
			float4 _EdgeColor;
			float4 _Mask_ST;
			int _MaskAlphaChannel;
			int _InvertMask;
			float _DistortionStrength;
			float _loopSpeed;
			float _MaskDistortionStrength;
			float _EdgeWidth;
			float _YHeight;
			int _UseParticleData;
			vector _vertexAnima;
			int _OnlyUseMask;
			float4 _Tex2Col;
			half _softedgestr;
			half4 _MainTexAnimation;

			CBUFFER_END
			sampler2D _MainTex;
			#ifndef _BLENDTEX_NONE
			sampler2D _MainTex2;
			#endif
			#ifdef _Distortion_ON
			sampler2D _Noise;
			#endif
			#ifdef _Mask_ON
			sampler2D _Mask;
			#endif


			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;

				o.uv.xy = v.uv.xy;
				float2 particleDataOn = v.uv.zw;
				float2 particleDataOff = 0;
				o.uv.zw = lerp(particleDataOff,particleDataOn,_UseParticleData);
				o.particleData.xy = v.custom.xy;
				o.particleData.zw = 0;
				
				o.vertexCol = v.color;
				o.normal = TransformObjectToWorld( v.normal);
				
				#ifdef _Distortion_ON
					float2 uv_noise = o.uv.xy * _Noise_ST.xy + _Noise_ST.zw * fmod(_Time.y,1000);
					float4 noise_col = tex2Dlod(_Noise,float4(uv_noise,0,0));
					v.vertex.xyz += v.normal * Luminance(noise_col.rgb) * _vertexAnima.xyz;
				#else
					v.vertex.xyz += v.normal * _vertexAnima.xyz;
				#endif

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

				float3 WorldPosition = IN.worldPos.xyz;

				// 粒子自定义属性xy(uv.zw)控制mainTex流动速度

				float2 uv = IN.uv.xy;
				float Time = fmod(_Time.y,1000);

				// UV扭曲 flowmap循环动画
				#ifdef _Distortion_ON
					float2 uv_Noise = IN.uv.xy * _Noise_ST.xy + frac(_Noise_ST.zw * Time + 0.0001);
					float4 noiseTexBase = tex2D( _Noise, uv_Noise);
					
					float distortionAnima = 1;
					#ifdef _loopAnima
						float4 noiseTex = noiseTexBase * 2 - 1;
						float2 staticSwitch94 = lerp(noiseTex.rg,( 1.0 - noiseTex.rg ),_InvertNoise);
						float BaseAnima = Time * _loopSpeed;
						distortionAnima = frac(BaseAnima);
					#else
						float4 noiseTex = noiseTexBase * 2 - 0.5;
						float2 staticSwitch94 = lerp(noiseTex.rg,( 1.0 - noiseTex.rg ),_InvertNoise);
					#endif
					uv += staticSwitch94 * distortionAnima * _DistortionStrength;
				#endif

				float2 uv_MainTex = uv * _MainTex_ST.xy + (frac(_MainTex_ST.zw + _MainTexAnimation.xy * Time + 0.0001) * 2 - 1) + IN.uv.zw + float2(1, 1);
				float4 mainTex = tex2D( _MainTex,uv_MainTex);
				mainTex.a = lerp(mainTex.r, mainTex.a, _UseAlphaChannel);

				//粒子自定义属性w(particleData.w)控制扭曲强度

				#if _Distortion_ON && _loopAnima
					float distortionAnima2 = frac(BaseAnima - 0.5);
					float2 uvLA = IN.uv.xy + staticSwitch94 * distortionAnima2 * (_DistortionStrength + IN.particleData.y);
					float2 uv_mainTexLoop = uvLA * _MainTex_ST.xy + frac(_MainTex_ST.zw * Time + 0.001) + IN.uv.zw;
					float4 mainTexLoop = tex2D( _MainTex,uv_mainTexLoop);
					float timeInOut = 1 - abs(distortionAnima * 2 - 1);
					mainTex = lerp(mainTexLoop,mainTex,timeInOut);
				#endif

				float4 TexBlend = mainTex;

				// 双纹理融合				
				#ifdef _BLENDTEX_NONE
					TexBlend = mainTex;
				#else
					float2 uv_Layer2Tex = uv.xy * _MainTex2_ST.xy + (frac(_MainTex2_ST.zw + _MainTexAnimation.zw * Time + 0.001) * 2 - 1) + IN.uv.zw + float2(1, 1);
					float4 Layer2Tex = tex2D( _MainTex2,uv_Layer2Tex);
					Layer2Tex.a = lerp(Layer2Tex.r,Layer2Tex.a,_UseAlphaChannel2);
					TexBlend.a = mainTex.a * Layer2Tex.a;
					#if defined(_BLENDTEX_ADD)
						TexBlend.rgb = mainTex.rgb + Layer2Tex.rgb * Layer2Tex.a * _Tex2Col.rgb;
						
						//TexBlend.a += Layer2Tex.a * _Tex2Col.a;
					#elif defined(_BLENDTEX_MULTIPLY)
						TexBlend.rgb = mainTex.rgb * Layer2Tex.rgb * Layer2Tex.a * _Tex2Col.rgb;
						//TexBlend.a *= Layer2Tex.a * _Tex2Col.a;
					// #elif defined(_BLENDTEX_OVERLAYER)
					// 	float4 TexBlend = ( saturate( (( Layer2Tex > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - Layer2Tex ) * ( 1.0 - mainTex ) ) : ( 2.0 * Layer2Tex * mainTex ) ) ));
					#endif
					
				#endif

				// 去色
				
				#ifdef _Desaturate
					//float3 TexBlend = dot(TexBlend.rgb,float3(0.33,0.33,0.33));
					TexBlend = Luminance(TexBlend);
				#else
					TexBlend = TexBlend;
				#endif

				float mask = 1;
				#ifdef _Mask_ON
					#ifdef _Distortion_ON
						float2 uv_Mask = IN.uv.xy * _Mask_ST.xy + frac(_Mask_ST.zw * Time + 0.001) + staticSwitch94 * _MaskDistortionStrength;
					#else
						float2 uv_Mask = IN.uv.xy * _Mask_ST.xy + frac(_Mask_ST.zw * Time + 0.001);
					#endif
					float4 maskTex = tex2D( _Mask, uv_Mask);
					mask = lerp(maskTex.r,maskTex.a,_MaskAlphaChannel);
					mask = lerp(mask,(1-mask),_InvertMask);
				#endif

				half3 Color = TexBlend.rgb * _Color.rgb * IN.vertexCol.rgb;;
				half Alpha = TexBlend.a * mask;

				// 粒子自定义属性z(particleData.z)值控制溶解速度
				
				float edgeMask = 0;
				
				#ifdef _Dissolve_ON
					half reAlpha = lerp(Alpha, mask, _OnlyUseMask);	
					#ifdef _SoftEdge_ON	
						half softalpha = (reAlpha - IN.particleData.x);
						Alpha = saturate(softalpha + softalpha * _softedgestr * 5) * lerp(1, TexBlend.a, _OnlyUseMask);
						edgeMask = smoothstep(_EdgeWidth, ( _EdgeWidth * 0.1), softalpha) * _EdgeColor.a;
	
					#else
						edgeMask = step(reAlpha, ( _EdgeWidth + IN.particleData.x));
						Alpha = step( saturate(IN.particleData.x) , reAlpha) * lerp(1, TexBlend.a, _OnlyUseMask);
					#endif
				#endif

				Color = lerp(Color,_EdgeColor.rgb,edgeMask) + (_EdgeColor.rgb * edgeMask);

				Alpha = Alpha * IN.vertexCol.a * _Color.a;
				Alpha *= saturate(WorldPosition.y * 2 - _YHeight);

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor);
				#endif
				//return float4(IN.normal,1.0);
				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}
}