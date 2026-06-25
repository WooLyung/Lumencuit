Shader "Lumencuit/CircuitElementShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _Normal("Normal", 2D) = "white" {}

        _Progress("Progress", Range(0, 1)) = 0
        _Signal("Signal", Int) = 0
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
                half _Progress;
            CBUFFER_END

            float3 scalePositionOS(float3 positionOS, half progress)
            {
                half t = saturate(progress);
                half eased = t * t * (3.0h - 2.0h * t);
                half zScale = lerp(0.5h, 1.0h, eased);
                half pivotZ = 0.2h;
                positionOS.z = pivotZ + (positionOS.z - pivotZ) * zScale;
                return positionOS;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 positionOS = scalePositionOS(IN.positionOS, _Progress);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                OUT.positionHCS = positionInputs.positionCS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return 0;
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
                float4 screenPos : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            TEXTURE2D(_Normal);
            SAMPLER(sampler_Normal);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half _Progress;
                int _Signal;
            CBUFFER_END

            float3 scalePositionOS(float3 positionOS, half progress)
            {
                half t = saturate(progress);
                half eased = t * t * (3.0h - 2.0h * t);
                half zScale = lerp(0.5h, 1.0h, eased);
                half pivotZ = 0.2h;
                positionOS.z = pivotZ + (positionOS.z - pivotZ) * zScale;
                return positionOS;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                 
                float3 positionOS = scalePositionOS(IN.positionOS, _Progress);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionOS = IN.positionOS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(positionInputs);
                OUT.screenPos = ComputeScreenPos(positionInputs.positionCS);

                return OUT;
            }

            half4 GetElementColor(Varyings IN)
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, IN.uv));

                half3 normalWS = normalize(IN.normalWS + 0.2h * (normal - half3(0.5h, 0.5h, 0.0h)));
                Light mainLight = GetMainLight(IN.shadowCoord);

                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = color.rgb * mainLight.color * ndotl * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(normalWS) * color.rgb;
                half3 finalColor = diffuse + ambient;

                return half4(finalColor, 1);
            }

            half3 SignalToColor(int signal)
            {
                if (signal == 0)
                    return half3(0.005h, 0.005h, 0.005h);
                half r = (signal & 1) != 0 ? 1.0h : 0.0h;
                half g = (signal & 2) != 0 ? 1.0h : 0.0h;
                half b = (signal & 4) != 0 ? 1.0h : 0.0h;
                return half3(r, g, b);
            }

            int CountSubSignals(int signal)
            {
                int count = 0;
                for (int signalValue = 0; signalValue < 8; signalValue++)
                    if ((signal & (1 << signalValue)) != 0) 
                        count++;
                return count;
            }

            int GetSubSignalByIndex(int signal, int index)
            {
                int current = 0;
                for (int signalValue = 0; signalValue < 8; signalValue++)
                {
                    if ((signal & (1 << signalValue)) != 0)
                    {
                        if (current == index)
                            return signalValue;

                        current++;
                    }
                }
                return 0;
            }

            half4 GetSignalColor(Varyings IN)
            {
                half3 color = 0;

                // Null Signal
                if (_Signal == 0)
                {
                    color = half3(0.03h, 0.03h, 0.03h);
                }
                else
                {
                    int count = CountSubSignals(_Signal);

                    // Single Signal
                    if (count <= 1)
                    {
                        int signalValue = GetSubSignalByIndex(_Signal, 0);
                        color = SignalToColor(signalValue);
                    }
                    else
                    {
                        float holdTime = 1.0;
                        float transitionTime = 0.5;
                        float segmentTime = holdTime + transitionTime;

                        int segmentIndex = (int)floor(_Time.y / segmentTime);

                        int indexA = segmentIndex % count;
                        int indexB = (indexA + 1) % count;

                        float localTime = _Time.y - floor(_Time.y / segmentTime) * segmentTime;

                        int signalA = GetSubSignalByIndex(_Signal, indexA);
                        int signalB = GetSubSignalByIndex(_Signal, indexB);

                        half3 colorA = SignalToColor(signalA);
                        half3 colorB = SignalToColor(signalB);

                        half blend = 0.0h;

                        if (localTime > holdTime)
                        {
                            float t = (localTime - holdTime) / transitionTime;
                            t = saturate(t);
                            blend = t * t * (3.0h - 2.0h * t);
                        }

                        color = lerp(colorA, colorB, blend);
                    }
                }

                half3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight(IN.shadowCoord);

                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = color.rgb * mainLight.color * ndotl * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(normalWS) * color.rgb;
                half3 finalColor = diffuse + ambient;

                return half4(finalColor, 1);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 elementColor = GetElementColor(IN);
                half4 signalColor = GetSignalColor(IN);

                half mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, IN.uv).r;
                half4 color = lerp(elementColor, signalColor, mask);

                return half4(lerp(half3(0, 0, 0), color.rgb, _Progress), _Progress);
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

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half _Progress;
            CBUFFER_END

            float3 scalePositionOS(float3 positionOS, half progress)
            {
                half t = saturate(progress);
                half eased = t * t * (3.0h - 2.0h * t);
                half zScale = lerp(0.5h, 1.0h, eased);
                half xyScale = lerp(0.0h, 1.0h, eased);

                half pivotZ = 0.2h;
                return float3(positionOS.xy * xyScale, pivotZ + (positionOS.z - pivotZ) * zScale);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 positionOS = scalePositionOS(IN.positionOS, _Progress);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(
                        positionInputs.positionWS,
                        normalInputs.normalWS,
                        _MainLightPosition.xyz
                    )
                );

                return OUT;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                return 0;
            }

            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}