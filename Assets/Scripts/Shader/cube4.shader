Shader "Custom/Cool/Iridescent"
{
    Properties
    {
        [MainColor] _Color ("Tint", Color) = (1, 1, 1, 1)
        _HueShift ("Hue Shift", Range(0, 1)) = 0
        _HueSpeed ("Hue Speed", Float) = 0.2
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 2
        _Saturation ("Saturation", Range(0, 1)) = 0.9
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
            float _HueShift;
            float _HueSpeed;
            float _FresnelPower;
            float _Saturation;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            // HSV 转 RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
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

                float fres = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // 色相随视角和位置变化
                float hue = frac(_HueShift + fres * 0.8 + _Time.y * _HueSpeed);
                float3 rainbow = hsv2rgb(float3(hue, _Saturation, 1.0));

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(N, normalize(mainLight.direction)));
                float3 diffuse = mainLight.color * (NdotL * 0.6 + 0.4);

                float3 col = rainbow * diffuse * _Color.rgb;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}