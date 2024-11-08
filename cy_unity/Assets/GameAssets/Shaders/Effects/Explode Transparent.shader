Shader "DH/Effects/Explode Transparent"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("剔除模式", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src混合模式",float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst混合模式",float) = 0
		[Toggle]_useParticleData("使用粒子自定义属性",float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("[ZTest]",Range(0,7)) = 4 

		[Space(10)]
		_mainTex("主贴图", 2D) = "white" {}
		[IntRange]_mainMulChannel("主贴图多通道切换", Range(0,3)) = 0
		[HDR]_highLight("高亮颜色", Color) = (1,1,1,0)
		[HDR]_gray("中间颜色", Color) = (0.5,0.5,0.5,0)
		_dark("暗部颜色", Color) = (0,0,0,0)
		_darkRange("暗部范围", Range( -1 , 1)) = 0
		_darkFade("暗部颜色渐变",Range(0,1)) = 0
		_highLightRange("高亮范围",Range(0,1)) = 0
		_hightLightFade("高亮渐变",Range(0,1)) = 1

		_mainTexAnima("主纹理UV动画,顶点动画强度", vector) = (0,0,0,0)

		[Space(10)]	
		[Toggle]_uvChannelSwitch("切换到UV Y通道",float) = 1
		_uvdistmin("UV颜色控制 最小值",Range(-1,1)) = 0
		_uvdistmax("UV颜色控制 最大值",Range(-1,1)) = 1
		_uvDistWeight("UV控制颜色权重", Range(0,10)) = 0
		//_worldDist("距离衰减",Range(0,10)) = 0
		//_distWeight("距离影响权重",Range(0,1)) = 0

		[Space(10)]
		_uvDistVertexMin("UV顶点控制 最小值",Range(-1,1)) = 0
		_uvDistVertexMax("UV顶点控制 最大值",Range(-1,1)) = 1
		_uvDistVertexWeight("UV控制顶点权重",Range(0,10)) = 0
		//_vertexDistRange("距离影响顶点位置范围",range(0,1)) = 0
		//_vertexDistWeight("距离影响顶点位置权重",Range(0,1)) = 0
		
		[Space(10)]
		[HDR]_fresnelCol("菲涅尔颜色",Color) = (1,1,1,0)
		_fresnelRange("菲涅尔范围",Range(-1,1)) = 0
		_fresnelFade("菲涅尔衰减",range(0,1)) = 1
		_fresnelOffset("菲涅尔偏移",vector) = (0,0,0,0)

		_lightrange("灯光范围", Range( 0 , 5)) = 0
		

		[Space(20)]
		[Toggle(_Distortion_ON)]_Distortion_ON("扭曲",float) = 0
		_distortion("扭曲贴图", 2D) = "white" {}
		[IntRange]_DistorMulChannel("扭曲贴图多通道切换", Range(0,3)) = 0
		[Toggle]_distortionSwitch("扭曲Y偏移动画方向", float) = 0
		[Toggle]_useFlowMap("使用FlowMap",float) = 0
		[Toggle]_valueSwitch("从0,1转换到-1,1",float) = 0
		_distorDissAnima("扭曲溶解贴图UV动画及强度",vector) = (0,0,0,0)

		[Space(10)]
		[Toggle(_Dissolve_ON)]_Dissolve_ON("溶解",float) = 0
		//[Toggle]_useMainTex("使用主贴图溶解",float) = 0
		
		_dissolve("溶解贴图", 2D) = "white" {}
		[IntRange]_dissolveMulChannel("溶解贴图多通道切换",Range(0,3)) = 0
		[Toggle]_MainBlendDissolve("主贴图溶解贴图混合",float) = 0
		[Toggle]_addDissolveCol("叠加溶解贴图颜色",float) = 0
		//[Toggle]_MainAddDissolve("主贴图溶解贴图相加",float) = 0
		[Toggle]_invertMask("反相溶解贴图",float) = 0
		[Toggle]_softDissolve("软边溶解",float) = 0

		[Space(10)]
		[HDR]_edgeCol("溶解边缘颜色",color) = (0,0,0,0)
		_edgeWidth("溶解边宽度",Range(0,1)) = 0
		
		[Toggle]_dissolveSwitch("溶解Y偏移动画方向", float) = 0

		[Space(10)]
		_posY("世界位置Y方向透明", Range(-100,100)) = -100
				
		
	}

	SubShader
	{

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "PreviewType" = "Plane" }

		Cull [_CullMode]
		AlphaToMask Off
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend [_Src] [_Dst]
			ZWrite Off
			ZTest [_Ztest]
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#pragma shader_feature_local _Distortion_ON
			#pragma shader_feature_local _Dissolve_ON

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
				float4 particleData0 : TEXCOORD1;
				float4 particleData1 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD1;
				#endif
				float4 uv : TEXCOORD2;
				float4 normal : TEXCOORD3;
				float4 particleDataOutpu0 : TEXCOORD4;
				float4 particleDataOutpu1 : TEXCOORD5;
				float4 color : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _mainTex_ST;
			half4 _distortion_ST;
			half4 _dark;
			half4 _gray;
			half4 _highLight;
			half4 _dissolve_ST;
			int _distortionSwitch;
			int _dissolveSwitch;
			half _darkRange;
			half _lightrange;
			int _useMainTex;
			int _MainBlendDissolve;
			//int _MainAddDissolve;
			int _addDissolveCol;
			//half _worldDist;
			half _hightLightFade;
			half _darkFade;
			half _highLightRange;
			//half _distWeight;
			//half _vertexDistRange;
			//half _vertexDistWeight;
			half _uvdistmin;
			half _uvdistmax;
			half _uvDistWeight;
			half _uvDistVertexMin;
			half _uvDistVertexMax;
			half _uvDistVertexWeight;
			int _uvChannelSwitch;
			int _valueSwitch;
			int _useFlowMap;
			int _invertMask;
			int _useParticleData;
			half4 _mainTexAnima;
			half4 _distorDissAnima;

			int _mainMulChannel;
			int _DistorMulChannel;
			int _dissolveMulChannel;
			int _softDissolve;

			half4 _edgeCol;
			half _edgeWidth;

			half4 _fresnelCol;
			half _fresnelRange;
			half _fresnelFade;
			half4 _fresnelOffset;

			half _posY;

			CBUFFER_END

			sampler2D _mainTex;
			#ifdef _Distortion_ON
			sampler2D _distortion;
			#endif
			#ifdef _Dissolve_ON
			sampler2D _dissolve;
			#endif

			half mulChannelSwitch(half alphaChannel, half4 tex, int shiftFrame){
				alphaChannel = lerp(alphaChannel, tex.y, shiftFrame == 1);
				alphaChannel = lerp(alphaChannel, tex.z, shiftFrame == 2);
				alphaChannel = lerp(alphaChannel, tex.w, shiftFrame == 3);
				return alphaChannel;
			}

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				half Time = fmod(_Time.y,1000);
				//float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				//half dist = length(positionWS) * _vertexDistRange;
				half2 uv_main = v.uv.xy * _mainTex_ST.xy + _mainTex_ST.zw;
				half2 mainOffset = v.uv.zw;
				half2 distortionColToV2 = lerp(_mainTexAnima.xy * Time,0,_useParticleData);
				half2 distortionOffset = 0;
				#ifdef _Distortion_ON
					half distorControlSwitch = lerp(_distorDissAnima.x * Time,v.particleData0.z,_useParticleData);
					half2 distortionOffsetX = half2(distorControlSwitch,0);
					half2 distortionOffsetY = half2(0,distorControlSwitch);
					distortionOffset = lerp(distortionOffsetX,distortionOffsetY,_distortionSwitch);
					half2 uv_dirstortion = v.uv.xy * _distortion_ST.xy + _distortion_ST.zw + distortionOffset;
					half4 distortionTex = tex2Dlod( _distortion, float4(uv_dirstortion, 0, 0.0) ) * v.particleData0.w;
					half distortionCol = mulChannelSwitch(distortionTex.r,distortionTex,_DistorMulChannel);
					distortionColToV2 = distortionCol;
					distortionColToV2 = lerp(distortionColToV2, distortionTex.rg, _useFlowMap);
				#endif
				half mainColGray = tex2Dlod( _mainTex, float4( ( uv_main + mainOffset + distortionColToV2 ), 0, 0.0) ).r;
				
				
				o.particleDataOutpu0 = v.particleData0;
				o.uv = v.uv;
				float3 worldNormal = TransformObjectToWorldNormal(v.normal);
				o.normal.xyz = normalize(worldNormal + half3(mainColGray,mainColGray,mainColGray));
				o.particleDataOutpu1.xy = v.particleData1.xy;
				
				o.particleDataOutpu1.zw = distortionOffset;

				o.particleDataOutpu0 = lerp(half4(_mainTexAnima.zw,_distorDissAnima.x * Time,_distorDissAnima.y),o.particleDataOutpu0,_useParticleData);
				o.particleDataOutpu1.xy = lerp(half2(_distorDissAnima.z * Time,_distorDissAnima.w),o.particleDataOutpu1.xy,_useParticleData);

				o.color = v.color;

				//v.vertex.xyz += v.normal * o.particleDataOutpu0.y * mainColGray * lerp(1,dist,_vertexDistWeight);
				half uvchannal = lerp(v.uv.x,v.uv.y,_uvChannelSwitch);
				half uvdist = smoothstep(uvchannal,_uvDistVertexMin,_uvDistVertexMax) * _uvDistVertexWeight;
				v.vertex.xyz += normalize(v.normal) * o.particleDataOutpu0.y * mainColGray * lerp(1,uvdist,saturate(_uvDistVertexWeight));

				o.normal.w = uvchannal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 WorldPosition = IN.worldPos.xyz;
				//half dist = length(WorldPosition) * _worldDist;
				//dist = lerp(1,dist,_distWeight);

				//half mulColSlider = (dist * IN.particleDataOutpu0.x);
				half uvdist = smoothstep(IN.normal.w,_uvdistmin,_uvdistmax) * _uvDistWeight;
				half mulColSlider = (lerp(1, uvdist,saturate(_uvDistWeight)) * IN.particleDataOutpu0.x);
				//主纹理扭曲
				half2 distortionColToV2 = 0;
				#ifdef _Distortion_ON
					half2 uv_dirstortion = IN.uv.xy * _distortion_ST.xy + _distortion_ST.zw + IN.particleDataOutpu1.zw;
					half4 distortionTex = tex2D( _distortion,uv_dirstortion);
					half distortionCol = mulChannelSwitch(distortionTex.r,distortionTex,_DistorMulChannel);
					distortionCol = lerp(distortionCol,distortionCol * 2 - 1, _valueSwitch);
					distortionColToV2 = distortionCol * IN.particleDataOutpu0.w;
					distortionColToV2 = lerp(distortionColToV2, distortionTex.rg, _useFlowMap);
				#endif

				half2 uv_main = IN.uv.xy * _mainTex_ST.xy + _mainTex_ST.zw + IN.uv.zw;
				half2 mainOffset = lerp(_mainTexAnima.xy,IN.uv.zw,_useParticleData);
				half4 mainTex = tex2D( _mainTex, ( uv_main + mainOffset + distortionColToV2 ) );
				half mainColGray = mulChannelSwitch(mainTex.r,mainTex,_mainMulChannel);

				//half4 darkCol = lerp( _dark , _gray , saturate( smoothstep( mainColGray,mulColSlider,mulColSlider + _darkFade ) ));
				//half fireCol = smoothstep( saturate( ( mulColSlider * _darkRange ) ) , _hightLightFade , mainColGray);
				half range = mulColSlider + _darkRange;
				half fireRange = smoothstep(range, saturate(range + _darkFade), mainColGray);
				half4 darkCol = lerp( _dark , _gray , fireRange);
				half smokerange = saturate(range + _highLightRange);
				half lightfireRange = smoothstep(smokerange, saturate(smokerange + _hightLightFade), mainColGray);
				half4 mainCol = lerp(darkCol,_highLight,lightfireRange);
				
				half3 noiseColToV3 = mainColGray;

				half PosY = saturate(WorldPosition.y - _posY);

				//菲涅尔
				half3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz + _fresnelOffset.xyz - WorldPosition );
				half fresnel = 1 - dot(worldViewDir,IN.normal.xyz);
				half fresnelRemap = smoothstep(_fresnelRange,_fresnelRange + _fresnelFade,fresnel);
				half4 fresnelCol = fresnelRemap * _fresnelCol;

				//lambert光照
				half NdotL = dot( IN.normal.xyz , _MainLightPosition.xyz );
				half baseLight = saturate( ( saturate( ( floor( ( NdotL * _lightrange ) ) / _lightrange ) ) + 0.8 ) );
				half3 Color = ( mainCol * baseLight + fresnelCol).rgb * IN.color.rgb;
				//half Alpha = (lerp(1,mainColGray,_useMainTex) + fresnelCol.r) * IN.color.a;
				half Alpha = (mainColGray + fresnelCol.r) * IN.color.a;

				half AlphaClipThreshold = IN.particleDataOutpu1.y;

				//溶解
				#ifdef _Dissolve_ON
					half2 uv_dissolve = IN.uv.xy * _dissolve_ST.xy + _dissolve_ST.zw;
					half2 dissolveOffsetX = half2(IN.particleDataOutpu1.x,0);
					half2 dissolveOffsetY = half2(0,IN.particleDataOutpu1.x);
					half2 dissolveOffset = lerp(dissolveOffsetX,dissolveOffsetY,_dissolveSwitch);				
					half4 dissolveTex = tex2D( _dissolve, ( uv_dissolve + dissolveOffset ) );
					half dissolveCol = mulChannelSwitch(dissolveTex.r,dissolveTex,_dissolveMulChannel);
					dissolveCol = lerp(dissolveCol,1 - dissolveCol,_invertMask);
					Color = lerp(Color,Color * dissolveCol,_addDissolveCol);
					Alpha = lerp(dissolveCol,saturate(mainColGray * dissolveCol),_MainBlendDissolve) * IN.color.a;
					//Color = lerp(Color, smoothstep((IN.particleDataOutpu1.y),1,Color), _softDissolve);
					
					//Alpha = lerp(Alpha,saturate(lerp(Alpha * dissolveCol,Alpha + dissolveCol, _MainAddDissolve)),_MainBlendDissolve) * IN.color.a;
				#endif
				Alpha = lerp(Alpha, smoothstep((IN.particleDataOutpu1.y),1,Alpha), _softDissolve);
				AlphaClipThreshold = lerp(AlphaClipThreshold, 0, _softDissolve);
				Alpha *= PosY;

				//溶解描边
				half3 edgeRange = smoothstep(Alpha,IN.particleDataOutpu1.y + _edgeWidth,1);
				Color = lerp(Color,_edgeCol.rgb,edgeRange.r) * PosY; 
				
				
				clip( Alpha - AlphaClipThreshold );

				

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