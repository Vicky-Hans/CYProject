Shader "DH/Effects/DistortMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0.001, 1)) = 0.01
        _TimeStrength("Time Strength", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
			Name "DistortMask"
            Tags { "LightMode"="DistortMask" }
            
            Cull Back
            ZWrite Off
            Blend Off
            
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varings
            {
                float4 positionHC : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_ST;
            half _Strength;
            float _TimeStrength;
            
            Varings vert (Attributes input)
            {
                Varings o = (Varings)0;
                o.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return o;
            }
            
            float4 frag (Varings input): SV_Target
            {
                float4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float2 noiseUV = input.uv + frac(_Time.x * _TimeStrength);
                float noise = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, noiseUV).r;
                
				mask.r = noise;
                mask.b = _Strength;
                
                return mask;
            }
            
            ENDHLSL
        }
    }
}