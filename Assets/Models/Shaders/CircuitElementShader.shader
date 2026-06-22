Shader "Lumencuit/CircuitElementShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainTexture] _Mask("Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

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
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, IN.uv).r;
                half3 rgb = lerp(baseTex.rgb, _BaseColor.rgb, mask);

                half4 texColor = half4(rgb, baseTex.a);
                half3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight(IN.shadowCoord);

                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = texColor.rgb * mainLight.color * ndotl * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(normalWS) * texColor.rgb;
                half3 finalColor = diffuse + ambient;

                return half4(finalColor, texColor.a);
            }

            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}