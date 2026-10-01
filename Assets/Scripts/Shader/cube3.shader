Shader "Custom/Cool/Dissolve"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0.1, 0.6, 1, 1)
        _EdgeColor ("Edge Color", Color) = (1, 0.6, 0.1, 1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.3
        _EdgeWidth ("Edge Width", Range(0.001, 0.2)) = 0.05
        _NoiseScale ("Noise Scale", Float) = 5
        _NoiseSpeed ("Noise Speed", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _EdgeColor;
            float _DissolveAmount;
            float _EdgeWidth;
            float _NoiseScale;
            float _NoiseSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            // 简化噪声
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                         lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                         lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);

                // 三维噪声
                float n = noise(IN.positionWS * _NoiseScale + _Time.y * _NoiseSpeed);

                // 溶解裁剪
                clip(n - _DissolveAmount);

                // 边缘发光
                float edge = smoothstep(_DissolveAmount, _DissolveAmount + _EdgeWidth, n);
                float edgeGlow = 1.0 - edge;

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(N, normalize(mainLight.direction)));
                float3 diffuse = _Color.rgb * mainLight.color * (NdotL * 0.8 + 0.2);

                float3 col = lerp(_EdgeColor.rgb * 3.0, diffuse, edge);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}