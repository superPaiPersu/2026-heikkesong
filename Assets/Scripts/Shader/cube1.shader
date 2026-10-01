Shader "Custom/JellyURP"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1, 0.45, 0.65, 0.85)

        _RimColor ("Rim Color", Color) = (1, 0.9, 0.95, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.5

        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(1, 256)) = 64
        _SpecIntensity ("Spec Intensity", Range(0, 2)) = 0.6

        _Steps ("Light Steps", Range(1, 8)) = 3

        _WobbleSpeed ("Wobble Speed", Float) = 3
        _WobbleAmount ("Wobble Amount", Range(0, 0.5)) = 0.03
        _WobbleScale ("Wobble Scale", Float) = 5

        _OutlineColor ("Outline Color", Color) = (0.1, 0.05, 0.12, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.03
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Pass 1: 外轮廓 (Inverted Hull)
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float4 _SpecularColor;
            float _Gloss;
            float _SpecIntensity;
            float _Steps;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _WobbleScale;
            float4 _OutlineColor;
            float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float wobble = sin(_Time.y * _WobbleSpeed
                                   + positionWS.x * _WobbleScale
                                   + positionWS.y * 3.0) * _WobbleAmount;

                IN.positionOS.xyz += IN.normalOS * (wobble + _OutlineWidth);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Pass 2: 主体
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float4 _SpecularColor;
            float _Gloss;
            float _SpecIntensity;
            float _Steps;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _WobbleScale;
            float4 _OutlineColor;
            float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float wobble = sin(_Time.y * _WobbleSpeed
                                   + positionWS.x * _WobbleScale
                                   + positionWS.y * 3.0) * _WobbleAmount;
                IN.positionOS.xyz += IN.normalOS * wobble;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(OUT.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                // 卡通漫反射
                float NdotL = dot(N, L) * 0.5 + 0.5;
                float toon = floor(NdotL * _Steps) / _Steps;
                float3 diffuse = _Color.rgb * mainLight.color * toon;

                // 硬边高光
                float3 H = normalize(L + V);
                float NdotH = saturate(dot(N, H));
                float spec = pow(NdotH, _Gloss);
                spec = smoothstep(0.4, 0.42, spec);
                float3 specular = _SpecularColor.rgb * spec * _SpecIntensity;

                // 菲涅尔边缘光
                float fresnel = pow(1.0 - saturate(dot(N, V)), _RimPower);
                float3 rimColor = _RimColor.rgb * fresnel * _RimIntensity;

                float3 color = diffuse + specular + rimColor;

                // 透明度由 material.color.a 控制，边缘稍加一点
                float alpha = saturate(_Color.a + fresnel * 0.3);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}