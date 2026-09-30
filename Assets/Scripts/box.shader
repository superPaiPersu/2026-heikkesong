Shader "Custom/JellyURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 0.45, 0.65, 1)
        _Alpha ("Alpha", Range(0, 1)) = 0.85

        _RimColor ("Rim Color", Color) = (1, 0.9, 0.95, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.5
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = 0.7

        _OutlineColor ("Outline Color", Color) = (0.1, 0.05, 0.12, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.03

        _SpecColor2 ("Specular Color", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(1, 256)) = 64
        _SpecIntensity ("Spec Intensity", Range(0, 2)) = 0.6

        _Steps ("Light Steps", Range(1, 8)) = 3

        _WobbleSpeed ("Wobble Speed", Float) = 3
        _WobbleAmount ("Wobble Amount", Range(0, 0.5)) = 0.03
        _WobbleScale ("Wobble Scale", Float) = 5
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
            float4 _OutlineColor;
            float _OutlineWidth;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _WobbleScale;
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
                float wobble = sin(_Time.y * _WobbleSpeed + positionWS.x * _WobbleScale + positionWS.y * 3.0) * _WobbleAmount;

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
            float4 _BaseColor;
            float _Alpha;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float _RimSharpness;
            float4 _SpecColor2;
            float _Gloss;
            float _SpecIntensity;
            float _Steps;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _WobbleScale;
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
                float wobble = sin(_Time.y * _WobbleSpeed + positionWS.x * _WobbleScale + positionWS.y * 3.0) * _WobbleAmount;
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
                float3 diffuse = _BaseColor.rgb * mainLight.color * toon;

                // 硬边高光
                float3 H = normalize(L + V);
                float NdotH = saturate(dot(N, H));
                float spec = pow(NdotH, _Gloss);
                spec = smoothstep(0.4, 0.42, spec);
                float3 specular = _SpecColor2.rgb * spec * _SpecIntensity;

                // 菲涅尔边缘
                float fresnel = pow(1.0 - saturate(dot(N, V)), _RimPower);
                float rimSharp = smoothstep(_RimSharpness, _RimSharpness + 0.1, fresnel);
                float rim = lerp(fresnel, rimSharp, 0.7);
                float3 rimColor = _RimColor.rgb * rim * _RimIntensity;

                float3 color = diffuse + specular + rimColor;

                float alpha = _Alpha + rim * 0.5;
                alpha = saturate(alpha);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}