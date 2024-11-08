Shader "DH/Effects/BlendMultiChannel"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("[Cull Mode]", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("[Src]", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("[Dst]", Float) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("[ZTest]",Range(0,7)) = 4 

		[Space(10)]
		_UseParticleData("UseParticleData", float) = 0
		_UseMulChannel("UseMulChannel", float) = 0
		
		[Space(10)]
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_UseAlphaChannel("UseAlphaChannel", float) = 1

		[Space(10)]
		_Main2Mask("Use MainTex To Mask", float) = 0
		_MaskInvert("Mask Invert", float) = 0
		//[Toggle(_Mask_ON)]_Mask_ON("Mask Texture ON", float) = 0
		_MaskAlphaChannel("UseMaskAlphaChannel", float) = 1
		_Mask("Mask", 2D) = "White" {}
		_MaskPow("Mask Strenght", Range(1,10)) = 1
		_MaskRange("Mask Range", Range(0,5)) = 0

		_UVoffset("UV Offset(xy main,zw mask)", vector) = (0,0,0,0)

		[Space(10)]
		//[Toggle(_vertexAnimation_ON)]_vertexAnimation_ON("Vertex Animation ON", float) = 0
		_vertexNoiseMulChannel("Noise Mul Channel",Integer) = 0
		_vertexNoiseTex("Vertex Noise",2D) = "white" {}
		_vertexTillingOffset("Vertex Tilling Offset",vector) = (1,1,0,0)
		_vertexOffset("Vertex Offset", vector) = (0,0,0,1)

		_YHeight("YHeight", Range(-200,200)) = 0

		[Space(10)]
		[HDR]_fresnelCol("Fresnel Color", color) = (1,1,1,1)
		_fresnelRange("Fresnel Range", Range(0,1)) = 0.5
		_fresnelStr("Fresnel Strength", Range(0,2)) = 0
		_fresnelOffset("Fresnel Offset", vector) = (0,0,0,0)



		//[Toggle(_Distortion_ON)]_Distortion_ON("Distortion ON", float) = 0
		_DistorMulChannel("Distortion Mull Channel", Integer) = 0
		//_Main2Distortion("Use MainTex To Distortion", float) = 0
		_TexValueRangeShift("Tex Value Range Shift (0--1 to -1--1)",float) = 0
		_disrotionTex("Distortion", 2D) = "White" {}
		_disrotionUVOffset("UV Offset(xy distortion, zw dissolve)", vector) = (0,0,0,0)

		//[Toggle(_Dissolve_ON)]_Dissolve_ON("Dissolve ON",float) = 0
		_useFlowmap("Use Flowmap",float) = 0
		_dissolveMulChannel("Dissolve Mul Channel",Integer) = 0
		_dissovleMask("Dissolve Mask",float) = 0
		_loopAnima("Loop Animation",vector) = (0,0,0,0)
		_loppSpeed("Loop Speed",Range(0,5)) = 1
		_dissolveTex("Dissolve", 2D) = "white" {}
		[HDR]_edgeCol("Edge Color",color) = (1,1,1,1)
		_edgeWidth("Edge Width", Range(0,0.2)) = 0.05
		_softEdge("Soft Edge", float) = 0

		__group("GUI Group", vector) = (0,0,0,0)
		__fresnelgroup("Fresnel Group",float) = 0
		__fresnelon("fresnelon",float) = 0
		__blendIndex("Blend Index",float) = 0
		__cullIndex("Cull Index", float) = 0
		__ztestIndex("ZTest Index",float) = 0
		

	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "PreviewType" = "Plane"}
		
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
			#pragma shader_feature_local _Mask_ON
			#pragma shader_feature_local _Distortion_ON
			#pragma shader_feature_local _Dissolve_ON
			#pragma shader_feature_local _vertexAnimation_ON
			#pragma shader_feature_local _fresnel_ON
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
				half4 particleCurve : TEXCOORD1;
				half2 custom : TEXCOORD2;
				
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float4 uv : TEXCOORD0;
				half4 pColData : TEXCOORD4;
				half4 vertexCol : TEXCOORD2;
				half2 particleCurve : TEXCOORD5;
				#ifdef FOG
				float fogFactor : TEXCOORD3;
				#endif
				float3 normalWS : TEXCOORD6;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Mask_ST;
			float4 _disrotionTex_ST;
			float4 _dissolveTex_ST;
			half4 _vertexNoiseTex_ST;
			int _UseAlphaChannel;
			int _UseParticleData;
			int _UseMulChannel;
			int _MaskAlphaChannel;
			int _MaskInvert;
			half4 _Color;
			half _YHeight;
			half4 _UVoffset;
			half _Main2Mask;
			half _MaskPow;
			half _MaskRange;
			half4 _disrotionUVOffset;
			//int _Main2Distortion;
			int _DistorMulChannel;
			half _TexValueRangeShift;
			half4 _edgeCol;
			half _edgeWidth;
			int _softEdge;
			int _useFlowmap;
			int _dissolveMulChannel;
			int _dissovleMask;
			half4 _vertexOffset;
			half4 _vertexTillingOffset;
			int _vertexNoiseMulChannel;
			half4 _fresnelCol;
			half _fresnelRange;
			half _fresnelStr;
			half4 _fresnelOffset;
			half4 _loopAnima;
			half _loppSpeed;
			CBUFFER_END
			sampler2D _MainTex;
			#ifdef _Mask_ON
				sampler2D _Mask;
			#endif
			#ifdef _Distortion_ON
				sampler2D _disrotionTex;
			#endif
			#ifdef _Dissolve_ON
				sampler2D _dissolveTex;
			#endif
			#ifdef _vertexAnimation_ON
				sampler2D _vertexNoiseTex;
			#endif

			float mulChannelSwitch(float alphaChannel, float4 tex, int shiftFrame){
				alphaChannel = lerp(alphaChannel, tex.x, shiftFrame == 0);
				alphaChannel = lerp(alphaChannel, tex.y, shiftFrame == 1);
				alphaChannel = lerp(alphaChannel, tex.z, shiftFrame == 2);
				alphaChannel = lerp(alphaChannel, tex.w, shiftFrame == 3);
				return alphaChannel;
			}

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				half3 Time = fmod(_Time.y,1000);
				half3 sinloop = sin(v.vertex.xyz * _loopAnima.xyz + Time * _loppSpeed * _loopAnima.xyz);
				v.particleCurve.x += dot(sinloop,sinloop);	

				o.uv.xy = v.uv.xy;
				half2 particleDataOn = v.uv.zw;
				half2 particleDataOff = 0;
				o.uv.zw = lerp(particleDataOff,particleDataOn,_UseParticleData);
				o.vertexCol = v.color;
				o.particleCurve = v.particleCurve.xy;
				o.pColData = half4(v.particleCurve.zw,v.custom.xy);

				o.normalWS = mul(GetObjectToWorldMatrix(),float4( normalize(v.normal.xyz) , 0.0 )).xyz;
				

				#ifdef _vertexAnimation_ON
					half2 uv_vertexNoise = v.uv.xy * _vertexNoiseTex_ST.xy + _vertexNoiseTex_ST.zw * Time.x;
					half4 vertexNoiseCol = tex2Dlod(_vertexNoiseTex,half4(uv_vertexNoise,0,0));
					vertexNoiseCol = mulChannelSwitch(vertexNoiseCol.r,vertexNoiseCol,_vertexNoiseMulChannel);
					v.vertex.xyz += v.normal.xyz * _vertexOffset.xyz * vertexNoiseCol.rgb;
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
				float3 WorldPosition = IN.worldPos;
				float2 uv = IN.uv.xy;
				half Time = fmod(_Time.y,1000);
				
				//扭曲
				half4 distortionTex = 0;
				float2 uvdistortion = uv;
				#ifdef _Distortion_ON
					float2 uv_distortion = uv * _disrotionTex_ST.xy + _disrotionTex_ST.zw + _disrotionUVOffset.zw * Time;
					distortionTex = tex2D(_disrotionTex, uv_distortion);
					distortionTex = lerp(distortionTex, distortionTex * 2 - 1, _TexValueRangeShift);					
					half4 DistormulChannelTex = mulChannelSwitch(distortionTex.r,distortionTex,_DistorMulChannel);
					distortionTex = lerp(DistormulChannelTex,distortionTex,_useFlowmap);

					uvdistortion = uv + distortionTex.xy * lerp(_disrotionUVOffset.xy,IN.particleCurve.y,_UseParticleData);
				#endif
				
				

				float2 uv_MainTex = uvdistortion * _MainTex_ST.xy + (frac(_MainTex_ST.zw + _UVoffset.xy * Time + 0.0001) * 2 - 1) + IN.uv.zw + float2(1,1);
				half4 mainTex = tex2D( _MainTex,uv_MainTex);
				half mainTexAlphaShift = lerp(mainTex.r,mainTex.a,_UseAlphaChannel);
				
				half Alpha = mainTexAlphaShift * IN.vertexCol.a * _Color.a;
				half3 Color = (mainTex * _Color * IN.vertexCol).rgb;
				

				//主纹理多通道切换
				half AlphaMul = 0;
				half split = IN.pColData.a * 10 / 3;
				int shift = int(split);
				AlphaMul = mulChannelSwitch(AlphaMul,mainTex,shift) * IN.vertexCol.a;
				half3 ColorMul = lerp(IN.pColData, _Color * IN.vertexCol, AlphaMul).rgb;

				Alpha = lerp(Alpha, AlphaMul, _UseMulChannel);
				Color = lerp(Color, ColorMul, _UseMulChannel);

				//mask纹理多通道切换
				half maskAlphaMul = 1;
				int maskshift = int(frac(split) * 12 / 3);
				//int maskshift = int(frac(split) * 10 / 3);
				//使用主纹理多通道做为mask，可以利用多通道，减少mask纹理采样（主纹理中有适合的mask纹理的情况下），
				half4 mask = lerp(maskAlphaMul,mainTex,_Main2Mask);
				#ifdef _Mask_ON
					float2 uv_mask = uv * _Mask_ST.xy + _Mask_ST.zw + _UVoffset.zw * Time;
					mask = tex2D(_Mask,uv_mask);
					half4 maskAlphaShift = lerp(mask.r, mask.a, _MaskAlphaChannel);
					mask = lerp(maskAlphaShift,mask,_UseMulChannel);
				#endif
				//通道切换
				maskAlphaMul = mulChannelSwitch(maskAlphaMul,mask,maskshift);
				maskAlphaMul = lerp(maskAlphaMul, 1 - maskAlphaMul, _MaskInvert);
				maskAlphaMul = saturate(maskAlphaMul * _MaskPow * 2 - _MaskRange);

				Alpha *= maskAlphaMul;
				Color *= maskAlphaMul;

				//溶解
				#ifdef _Dissolve_ON
					float2 uv_dissolve = uv * _dissolveTex_ST.xy + _dissolveTex_ST.zw * Time;
					half4 dissolveCol = tex2D(_dissolveTex,uv_dissolve);
					
					half dissolveGray = mulChannelSwitch(dissolveCol.r,dissolveCol,_dissolveMulChannel);
					dissolveGray = lerp(dissolveGray,dissolveGray * maskAlphaMul,_dissovleMask);
					
					half hard = step(IN.particleCurve.x,dissolveGray);
					half edgeHardWidth = saturate(hard - step(IN.particleCurve.x + _edgeWidth,dissolveGray));

					half soft = smoothstep(1,dissolveGray,IN.particleCurve.x);
					half edgeSoftWidth = saturate(soft - smoothstep(1,saturate(dissolveGray - _edgeWidth),IN.particleCurve.x));
					
					half dissolveOutput = lerp(hard,soft,_softEdge);
					half edgeWidthOutput = lerp(edgeHardWidth,edgeSoftWidth,_softEdge);

					Color = lerp(Color,_edgeCol.rgb,edgeWidthOutput * Alpha);
					Alpha *= dissolveOutput;
					Color *= dissolveOutput;
				#endif

				//菲涅尔
				#ifdef _fresnel_ON
					float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
					viewDir = normalize(viewDir + _fresnelOffset.xyz);
					half VdotN = dot(viewDir,IN.normalWS);
					half3 fresnel = saturate(1 - VdotN / _fresnelRange) * _fresnelStr;
					Color *= fresnel * _fresnelCol.rgb;
					Alpha *= fresnel.r;
				#endif
				
				//世界空间Y轴渐变透明，减弱与地面交接处穿插
				Alpha *= saturate(WorldPosition.y * 2 - _YHeight);

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif
				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}
	CustomEditor "BlendGUI"
}