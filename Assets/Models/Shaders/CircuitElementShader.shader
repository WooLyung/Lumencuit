Shader "Lumencuit/CircuitElementShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}

        _Dissolve ("Dissolve", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeColor("Dissolve Edge Color", Color) = (1, 1, 1, 1)
        _DissolveEdgeWidth("Dissolve Edge Width", Range(0.001, 0.3)) = 0.08
        _DissolveEdgeIntensity("Dissolve Edge Intensity", Range(0, 5)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ZTest"

            ColorMask 0

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half _DissolveAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                half t = saturate(_DissolveAmount);
                half eased = t * t * (3.0h - 2.0h * t);
                float3 positionOS = lerp(IN.positionOS.xyz * 0.5f, IN.positionOS.xyz, eased);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                OUT.positionHCS = positionInputs.positionCS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 positionOS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);
            
            TEXTURE2D(_Dissolve);
            SAMPLER(sampler_Dissolve);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _DissolveAmount;
                half4 _DissolveEdgeColor;
                half _DissolveEdgeWidth;
                half _DissolveEdgeIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                half t = saturate(_DissolveAmount);
                half eased = t * t * (3.0h - 2.0h * t);
                float3 positionOS = lerp(IN.positionOS.xyz * 0.5f, IN.positionOS.xyz, eased);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionOS = IN.positionOS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 GetElementColor(Varyings IN)
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                half3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight(IN.shadowCoord);

                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = color.rgb * mainLight.color * ndotl * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(normalWS) * color.rgb;
                half3 finalColor = diffuse + ambient;

                return half4(finalColor, 1);
            }

            half4 GetSignalColor(Varyings IN)
            {
                half3 baseColor = _BaseColor.rgb;

                half3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight(IN.shadowCoord);

                half ndotl = saturate(dot(normalWS, mainLight.direction));

                half highlight = pow(ndotl, 3.0h) * 0.5h;

                half3 color = lerp(baseColor, half3(1.0h, 1.0h, 1.0h), highlight);

                return half4(color, 1);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 elementColor = GetElementColor(IN);
                half4 signalColor = GetSignalColor(IN);

                half mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, IN.uv).r;
                half4 color = lerp(elementColor, signalColor, mask);

                return half4(lerp(half3(0, 0, 0), color.rgb, _DissolveAmount), _DissolveAmount);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 positionOS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            TEXTURE2D(_Dissolve);
            SAMPLER(sampler_Dissolve);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half _DissolveAmount;
            CBUFFER_END

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;

                half t = saturate(_DissolveAmount);
                half eased = t * t * (3.0h - 2.0h * t);
                float3 positionOS = lerp(IN.positionOS.xyz * 0.5f, IN.positionOS.xyz, eased);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(
                        positionInputs.positionWS,
                        normalInputs.normalWS,
                        _MainLightPosition.xyz
                    )
                );

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionOS = IN.positionOS;

                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_TARGET
            {
                half dissolve = -IN.positionOS.z;
                clip(_DissolveAmount - dissolve);
                return 0;
            }

            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}