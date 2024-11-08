// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DHGames/UI/Silhouette"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Tex", 2D) = "white" {}
        _Tint("Color", Color) = (1,1,1,1)
        
        //MASK SUPPORT ADD
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		//MASK SUPPORT END
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True"}
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
           
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 1.3
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect; // 用于Mask2D的剪辑矩形
            fixed4 _Tint;
            
            struct a2v
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(a2v a)
            {
                v2f f;
                UNITY_SETUP_INSTANCE_ID(a);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(f);
                
                f.uv = TRANSFORM_TEX(a.texcoord, _MainTex);
                f.worldPosition = a.vertex;
                f.pos = UnityObjectToClipPos(f.worldPosition);
                #ifdef UNITY_HALF_TEXEL_OFFSET     
                f.pos.xy -= (_ScreenParams.zw - 1.0);
                #endif
                return f;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 colo = col * fixed4(0, 0, 0, 1) + fixed4(_Tint.rgb, 0);
                
                #ifdef UNITY_UI_CLIP_RECT
                colo.a *= UnityGet2DClipping(i.worldPosition.xy,_ClipRect);
                //clip(i.pos.xy - (_ClipRect.xy+_ClipRect.zw)*0.5);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(colo.a - 0.001);
                #endif
                return colo;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
