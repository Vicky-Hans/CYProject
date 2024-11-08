Shader "DH/Character/Outline"
{
    Properties
    {
		_cutoff("Cutoff", Range(0,1)) = 0.5
		_AmbientCol("Ambient Color Strength", Range(0,1)) = 0
		[IntRange]_RefStencil("RefStencil",Range(1,255)) = 1
        _MainCol("Main Color", color) = (1,1,1,1)
		_backCol("Back Color", color) = (0,0,0,0)
        _MainTex ("Texture", 2D) = "white" {}
		_colRemap("Color Remap",2D) = "white" {}

		_specularCol("Specular Color", color) = (1,1,1,1)
		_specularSize("Specular Size", Range(0,1)) = 0.9
		_specularStr("Specular Strength", Range(0,1)) = 0.5

		[Space(10)]
		[Toggle(_EmissionON)]_EmissionON("Emision ON",float) = 0
		_emissionTex("Emission", 2D) = "white" {}
		_emissionCol("Emission Color", color) = (0,0,0,0)
		

        [Space(10)]
        [HDR]_sideCol("Side Color", color) = (1,1,1,1)
        _sideLightRange("Side Light Range", Range(0,1)) = 0.1
        _sideLightStr("Side Light Strength", Range(0,2)) = 1
        _siddLightOffsetX("Side Light Offset X", Range(-1,1)) = 0.5
        _siddLightOffsetY("Side Light Offset Y", Range(-1,1)) = 0

        [Space(10)]
        _OutlineCol("Outline Color", color) = (0,0,0,0)
        _OutlineWidth("Outline Width", Range(0,1)) = 0.1

		[Space(10)]
		_animaNoiseTex("Aniamtion Noise",2D) = "white" {}
		_vertexAnima("Vertex Animation",Range(0,1)) = 0
		_speed("Speed",Range(0,5)) = 1

		[Space(10)]
		_dissolveTex("Dissolve",2D) = "white" {}
		_dissolveStr("Dissolve Strength",Range(0,1)) = 0

		[Space(10)]
		[HDR]_blockCol("Block Color", Color) = (1,0,0,1)

        
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            Cull Back

			Stencil
			{
				Ref [_RefStencil]
                Comp Always
                Pass Replace
			}

			HLSLPROGRAM

			#pragma multi_compile_instancing

			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			//#pragma multi_compile_fragment _ _SHADOWS_SOFT
			//#pragma shader_feature _ _SAMPLE_GI
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

			#pragma shader_feature _EmissionON

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 shadowCoord : TEXCOORD1;
				#ifdef FOG
					float fogFactor : TEXCOORD2;
				#endif

                float3 normalWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
				float4 tangentWS : TEXCOORD5;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);

				float4 color : TEXCOORD7;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START(UnityPerMaterial)
            float4 _MainCol;
			float4 _backCol;
            float4 _MainTex_ST;
			//float4 _colRemap_ST;
            float4 _sideCol;
            float4 _OutlineCol;
            float _OutlineWidth;
            float _sideLightRange;
            float _siddLightOffsetX;
            float _siddLightOffsetY;
			float _specularSize;
			float _specularStr;
			float4 _specularCol;
			float _AmbientCol;
			float _cutoff;
			float _sideLightStr;
			float4 _emissionCol;
			float _vertexAnima;
			float _speed;
			CBUFFER_END
            TEXTURE2D (_MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D (_colRemap); SAMPLER(sampler_colRemap);
			#ifdef _EmissionON
			TEXTURE2D (_emissionTex); SAMPLER(sampler_emissionTex);
			#endif
			

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				v.normal = normalize(v.normal);
				o.normalWS = mul(GetObjectToWorldMatrix(),float4( v.normal.xyz , 0.0 )).xyz;
				o.normalWS = normalize(o.normalWS);
				o.tangentWS = normalize(mul(GetObjectToWorldMatrix(),v.tangent));
                o.uv = v.uv;
				o.color = v.color;
				float Time = fmod(_Time.y,1000);

				v.vertex.xyz += v.tangent * _vertexAnima * (sin(Time * _speed * v.color.a) * 0.5 + 0.5);

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;

				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );

				#ifdef FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				OUTPUT_LIGHTMAP_UV(v.uv2, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);

				o.clipPos = positionCS;

				return o;
			}


			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 WorldPosition = IN.worldPos;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float3 normalFinal = IN.normalWS;

				ShadowCoords = TransformWorldToShadowCoord( WorldPosition );

				float2 uv = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 mainTexCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);

                float LdotN = dot(normalFinal,_MainLightPosition.xyz);
                float HalfLambert = LdotN * 0.5 + 0.49;
				float2 lightUV = HalfLambert;

				float4 colRemapTex = SAMPLE_TEXTURE2D(_colRemap,sampler_colRemap,lightUV);
				colRemapTex = lerp(_backCol,colRemapTex,HalfLambert);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
                float fresnel = saturate(dot(normalFinal,normalize(viewDir + float3(_siddLightOffsetX,_siddLightOffsetY,0))));
                fresnel = saturate(1 - fresnel / _sideLightRange) * _sideLightStr;

				float3 halfVector = normalize(_MainLightPosition.xyz + viewDir);
				float HdotN = dot(halfVector,normalFinal);
				float specularGray = smoothstep(saturate(HdotN),_specularSize,0) * _specularStr;

				float4 baseCol = mainTexCol * _MainCol * colRemapTex * _MainLightColor;

				baseCol += fresnel * _sideCol + specularGray * _specularCol;
				
				float4 emissionTex = 1;
				#ifdef _EmissionON
				emissionTex = SAMPLE_TEXTURE2D(_emissionTex,sampler_emissionTex,uv);
				#endif
				baseCol += (emissionTex * _emissionCol);
				float3 Color = baseCol.rgb;

				float3 ambinetCol = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, normalFinal);
				Color = lerp(Color,Color + ambinetCol, _AmbientCol) * _MainLightColor.rgb * _MainLightColor.a;
				Color *= IN.color;

				float Alpha = mainTexCol.a;
				float AlphaClipThreshold = mainTexCol.a;
				float AlphaClipThresholdShadow = 0.5;
				clip(mainTexCol.a - _cutoff);


				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

        Pass
		{
			
			Name "Outline"
            Tags { "LightMode"="Outline" }
			
			Blend Off
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
            Cull Front

			Stencil
			{
				Ref [_RefStencil]
                Comp NotEqual
			}

			HLSLPROGRAM

			#pragma multi_compile_instancing


			#pragma shader_feature _ _SAMPLE_GI

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#ifdef FOG
					float fogFactor : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _OutlineCol;
            float _OutlineWidth;
			float _vertexAnima;
			float _speed;
			CBUFFER_END
			

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float Time = fmod(_Time.y,1000);
				v.vertex.xyz += v.tangent * _vertexAnima * (sin(Time * _speed * v.color.a) * 0.5 + 0.5);

                v.vertex.xyz += v.tangent.xyz * _OutlineWidth * 0.1;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#ifdef FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				o.clipPos = positionCS;

				return o;
			}


			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 Color = _OutlineCol.rgb * _MainLightColor.rgb * _MainLightColor.a;
				float Alpha = _OutlineCol.a;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;


				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}


		Pass
		{
			
			Name "Block"
            Tags { "LightMode"="Block" }
			
			Blend One One
			ZWrite On
			ZTest Greater
			ColorMask RGBA
            Cull Back

			Stencil
			{
				Ref [_RefStencil]
                Comp NotEqual
			}


			HLSLPROGRAM

			#pragma multi_compile_instancing


			#pragma shader_feature _ _SAMPLE_GI

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
                float4 tangent : TANGENT;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#ifdef FOG
					float fogFactor : TEXCOORD2;
				#endif
				float3 worldPos : TEXCOORD3;
				float3 normalWS : TEXCOORD4;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _OutlineCol;
            float _OutlineWidth;
			float _vertexAnima;
			float _speed;
			float4 _blockCol;
			CBUFFER_END
			

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				v.normal = normalize(v.normal);
				o.normalWS = mul(GetObjectToWorldMatrix(),float4( v.normal.xyz , 0.0 )).xyz;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#ifdef FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				o.clipPos = positionCS;

				return o;
			}


			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );

				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos);
                float fresnel = dot(IN.normalWS,viewDir );
                fresnel = saturate((0.6 - fresnel) * 2);

				float3 Color = _blockCol * fresnel + _blockCol * 0.3;
				float Alpha = 1;


				#ifdef FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
            Pass
            {
                
            	Name "ShadowCaster"
            	Tags { "LightMode"="ShadowCaster" }

            	ZWrite On
            	ZTest LEqual
            	AlphaToMask Off
            	ColorMask 0

            	HLSLPROGRAM
                
            	#pragma multi_compile_instancing

                
            	#pragma vertex vert
            	#pragma fragment frag

            	#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            	#define SHADERPASS SHADERPASS_SHADOWCASTER

            	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                

            	struct VertexInput
            	{
            		float4 vertex : POSITION;
            		float3 normal : NORMAL;
                    
            		UNITY_VERTEX_INPUT_INSTANCE_ID
            	};

            	struct VertexOutput
            	{
            		float4 clipPos : SV_POSITION;
            		float3 worldPos : TEXCOORD0;
            		float4 shadowCoord : TEXCOORD1;
                    
            		UNITY_VERTEX_INPUT_INSTANCE_ID
            	};

            	CBUFFER_START(UnityPerMaterial)

            	CBUFFER_END
                
            	float3 _LightDirection;
            	float3 _LightPosition;


            	VertexOutput vert ( VertexInput v )
            	{
            		VertexOutput o;
            		UNITY_SETUP_INSTANCE_ID(v);
            		UNITY_TRANSFER_INSTANCE_ID(v, o);

            		float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

            		o.worldPos = positionWS;

            		float3 normalWS = TransformObjectToWorldDir( v.normal );

            		#if _CASTING_PUNCTUAL_LIGHT_SHADOW
            			float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            		#else
            			float3 lightDirectionWS = _LightDirection;
            		#endif

            		float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

            		VertexPositionInputs vertexInput = (VertexPositionInputs)0;
            		vertexInput.positionWS = positionWS;
            		vertexInput.positionCS = clipPos;
            		o.shadowCoord = GetShadowCoord( vertexInput );

            		o.clipPos = clipPos;

            		return o;
            	}

            	half4 frag(VertexOutput IN  ) : SV_TARGET
            	{
            		UNITY_SETUP_INSTANCE_ID( IN );

            		float3 WorldPosition = IN.worldPos;

            		float4 ShadowCoords = float4( 0, 0, 0, 0 );

            		ShadowCoords = TransformWorldToShadowCoord( WorldPosition );

                    

            		float Alpha = 1;
            		float AlphaClipThreshold = 0.5;
            		float AlphaClipThresholdShadow = 0.5;


            		#ifdef LOD_FADE_CROSSFADE
            			LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
            		#endif
            		return 0;
            	}
            	ENDHLSL
            }
        
    }
	//Fallback "Diffuse"
}
