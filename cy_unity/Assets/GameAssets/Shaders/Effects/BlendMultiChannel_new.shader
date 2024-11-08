Shader "DH/Effects/BlendMultiChannel_new"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("[Cull Mode]", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _Src("[Src]", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _Dst("[Dst]", Float) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("[ZTest]",Range(0,7)) = 4 

		//[Toggle(_ALPHAPREMULTIPLY_ON)]_ALPHAPREMULTIPLY_ON("Alpha预乘", float) = 0

		[Space(10)]
		[Toggle]_UseParticleData("使用粒子自定义数据", float) = 0
		[Toggle]_UseMulChannel("使用多通道", float) = 0
		
		[Space(10)]
		[HDR]_Color("主颜色", Color) = (1,1,1,1)
		_DarkColor("主纹理暗部颜色", Color) = (1,1,1,0)
		_MainTex("主纹理", 2D) = "white" {}
		[Toggle]_UseAlphaChannel("使用透明通道", float) = 1
        [Toggle]_Main2Mask("用主纹理做遮罩(多通道下)", float) = 0

        //遮罩
		[Space(20)]
		[Toggle(_Mask_ON)]_Mask_ON("打开遮罩", float) = 0
		_Mask("遮罩贴图", 2D) = "White" {}
		[Toggle]_MaskAlphaChannel("使用透明通道", float) = 1
		[Toggle]_MaskInvert("遮罩反相", float) = 0
		_MaskPow("遮罩强度", Range(0,10)) = 0.5
		_MaskRange("遮罩范围", Range(0,5)) = 0

		[Space(15)]
		_UVoffset("UV动画(xy主纹理, zw遮罩贴图)", vector) = (0,0,0,0)

        //顶点动画
		[Space(10)]
		[Toggle(_vertexAnimation_ON)]_vertexAnimation_ON("打开顶点动画", float) = 0
		_vertexNoiseTex("顶点动画贴图",2D) = "white" {}
		[IntRange]_vertexNoiseMulChannel("顶点动画贴图多通道",Range(0,3)) = 0
		_vertexOffset("顶点动画", vector) = (0,0,0,1)
		//_vertexTillingOffset("Vertex Tilling Offset",vector) = (1,1,0,0)
		
        //菲涅尔
		[Space(10)]
		[Toggle(_fresnel_ON)]_fresnel_ON("打开菲涅尔", float) = 0
		[HDR]_fresnelCol("菲涅尔颜色", color) = (1,1,1,1)
		_fresnelRange("菲涅尔范围", Range(0,1)) = 0.5
		_fresnelStr("菲涅尔强度", Range(0,2)) = 1
		_fresnelOffset("菲涅尔偏移", vector) = (0,0,0,0)


        //主纹理扭曲
		[Toggle(_Distortion_ON)]_Distortion_ON("打开扭曲", float) = 0
		_distortionTex("扭曲贴图", 2D) = "White" {}
		[IntRange]_DistorMulChannel("扭曲贴图多通道", Range(0,3)) = 0
		_DistorStrength("扭曲强度", Range(-1,1)) = 0
		//_Main2Distortion("Use MainTex To Distortion", float) = 0
		[Toggle]_TexValueRangeShift("贴图值切换(从0--1切换到-1--1)",float) = 0
		[Toggle]_useFlowmap("使用Flowmap",float) = 0
		//[Toggle]_distorShiftY("动画方向切换(粒子自定义属性下,UV切换到Y方向)",float) = 0
		_distorDir("扭曲方向及动画方向",vector) = (1,1,1,1)

		[Space(15)]
		_distortionUVOffset("扭曲 溶解UV动画(xy扭曲, zw溶解)", vector) = (0,0,0,0)
		
        //溶解
		[Toggle(_Dissolve_ON)]_Dissolve_ON("打开溶解",float) = 0
		_dissolveTex("溶解贴图", 2D) = "white" {}
		[IntRange]_dissolveMulChannel("溶解贴图多通道",Range(0,3)) = 0
		[Toggle]_dissovleMask("溶解贴图遮罩贴图合并溶解",float) = 0
		[Toggle]_softEdge("使用软边溶解", float) = 0
		_dissovleValue("溶解值", Range(0,1)) = 0
		_loppSpeed("循环动画速度",Range(0,5)) = 1
		_loopAnima("循环动画",vector) = (0,0,0,0)
		
		
		[HDR]_edgeCol("溶解边颜色",color) = (1,1,1,1)
		_edgeWidth("溶解边宽度", Range(0,0.2)) = 0.05
		
		
		[Toggle]_dissShiftY("动画方向切换(粒子自定义属性下,UV切换到Y方向)",float) = 0

		_YHeight("世界坐标Y方向透明", Range(-200,200)) = -200

		[HideInInspector]__basegroup("basegroup",vector) = (0,0,0,0)
		[HideInInspector]__basegroup2("basegroup2",vector) = (0,0,0,0)

		[HideInInspector]__blendIndex("Blend Index",float) = 0
		[HideInInspector]__cullIndex("Cull Index", float) = 0
		[HideInInspector]__ztestIndex("ZTest Index",float) = 0
		

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
			//#pragma shader_feature_local _ALPHAPREMULTIPLY_ON
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
			float4 _distortionTex_ST;
			float4 _dissolveTex_ST;
			half4 _vertexNoiseTex_ST;
			int _UseAlphaChannel;
			int _UseParticleData;
			int _UseMulChannel;
			int _MaskAlphaChannel;
			int _MaskInvert;
			half4 _Color;
			half4 _DarkColor;
			half _YHeight;
			half4 _UVoffset;
			half _Main2Mask;
			half _MaskPow;
			half _MaskRange;
			half4 _distortionUVOffset;
			half _DistorStrength;
			//int _Main2Distortion;
			int _DistorMulChannel;
			//int _distorShiftY;
			half4 _distorDir;
			half _TexValueRangeShift;
			half4 _edgeCol;
			half _edgeWidth;
			int _softEdge;
			int _useFlowmap;
			int _dissolveMulChannel;
			int _dissovleMask;
			half _dissovleValue;
			int _dissShiftY;
			half4 _vertexOffset;
			//half4 _vertexTillingOffset;
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
				sampler2D _distortionTex;
			#endif
			#ifdef _Dissolve_ON
				sampler2D _dissolveTex;
			#endif
			#ifdef _vertexAnimation_ON
				sampler2D _vertexNoiseTex;
			#endif

			float mulChannelSwitch(float alphaChannel, float4 tex, int shiftFrame){
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

				o.normalWS = mul(unity_ObjectToWorld,float4( normalize(v.normal.xyz) , 0.0 )).xyz;
				

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
				half2 uvdistortion = 0;
				#ifdef _Distortion_ON
					//float2 distorShiftY = lerp(float2(IN.pColData.y,0), float2(0,IN.pColData.y), _distorShiftY);
					float2 distorShiftY = IN.pColData.y * _distorDir.zw;
					float2 distortionTexOffset = lerp(_distortionUVOffset.xy * Time, distorShiftY, _UseParticleData);
					float2 uv_distortion = uv * _distortionTex_ST.xy + _distortionTex_ST.zw + distortionTexOffset;
					distortionTex = tex2D(_distortionTex, uv_distortion);
					distortionTex = lerp(distortionTex, distortionTex * 2 - 1, _TexValueRangeShift);					
					half4 DistormulChannelTex = mulChannelSwitch(distortionTex.r,distortionTex,_DistorMulChannel);
					distortionTex = lerp(DistormulChannelTex,distortionTex,_useFlowmap);

					//uvdistortion = distortionTex.xy * lerp(_DistorStrength,IN.particleCurve.y,_UseParticleData);
					uvdistortion = distortionTex.xy * lerp(_DistorStrength,IN.particleCurve.y,_UseParticleData) * _distorDir.xy;
				#endif				
				

				float2 uv_MainTex = uv *  _MainTex_ST.xy + (frac(_MainTex_ST.zw + _UVoffset.xy * Time + 0.0001) * 2 - 1) + IN.uv.zw + float2(1,1) + uvdistortion;
				half4 mainTex = tex2D( _MainTex,uv_MainTex);
				half mainTexAlphaShift = lerp(mainTex.r,mainTex.a,_UseAlphaChannel);
				
				half Alpha = mainTexAlphaShift;
				
				

				//主纹理多通道切换
				half AlphaMul = 0;
				half split = frac(IN.pColData.a / 4) * 4;
				int shift = floor(split);
				AlphaMul = mulChannelSwitch(Alpha,mainTex,shift);

                mainTex = lerp(mainTex,AlphaMul,_UseMulChannel);

                half3 Color = (mainTex * _Color).rgb;
				Color = lerp(_DarkColor.rgb,Color,AlphaMul);

				Alpha = lerp(Alpha, AlphaMul, _UseMulChannel);

				//mask纹理多通道切换
				half4 maskAlphaMul = 1;
				int maskshift = int(frac(IN.pColData.a) * 12 / 3);
				//使用主纹理多通道做为mask，可以利用多通道，减少mask纹理采样（主纹理中有适合的mask纹理的情况下），
				half4 mask = lerp(maskAlphaMul,mainTex,_Main2Mask);
				half maskOutput = lerp(1,mulChannelSwitch(mainTexAlphaShift,mask,maskshift),_Main2Mask);
				#ifdef _Mask_ON
					float2 uv_mask = uv * _Mask_ST.xy + _Mask_ST.zw + _UVoffset.zw * Time;
					mask = tex2D(_Mask,uv_mask);
					half4 maskAlphaShift = lerp(mask.r, mask.a, _MaskAlphaChannel);
					//通道切换
					maskOutput = mulChannelSwitch(maskAlphaShift.r,mask,maskshift);
					maskAlphaMul = lerp(maskAlphaShift.r,maskOutput,_UseMulChannel);
					
					maskOutput = lerp(maskAlphaMul, 1 - maskAlphaMul, _MaskInvert);
					maskOutput = saturate(maskOutput * _MaskPow * 2 - _MaskRange);
					
				#endif				

				Alpha *= maskOutput;
				//Color = ceil(_Src)==1 ? (Color * maskOutput * AlphaMul) : Color;

				//溶解
				half dissolveOutput = 1;
				#ifdef _Dissolve_ON
					float2 distssShiftY = lerp(float2(IN.pColData.x,0), float2(0,IN.pColData.x), _dissShiftY);
					float2 dissolveTexOffest = lerp(_distortionUVOffset.zw * Time, distssShiftY,_UseParticleData);
					float2 uv_dissolve = uv * _dissolveTex_ST.xy + _dissolveTex_ST.zw + dissolveTexOffest;
					half4 dissolveCol = tex2D(_dissolveTex,uv_dissolve);

					half dissolveValue = IN.particleCurve.x + _dissovleValue;
					
					half dissolveGray = mulChannelSwitch(dissolveCol.r,dissolveCol,_dissolveMulChannel);
					dissolveGray = lerp(dissolveGray,dissolveGray * maskOutput,_dissovleMask);
					
					half hard = step(dissolveValue,dissolveGray);
					half edgeHardWidth = saturate(hard - step(dissolveValue + _edgeWidth,dissolveGray));

					half soft = smoothstep(1,dissolveGray,dissolveValue);
					half edgeSoftWidth = saturate(soft - smoothstep(1,saturate(dissolveGray - _edgeWidth),dissolveValue));
					
					dissolveOutput = lerp(hard,soft,_softEdge);
					half edgeWidthOutput = lerp(edgeHardWidth,edgeSoftWidth,_softEdge);

					Color = lerp(Color,_edgeCol.rgb,edgeWidthOutput);
					Alpha *= dissolveOutput;
				#endif

				//菲涅尔
				#ifdef _fresnel_ON
					float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
					viewDir = normalize(viewDir + _fresnelOffset.xyz);
					half VdotN = dot(viewDir,IN.normalWS);
					half3 fresnel = saturate(1 - VdotN / _fresnelRange) * _fresnelStr;
					Color += fresnel * _fresnelCol.rgb;
					Alpha *= fresnel.r;
				#endif

				//#ifdef _ALPHAPREMULTIPLY_ON
				//	Color *= maskOutput * AlphaMul * dissolveOutput;
				//#endif
				
				//世界空间Y轴渐变透明，减弱与地面交接处穿插
				Alpha *= saturate(WorldPosition.y * 2 - _YHeight) * _Color.a * IN.vertexCol.a;

				Color *= IN.vertexCol.rgb;

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif
				return half4( Color, Alpha );
			}

			ENDHLSL
		}
		//2D
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }


			
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
			//#pragma shader_feature_local _ALPHAPREMULTIPLY_ON
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
			float4 _distortionTex_ST;
			float4 _dissolveTex_ST;
			half4 _vertexNoiseTex_ST;
			int _UseAlphaChannel;
			int _UseParticleData;
			int _UseMulChannel;
			int _MaskAlphaChannel;
			int _MaskInvert;
			half4 _Color;
			half4 _DarkColor;
			half _YHeight;
			half4 _UVoffset;
			half _Main2Mask;
			half _MaskPow;
			half _MaskRange;
			half4 _distortionUVOffset;
			half _DistorStrength;
			//int _Main2Distortion;
			int _DistorMulChannel;
			//int _distorShiftY;
			half4 _distorDir;
			half _TexValueRangeShift;
			half4 _edgeCol;
			half _edgeWidth;
			int _softEdge;
			int _useFlowmap;
			int _dissolveMulChannel;
			int _dissovleMask;
			half _dissovleValue;
			int _dissShiftY;
			half4 _vertexOffset;
			//half4 _vertexTillingOffset;
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
				sampler2D _distortionTex;
			#endif
			#ifdef _Dissolve_ON
				sampler2D _dissolveTex;
			#endif
			#ifdef _vertexAnimation_ON
				sampler2D _vertexNoiseTex;
			#endif

			float mulChannelSwitch(float alphaChannel, float4 tex, int shiftFrame){
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

				o.normalWS = mul(unity_ObjectToWorld,float4( normalize(v.normal.xyz) , 0.0 )).xyz;
				

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
				half2 uvdistortion = 0;
				#ifdef _Distortion_ON
					//float2 distorShiftY = lerp(float2(IN.pColData.y,0), float2(0,IN.pColData.y), _distorShiftY);
					float2 distorShiftY = IN.pColData.y * _distorDir.zw;
					float2 distortionTexOffset = lerp(_distortionUVOffset.xy * Time, distorShiftY, _UseParticleData);
					float2 uv_distortion = uv * _distortionTex_ST.xy + _distortionTex_ST.zw + distortionTexOffset;
					distortionTex = tex2D(_distortionTex, uv_distortion);
					distortionTex = lerp(distortionTex, distortionTex * 2 - 1, _TexValueRangeShift);					
					half4 DistormulChannelTex = mulChannelSwitch(distortionTex.r,distortionTex,_DistorMulChannel);
					distortionTex = lerp(DistormulChannelTex,distortionTex,_useFlowmap);

					//uvdistortion = distortionTex.xy * lerp(_DistorStrength,IN.particleCurve.y,_UseParticleData);
					uvdistortion = distortionTex.xy * lerp(_DistorStrength,IN.particleCurve.y,_UseParticleData) * _distorDir.xy;
				#endif				
				

				float2 uv_MainTex = uv *  _MainTex_ST.xy + (frac(_MainTex_ST.zw + _UVoffset.xy * Time + 0.0001) * 2 - 1) + IN.uv.zw + float2(1,1) + uvdistortion;
				half4 mainTex = tex2D( _MainTex,uv_MainTex);
				half mainTexAlphaShift = lerp(mainTex.r,mainTex.a,_UseAlphaChannel);
				
				half Alpha = mainTexAlphaShift;
				
				

				//主纹理多通道切换
				half AlphaMul = 0;
				half split = frac(IN.pColData.a / 4) * 4;
				int shift = floor(split);
				AlphaMul = mulChannelSwitch(Alpha,mainTex,shift);

                mainTex = lerp(mainTex,AlphaMul,_UseMulChannel);

                half3 Color = (mainTex * _Color).rgb;
				Color = lerp(_DarkColor.rgb,Color,AlphaMul);

				Alpha = lerp(Alpha, AlphaMul, _UseMulChannel);

				//mask纹理多通道切换
				half4 maskAlphaMul = 1;
				int maskshift = int(frac(IN.pColData.a) * 12 / 3);
				//使用主纹理多通道做为mask，可以利用多通道，减少mask纹理采样（主纹理中有适合的mask纹理的情况下），
				half4 mask = lerp(maskAlphaMul,mainTex,_Main2Mask);
				half maskOutput = lerp(1,mulChannelSwitch(mainTexAlphaShift,mask,maskshift),_Main2Mask);
				#ifdef _Mask_ON
					float2 uv_mask = uv * _Mask_ST.xy + _Mask_ST.zw + _UVoffset.zw * Time;
					mask = tex2D(_Mask,uv_mask);
					half4 maskAlphaShift = lerp(mask.r, mask.a, _MaskAlphaChannel);
					//通道切换
					maskOutput = mulChannelSwitch(maskAlphaShift.r,mask,maskshift);
					maskAlphaMul = lerp(maskAlphaShift.r,maskOutput,_UseMulChannel);
					
					maskOutput = lerp(maskAlphaMul, 1 - maskAlphaMul, _MaskInvert);
					maskOutput = saturate(maskOutput * _MaskPow * 2 - _MaskRange);
					
				#endif				

				Alpha *= maskOutput;
				//Color = ceil(_Src)==1 ? (Color * maskOutput * AlphaMul) : Color;

				//溶解
				half dissolveOutput = 1;
				#ifdef _Dissolve_ON
					float2 distssShiftY = lerp(float2(IN.pColData.x,0), float2(0,IN.pColData.x), _dissShiftY);
					float2 dissolveTexOffest = lerp(_distortionUVOffset.zw * Time, distssShiftY,_UseParticleData);
					float2 uv_dissolve = uv * _dissolveTex_ST.xy + _dissolveTex_ST.zw + dissolveTexOffest;
					half4 dissolveCol = tex2D(_dissolveTex,uv_dissolve);

					half dissolveValue = IN.particleCurve.x + _dissovleValue;
					
					half dissolveGray = mulChannelSwitch(dissolveCol.r,dissolveCol,_dissolveMulChannel);
					dissolveGray = lerp(dissolveGray,dissolveGray * maskOutput,_dissovleMask);
					
					half hard = step(dissolveValue,dissolveGray);
					half edgeHardWidth = saturate(hard - step(dissolveValue + _edgeWidth,dissolveGray));

					half soft = smoothstep(1,dissolveGray,dissolveValue);
					half edgeSoftWidth = saturate(soft - smoothstep(1,saturate(dissolveGray - _edgeWidth),dissolveValue));
					
					dissolveOutput = lerp(hard,soft,_softEdge);
					half edgeWidthOutput = lerp(edgeHardWidth,edgeSoftWidth,_softEdge);

					Color = lerp(Color,_edgeCol.rgb,edgeWidthOutput);
					Alpha *= dissolveOutput;
				#endif

				//菲涅尔
				#ifdef _fresnel_ON
					float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
					viewDir = normalize(viewDir + _fresnelOffset.xyz);
					half VdotN = dot(viewDir,IN.normalWS);
					half3 fresnel = saturate(1 - VdotN / _fresnelRange) * _fresnelStr;
					Color += fresnel * _fresnelCol.rgb;
					Alpha *= fresnel.r;
				#endif

				//#ifdef _ALPHAPREMULTIPLY_ON
				//	Color *= maskOutput * AlphaMul * dissolveOutput;
				//#endif
				
				//世界空间Y轴渐变透明，减弱与地面交接处穿插
				Alpha *= saturate(WorldPosition.y * 2 - _YHeight) * _Color.a * IN.vertexCol.a;

				Color *= IN.vertexCol.rgb;

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif
				return half4( Color, Alpha );
			}

			ENDHLSL
		}

	}
	//CustomEditor "BlendGUI"
}