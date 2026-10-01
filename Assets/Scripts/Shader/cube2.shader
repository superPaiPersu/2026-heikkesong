Shader "Custom/Cool/Hologram"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0.2, 0.9, 1, 1)
        _ScanSpeed ("Scan Speed", Float) = 2
        _ScanDensity ("Scan Density", Float) = 40
        _RimPower ("Rim Power", Range(0.5, 8)) = 2
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 2
        _Alpha ("Alpha", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _ScanSpeed;
            float _ScanDensity;
            float _RimPower;
            float _RimIntensity;
            float _Alpha;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
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

                // 扫描线
                float scan = sin((IN.positionWS.y + _Time.y * _ScanSpeed) * _ScanDensity);
                scan = smoothstep(0.5, 1.0, scan) * 0.8 + 0.2;

                // 菲涅尔
                float fres = pow(1.0 - saturate(dot(N, V)), _RimPower);
                float3 col = _Color.rgb * (scan + fres * _RimIntensity);

                float alpha = saturate(_Alpha + fres * 0.5) * _Color.a;
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}