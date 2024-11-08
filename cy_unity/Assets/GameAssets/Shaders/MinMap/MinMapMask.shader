Shader "DH/MinMapMask"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white"{}
        _MaskNoiseTex("Noise Texture", 2D) = "white"{}
        
        _FogColor("Fog Color", Color) = (1,1,1,1)
        
        [Header(Edge)]
        _EdgeSize("Edge Size", Range(1, 8)) = 2
        
        [Header(Inner Edge)]_InnerEdgeColor("Inner Edge Color", Color) = (1,1,1,1)
        _InnerHardEdgeFactor("Inner Hard Edge Factor", Range(0, 1)) = 0.5
        
        [Header(Outer Edge)]_OuterHardEdgeFactor("Outer Hard Edge Factor", Range(0, 0.99)) = 0.8
        _OuterHardEdgeNoiseFactor("Outer Hard Edge Noise", Range(0, 1)) = 0.5
        _OuterEdgeFactor("Outer Edge Factor", Range(1, 2)) =1.1
        _OuterEdgeSoftStrength("Outer Edge Soft Strength", Int) = 1
        _CorrodeColor("Corrode Color", Color) = (1,1,1,1)
        _OuterEdgeWidthFactor("Outer Edge Width Factor", Range(0, 0.5)) = 0.5
        
        _CorrodeColor1("Corrode Color1", Color) = (1,1,1,1)
        _CorrodeColor2("Corrode Color2", Color) = (1,1,1,1)
        _CorrodeColor3("Corrode Color3", Color) = (1,1,1,1)
        
        _HardTileColor("Hard Tile Color", Color) = (1,1,1,1)
        _HardTileColorFactor("Hard Tile Color Factor",Range(0, 1))=0.1
        _HardTileWidthFactor("Hard Tile Width Factor", Range(0, 0.5)) = 0.3
        
        _HardTileColor1("Hard Tile Color1", Color) = (1,1,1,1)
        _HardTileColor2("Hard Tile Color2", Color) = (1,1,1,1)
        _HardTileColor3("Hard Tile Color3", Color) = (1,1,1,1)
        
        [Header(Light Color)] _LightInnerColor("Light Inner Color", Color) = (1,1,1,1)
        _LightOuterColor("Light Outer Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "MaskCast"
            Tags { "LightMode" = "Universal2D" }
        
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex VertexFun
            #pragma fragment FragmentFun
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionHC : SV_POSITION;
            };
            
            Varyings VertexFun(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                
                return output;
            }
            
            half4 FragmentFun(Varyings input) : SV_Target
            {
                return half4(1,0,0,1);
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "EdgeDetectDownSample"
            Tags { "LightMode"="Universal2D" }
            
            ZTest Always
            ZWrite Off
            
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex VertexFun
            #pragma fragment FragmentFun
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            half _EdgeSize;
            
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHC : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 uv01: TEXCOORD1;
		        float4 uv23: TEXCOORD2;
            };
            
            Varyings VertexFun(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = TRANSFORM_TEX(input.uv, _MainTex);
                
                half2 halfTexelSize = _MainTex_TexelSize.xy * 0.5;
                half2 offset = half2(1 + _EdgeSize, 1 + _EdgeSize);
                output.uv01.xy = output.uv - halfTexelSize * offset;//top right
		        output.uv01.zw = output.uv + halfTexelSize * offset;//bottom left
		        output.uv23.xy = output.uv - float2(halfTexelSize.x, -halfTexelSize.y) * offset;//top left
		        output.uv23.zw = output.uv + float2(halfTexelSize.x, -halfTexelSize.y) * offset;//bottom right
                
                return output;
            }
            
            half4 FragmentFun(Varyings input) : SV_Target
            {
                half4 sum = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * 4;
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.zw);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.zw);
		        return sum * 0.125;
            }
            
            ENDHLSL
        }
        
        
        Pass
        {
            Name "EdgeDetectUpSample"
            Tags { "LightMode"="Universal2D" }
            
            ZTest Always
            ZWrite Off
            
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex VertexFun
            #pragma fragment FragmentFun
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            half _EdgeSize;
            
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHC : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 uv01: TEXCOORD1;
		        float4 uv23: TEXCOORD2;
		        float4 uv45: TEXCOORD3;
		        float4 uv67: TEXCOORD4;
            };
            
            Varyings VertexFun(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = TRANSFORM_TEX(input.uv, _MainTex);
                
                half2 halfTexelSize = _MainTex_TexelSize.xy * 0.5;
                half2 offset = half2(1 + _EdgeSize, 1 + _EdgeSize);
                
                output.uv01.xy = output.uv + half2(-halfTexelSize.x * 2, 0) * offset;
		        output.uv01.zw = output.uv + half2(-halfTexelSize.x, halfTexelSize.y) * offset;
		        output.uv23.xy = output.uv + half2(0, halfTexelSize.y * 2) * offset;
		        output.uv23.zw = output.uv + halfTexelSize * offset;
		        output.uv45.xy = output.uv + half2(halfTexelSize.x * 2, 0) * offset;
		        output.uv45.zw = output.uv + half2(halfTexelSize.x, -halfTexelSize.y) * offset;
		        output.uv67.xy = output.uv + half2(0, -halfTexelSize.y * 2) * offset;
		        output.uv67.zw = output.uv - halfTexelSize * offset;
                
                return output;
            }
            
            half4 FragmentFun(Varyings input) : SV_Target
            {
                half4 sum = 0;
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.zw) * 2;
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.zw) * 2;
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv45.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv45.zw) * 2;
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv67.xy);
		        sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv67.zw) * 2;
		
		        return sum * 0.0833;
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "MaskCombine"
            Tags { "LightMode"="Universal2D" }
            ZWrite Off
            ZTest Always
            
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex VertexFun
            #pragma fragment FragmentFun
            
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            half4 _FogColor;
            half4 _InnerEdgeColor;
            half4 _DH_MapMaskRT_TexelSize;
            half4 _CorrodeColor;
            half4 _CorrodeColor1;
            half4 _CorrodeColor2;
            half4 _CorrodeColor3;
            
            
            half4 _HardTileColor;
            half4 _HardTileColor1;
            half4 _HardTileColor2;
            half4 _HardTileColor3;
            half4 _MapRange;
            half _HardTileColorFactor;
            half _HardTileWidthFactor;
            half _OuterEdgeWidthFactor;
            
            half _EdgeSize;
            half _InnerHardEdgeFactor;
            half _OuterHardEdgeFactor;
            half _OuterHardEdgeNoiseFactor;
            half _OuterEdgeFactor;
            half _OuterEdgeSoftStrength;
            half _FogCutRadius;

            float4 _PlayerPos;
            float4 _LightFactor;
            half4 _LightInnerColor;
            half4 _LightOuterColor;
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_DH_MapMaskRT);
            SAMPLER(sampler_DH_MapMaskRT);
            
            TEXTURE2D(_DH_EdgeMaskRT);
            
            TEXTURE2D(_DH_InnerEdgeMaskRT);
            SAMPLER(sampler_DH_InnerEdgeMaskRT);
            
            TEXTURE2D(_MaskNoiseTex);
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };
            
            struct Varyings 
            {
                float4 positionHC : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 positionWS : TEXCOORD1;
            };
            
            Varyings VertexFun(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv.xy = TRANSFORM_TEX(input.uv, _MainTex);

                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    output.uv.y = 1 - output.uv.y;
                #endif
                
                return output;
            }
             
             half4 FragmentFun(Varyings input) : SV_Target
             {
                half3 worldPos = ComputeWorldSpacePosition(input.uv, 0, UNITY_MATRIX_I_VP);
                half2 mapLocalPos = worldPos.xy - _MapRange.xy;
                half2 uvOffset = mapLocalPos.xy/_MapRange.zw;
                half dist = distance(worldPos.xy, _PlayerPos.xy);

                 half innerLight = step(dist, _LightFactor.x);
                 half outerLight = step(dist, _LightFactor.y) * (1 - innerLight);
                 half fogCut = saturate((dist) / (_FogCutRadius));
                
                half4 edgeFactor = SAMPLE_TEXTURE2D(_DH_EdgeMaskRT, sampler_DH_MapMaskRT, input.uv);
                half4 tunnelCutout = SAMPLE_TEXTURE2D(_DH_MapMaskRT, sampler_DH_MapMaskRT, input.uv);
                half4 noise = SAMPLE_TEXTURE2D(_MaskNoiseTex, sampler_DH_MapMaskRT, uvOffset);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                 
                half innerEdgeFactor = SAMPLE_TEXTURE2D(_DH_InnerEdgeMaskRT, sampler_DH_InnerEdgeMaskRT, input.uv).r;
                //计算外边缘效果
                half outerHardEdgeFactor = step(lerp(innerEdgeFactor + (noise.r - _OuterEdgeWidthFactor) * _OuterHardEdgeNoiseFactor, innerEdgeFactor, step(0.9999, innerEdgeFactor)), _OuterHardEdgeFactor);
                half outerHardTileFactor = step(lerp(innerEdgeFactor + (noise.g - _HardTileWidthFactor) * _OuterHardEdgeNoiseFactor, innerEdgeFactor,  step(0.999, innerEdgeFactor)), _OuterHardEdgeFactor);
                 
                 half3 corrodeTempColor = lerp(lerp(lerp(lerp(_CorrodeColor.rgb, _CorrodeColor3.rgb, step(tunnelCutout.b, 1)),_CorrodeColor2.rgb, step(tunnelCutout.b, 0.75)), _CorrodeColor1.rgb, step(tunnelCutout.b, 0.5)), _CorrodeColor.rgb , step(tunnelCutout.b, 0.25));
                corrodeTempColor = lerp(lerp(corrodeTempColor, _LightInnerColor.rgb, innerLight), _LightOuterColor.rgb *_LightOuterColor.a, outerLight);
                 half3 hardEdgeColor = lerp(half3(0,0,0), corrodeTempColor.rgb, outerHardEdgeFactor);

                 half3 hardTileTempColor = lerp(lerp(lerp(lerp(_HardTileColor.rgb, _HardTileColor3.rgb, step(tunnelCutout.b, 1)),_HardTileColor2.rgb, step(tunnelCutout.b, 0.75)), _HardTileColor1.rgb, step(tunnelCutout.b, 0.5)), _HardTileColor.rgb , step(tunnelCutout.b, 0.25));
                hardTileTempColor = lerp(lerp(hardTileTempColor, _LightInnerColor.rgb, innerLight), _LightOuterColor.rgb *_LightOuterColor.a, outerLight);
                 hardEdgeColor = lerp(hardEdgeColor, lerp(half3(0,0,0), hardTileTempColor, outerHardTileFactor), step(_HardTileColorFactor, tunnelCutout.g));
                 

                half3 outerOriginalColor =  lerp(color.rgb, color.rgb * lerp(half3(1,1,1), _LightInnerColor.rgb *_LightInnerColor.a, innerLight), outerHardEdgeFactor);
                half outerEdgeFactor = step(_OuterEdgeFactor, edgeFactor.r);                
                half3 outerEdgeColor = lerp(lerp(outerOriginalColor.rgb, _FogColor.rgb, pow(lerp(0, edgeFactor.r, fogCut), _OuterEdgeSoftStrength)), color.rgb, outerEdgeFactor);
                outerEdgeColor += hardEdgeColor;
                
                //计算内边缘效果
                 color.rgb = lerp(color.rgb, color.rgb * (1 -_LightInnerColor.a) + _LightInnerColor.rgb * _LightInnerColor.a, innerLight);
                half3 innerEdgeColor =lerp(color.rgb, _InnerEdgeColor.rgb,  _InnerEdgeColor.a * innerEdgeFactor * step(_InnerHardEdgeFactor, innerEdgeFactor));

                //腐蚀效果
                

                color.rgb = lerp(innerEdgeColor, outerEdgeColor, tunnelCutout.r);
                return color;
             }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "MaxSample"
            Tags { "LightMode"="Universal2D" }
            
            ZTest Always
            ZWrite Off
            
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex VertexFun
            #pragma fragment FragmentFun
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            half _EdgeSize;
            
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHC : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 uv01: TEXCOORD1;
		        float4 uv23: TEXCOORD2;
            };
            
            Varyings VertexFun(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHC = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = TRANSFORM_TEX(input.uv, _MainTex);
                
                half2 halfTexelSize = _MainTex_TexelSize.xy * 0.5;
                half2 offset = half2(1 + _EdgeSize, 1 + _EdgeSize);
                output.uv01.xy = output.uv - halfTexelSize * offset;//top right
		        output.uv01.zw = output.uv + halfTexelSize * offset;//bottom left
		        output.uv23.xy = output.uv - float2(halfTexelSize.x, -halfTexelSize.y) * offset;//top left
		        output.uv23.zw = output.uv + float2(halfTexelSize.x, -halfTexelSize.y) * offset;//bottom right
                
                return output;
            }
            
            half4 FragmentFun(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv01.xy);
                half4 color2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,input.uv01.zw);
                half4 color3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.xy);
                half4 color4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv23.zw);
                color = min(color, min(color1, min(color2, min(color3, color4))));
		        return color;
            }
            
            ENDHLSL
        }
    }
}
