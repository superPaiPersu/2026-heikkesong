Shader "Custom/Blocks/Glass"
{
    Properties
    {
        [MainColor] _Color ("Glass Color", Color) = (0.35, 0.82, 1, 0.32)
        _FresnelColor ("Edge Color", Color) = (0.65, 0.95, 1, 1)
        _FresnelPower ("Edge Power", Range(0.5, 8)) = 3
        _Opacity ("Opacity", Range(0.05, 1)) = 0.32
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float4 _FresnelColor; float _FresnelPower; float _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };
            Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionWS=TransformObjectToWorld(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.viewDirWS=GetWorldSpaceViewDir(o.positionWS); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.normalWS); float3 v=normalize(i.viewDirWS); float fres=pow(1-saturate(dot(n,v)),_FresnelPower);
                Light l=GetMainLight(); float lit=0.55+0.45*saturate(dot(n,normalize(l.direction)));
                float3 col=_Color.rgb*lit*l.color+_FresnelColor.rgb*fres*1.25;
                return half4(col,saturate(_Opacity+fres*0.42)*_Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
