Shader "DHGames/UI/GuideMask"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
 
 
	//-------------------add----------------------
		_Center("Center", vector) = (0, 0, 0, 0)
		_Silder ("_Silder", Range (0,1000)) = 1000 // sliders
		_IsCircle("_IsCircle", Int) = 0

    //-------------------add----------------------
	}
 
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
 
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]
 
		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
 
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
 
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
 
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			
			//-------------------add----------------------
            float _Silder;
            float2 _Center;
			int _IsCircle;
            //-------------------add----------------------
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
 
				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}
 
			sampler2D _MainTex;
 
			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				float2 uv = IN.texcoord;
				//开始裁剪
                //外部直接给坐标 宽 高 GPU计算比率
                float posX = (_Center.x + 640) / 1280;
                float posY = (_Center.y + 360) / 720;
                float2 pos = float2(posX, posY);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				//-------------------add----------------------
				if(_IsCircle != 0)
				{
					color.a*=(distance(IN.worldPosition.xy,_Center.xy) > _Silder);
				} else
				{
					 // 矩形的中心位置
	                float2 rectCenter = _Center.xy;

	                // 矩形的半宽和半高（根据滑动条的值进行调整）
	                float2 rectHalfSize = _Silder * 0.5 * 2;

	                // 计算矩形的边界
	                float2 rectMinx = rectCenter - rectHalfSize;
	                float2 rectMaxx = rectCenter + rectHalfSize;
					float2 rectMiny = rectCenter - rectHalfSize;
	                float2 rectMaxy = rectCenter + rectHalfSize;

	                // 判断点是否在矩形内
	                float2 pointPos = IN.worldPosition.xy;
	                bool isPointInside = pointPos.x > rectMinx.x && pointPos.x < rectMaxx.x &&
	                                     pointPos.y > rectMiny.y && pointPos.y < rectMaxy.y;
					color.a*=!isPointInside;
				}
               	
				// posX = posX * 1280 / 720;
				// pos = float2(posX, posY);
    //             float rid = _Center.z / 720 / 2;
    //             uv.x = uv.x * 1280 / 720;
    //             float2 nor = uv-pos;
    //             if (length(nor) < rid)
    //             color.a = 0;
               	color.rgb*= color.a;
               	//-------------------add----------------------
				return color;
			
			}
			
		ENDCG
		}
	}
}