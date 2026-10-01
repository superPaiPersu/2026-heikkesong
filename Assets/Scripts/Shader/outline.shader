Shader "Custom/InnerEdgeOutline"
{
    Properties
    {
        [HDR] _Color ("Edge Color", Color) = (1, 0.9, 0.1, 1)
        _Intensity ("Intensity", Range(0, 10)) = 2
        _EdgeWidth ("Edge Width", Range(0.001, 0.2)) = 0.025
        _Softness ("Edge Softness", Range(0, 0.05)) = 0.003
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "InnerEdge"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity;
                float _EdgeWidth;
                float _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 faceUV = frac(input.uv);
                float2 edgeDistance = min(faceUV, 1.0 - faceUV);
                float distanceToEdge = min(edgeDistance.x, edgeDistance.y);

                // Derivatives keep thin lines smooth when the cube is small on screen.
                float softness = max(_Softness, fwidth(distanceToEdge));
                float edge = 1.0 - smoothstep(
                    _EdgeWidth - softness,
                    _EdgeWidth + softness,
                    distanceToEdge);

                clip(edge - 0.001);
                return half4(_Color.rgb * _Intensity, edge * _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
