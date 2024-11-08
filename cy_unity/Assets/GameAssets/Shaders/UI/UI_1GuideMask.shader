Shader "DHGames/UI/UI_1GuideMask"
{
	Properties
	{
		[Enum(Off,0,On,1)]_ZWriteMode("ZWrite",float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("ZTest",Range(0,7)) = 4 
		_MainColor("MainColor", Color) = (0,0,0,0.8)
		
		_Size("Size", Range( 0 , 1080)) = 100
		_YScale("YScale", Range( 0 , 10)) = 1
		_Smooth("Smooth", Range( 0.01 , 0.99)) = 0.9
		
		_Center("Center", Vector) = (0,0,0,0)
		[Toggle]_isCircle("isCircle", Float) = 0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane"}
				
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ColorMask RGBA
			ZWrite [_ZWriteMode]
			ZTest [_Ztest]
			

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "UnityShaderVariables.cginc"

			#pragma target 2.5

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 color : COLOR;
				float4 screenPos : TEXCOORD0;
			};

			CBUFFER_START(UnityPerMaterial)
			uniform float4 _MainColor;
			uniform int _isCircle;
			uniform float _Size;
			uniform float _Smooth;
			uniform float4 _Center;
			uniform float _YScale;

			CBUFFER_END

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				
				o.color = v.color;
				
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{	
				float2 center = float2(_Center.x,_Center.y);
				float dist = distance(IN.clipPos.xy,center);
				
				float circleRamp = saturate(dist / _Size);
				float circleClamp = smoothstep( _Smooth , 1 , circleRamp);

				float2 posOffset = IN.clipPos.xy - _Center.xy;

				float WRamp = saturate(abs(posOffset.x * 2 - 1) / (_Size * 2));
				float WClamp = smoothstep(_Smooth,1,WRamp);
				float HRamp = saturate(abs(posOffset.y * 2 - 1) / (_Size * 2 * _YScale));
				float HClamp = smoothstep(_Smooth,1,HRamp);

				float rampBlend = lerp(WClamp,1,HClamp);
				
				float3 Color = ( IN.color * _MainColor ).rgb;
				//用插值的方式做圆形到方形的切换，减少使用判断
				float Alpha = lerp(rampBlend,circleClamp,_isCircle) * ( IN.color * _MainColor ).a;

				#ifdef UNITY_UI_ALPHACLIP
				clip (Alpha - 0.001);
				#endif

				half4 FinalCol = float4(Color,Alpha);
				return FinalCol;
			}
			ENDHLSL
		}

	
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
