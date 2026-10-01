Shader "Custom/Cool/EnergyShield"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0.2, 0.8, 1, 1)
        _GridScale ("Grid Scale", Float) = 8
        _GridWidth ("Grid Width", Range(0.001, 0.2)) = 0.03
        _PulseSpeed ("Pulse Speed", Float) = 3
        _RimPower ("Rim Power", Range(0.5, 8)) = 2
        _Alpha ("Alpha", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _GridScale;
            float _GridWidth;
            float _PulseSpeed;
            float _RimPower;
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

            // 六边形网格
            float hexGrid(float2 p)
            {
                float2 s = float2(1.0, 1.732);
                float2 h = s * 0.5;
                float2 a = frac(p / s) - 0.5;
                float2 b = frac((p - h) / s) - 0.5;
                float2 gv = dot(a, a) < dot(b, b) ? a : b;
                float2 id = dot(a, a) < dot(b, b) ? floor(p / s) : floor((p - h) / s);
                float2 uv = gv;
                float d = abs(max(abs(uv.x) * 1.5 + uv.y, abs(uv.y) * 2.0 - 1.0));
                return d;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                // 用世界坐标做网格
                float2 uv = IN.positionWS.xz + IN.positionWS.xy * 0.5;
                float hex = hexGrid(uv * _GridScale);
                float grid = smoothstep(0.0, _GridWidth, 1.0 - hex);

                // 脉冲
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = smoothstep(0.3, 1.0, pulse);

                // 菲涅尔
                float fres = pow(1.0 - saturate(dot(N, V)), _RimPower);

                float3 col = _Color.rgb * (grid * pulse * 1.5 + fres * 2.0);
                float alpha = saturate(_Alpha + fres * 0.5 + grid * 0.3) * _Color.a;
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}