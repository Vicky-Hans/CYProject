Shader "DH/Effects/Range"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("剔除模式", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src混合模式",float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst混合模式",float) = 10

		[Toggle]_UseParticleData("使用粒子自定义属性", Float) = 0
		[HDR]_EdgeColor("边颜色", Color) = (1,1,1,1)
		_DarkColor("中间颜色", Color) = (0,0,0,0)
		_Scale("缩放", Range( 0 , 1)) = 0
		_Width("宽度", Range( 0 , 1)) = 0.1
		[Toggle(_CircleON)]_CircleON("圆形", Float) = 0
		_SoftEdge("柔边", Range( 0 , 1)) = 0
		_SoftEdgeOutside("外柔边", Range( 0 , 1)) = 0
		_SoftEdgeInside("内柔边", Range( 0 , 1)) = 0
		[Toggle]_SingleEdge("双边", Float) = 1
		_FilletStr("圆角强度", Range( 1 , 10)) = 1
		[Toggle(_UseTexON)]_UseTexON("使用纹理", Float) = 0
		[Toggle]_UseAlphaChannel("使用Alpha通道",float) = 0

		_MainTex("主贴图", 2D) = "white" {}
		_Edge2("扇形左侧边（旋转）", 2D) = "black" {}
		_Edge1("扇形右侧边", 2D) = "black" {}
		[Toggle(_SectorON)]_SectorON("扇形", Float) = 0
		_Sector("扇形范围", Range( 0 , 1)) = 0.5
        _RotatePivotAngle("旋转中心点和角度", Vector) = (0.5,0.5,0,0)

	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull [_CullMode]
		AlphaToMask Off
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }
			
			Blend [_Src] [_Dst] //,One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma multi_compile_instancing

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature_local _UseTexON
			#pragma shader_feature_local _CircleON
			#pragma shader_feature_local _SectorON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 particleCustomData : TEXCOORD1;
				float4 particleCustomData2 : TEXCOORD2;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				#ifdef FOG
					float fogFactor : TEXCOORD1;
				#endif
				float4 uv : TEXCOORD2;
				float4 particleCustomData : TEXCOORD3;
				float4 particleCustomData2 : TEXCOORD4;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DarkColor;
			float4 _EdgeColor;
			float4 _Edge1_ST;
			float4 _RotatePivotAngle;
			float4 _MainTex_ST;
			float _Sector;
			float _UseParticleData;
			float _Scale;
			float _SoftEdge;
			float _SoftEdgeOutside;
			float _SoftEdgeInside;
			float _Width;
			float _SingleEdge;
			float _FilletStr;
			int _UseAlphaChannel;
			CBUFFER_END

			#ifdef _UseTexON
			sampler2D _Edge2;
			sampler2D _Edge1;
			sampler2D _MainTex;
			#endif

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				//v.uv.xz是获取粒子的size属性 
				o.uv = v.uv;

				//x为宽度，y为缩放（非贴图的情况下），z为软边大小，w为扇形区域
				o.particleCustomData = v.particleCustomData;
				//x为外边虚化，y为内边虚化
				o.particleCustomData2 = v.particleCustomData2;
				o.color = v.color;


				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;

				#ifdef FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 WorldPosition = IN.worldPos;

				//扇形侧边贴图旋转
				float sectorControllSwitch = lerp( _Sector , IN.particleCustomData.w , _UseParticleData);
				float2 remapUV = IN.uv.xy * 2 - 1 ;
				//扇形
				float sectorRange = 1;
				#ifdef _SectorON
					float polar = atan2( remapUV.x , remapUV.y ) / TWO_PI + 0.5;
					sectorRange = step( ( 1.0 - sectorControllSwitch ) , polar);
				#endif

				#if defined(_UseTexON)
					float2 uv_Edge1 = IN.uv.xy * _Edge1_ST.xy + _Edge1_ST.zw;
					float2 invertUV_Edge1 = ( 1.0 - uv_Edge1 );
					float2 rotatePivot = (float2(_RotatePivotAngle.x , _RotatePivotAngle.y));
					float cos189 = cos( radians( ( sectorControllSwitch * 360.0 ) ) );
					float sin189 = sin( radians( ( sectorControllSwitch * 360.0 ) ) );
					float2 rotator = mul( invertUV_Edge1 - rotatePivot , float2x2( cos189 , -sin189 , sin189 , cos189 )) + rotatePivot;
					float4 EdgeCol = tex2D( _Edge1, rotator );
					float4 EdgeCol2 = tex2D( _Edge2, invertUV_Edge1 );
					//开关自定义纹理
					float3 sideTexOut = saturate(EdgeCol + EdgeCol2).a;
					float2 uv_MainTex = IN.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float4 mainTex = tex2D( _MainTex, uv_MainTex );
					float3 Color = lerp(mainTex.r,mainTex.a,_UseAlphaChannel);
					float Alpha = saturate(Color.r * sectorRange + sideTexOut.r);
					Color = lerp( _DarkColor, _EdgeColor, saturate(Color + sideTexOut)).rgb;
					
				#else
					
					float widthValue = lerp( _Width , IN.particleCustomData.x , _UseParticleData);
					float2 scaleValue = lerp( _Scale , IN.particleCustomData.y , _UseParticleData);
					float softValue = lerp( _SoftEdge , IN.particleCustomData.z , _UseParticleData);
					float SoftEdgeOutside = lerp( _SoftEdgeOutside , IN.particleCustomData2.x , _UseParticleData);
					float SoftEdgeInside = lerp( _SoftEdgeInside , IN.particleCustomData2.y , _UseParticleData);

					float invertScaleValue = (1 - scaleValue.r);
					softValue *= invertScaleValue;
					float2 outsideSoft = scaleValue + softValue + SoftEdgeOutside;
					float insideValue = scaleValue.r + widthValue * invertScaleValue;
					float insideSoft = insideValue + softValue + SoftEdgeInside;

					//矩形
					float2 singgleEdge = 1 - abs( remapUV.y);
					float2 WHRatio = float2(IN.uv.z / IN.uv.w, 1 );//获取粒子长宽，矩形外框保持一致

					float2 squardUV = lerp( (1 - abs( remapUV)) * WHRatio, singgleEdge, _SingleEdge);
					float2 squardOutsideUV = smoothstep( outsideSoft + 0.001, scaleValue, squardUV);
					float squardBase = saturate(dot(squardOutsideUV,squardOutsideUV));
					float squardOutside = 1 - squardBase;

					float2 squardInsideUV = smoothstep(insideValue, insideSoft + 0.001, squardUV);
					float squardInside = saturate(squardInsideUV.x * squardInsideUV.y);

					float colFinal = squardOutside;
					float alphaOut = squardInside;

					//圆形
					#ifdef _CircleON
						float2 circleUV = pow( abs( remapUV ), _FilletStr );//圆角矩形
						float circleBase = 1 - dot( circleUV  , circleUV );
						float circleOutside = smoothstep( scaleValue.r , outsideSoft.r , circleBase);
						float circleInside = smoothstep( insideValue, insideSoft, circleBase);
						colFinal = circleOutside;
						alphaOut = circleInside;
					#endif

					float3 Color = lerp(_EdgeColor, _DarkColor, alphaOut).rgb;
					float Alpha = lerp(colFinal * _EdgeColor.a,_DarkColor.a,alphaOut);
					Alpha *= sectorRange;

				#endif

				Color *= IN.color.rgb;
				Alpha *= IN.color.a;

				
				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif

				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}		
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	
}
