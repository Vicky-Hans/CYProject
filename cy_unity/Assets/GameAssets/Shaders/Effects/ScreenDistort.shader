Shader "DH/Effect/ScreenDistort"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Pass
        {

			ZTest Always
			ZWrite Off
			Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskSoildColor);
            SAMPLER(sampler_MaskSoildColor);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
				float2 realUV = input.uv;
				
                //采样Mask图获得权重信息
                half4 factor = SAMPLE_TEXTURE2D(_MaskSoildColor, sampler_MaskSoildColor, realUV);
                float2 uv = realUV + factor.r * factor.g * factor.b;
				float4 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack off
}