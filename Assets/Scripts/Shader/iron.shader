Shader "Custom/Blocks/Iron"
{
    Properties
    {
        [MainColor] _Color ("Iron Color", Color) = (0.25, 0.29, 0.34, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.92
        _Smoothness ("Smoothness", Range(0, 1)) = 0.72
        _WearScale ("Surface Variation", Float) = 6
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
            float4 _Color; float _Metallic; float _Smoothness; float _WearScale;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };
            float hash(float3 p){ return frac(sin(dot(p,float3(19.3,7.1,43.8)))*9157.1); }
            Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionWS=TransformObjectToWorld(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.viewDirWS=GetWorldSpaceViewDir(o.positionWS); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float wear=hash(floor(i.positionWS*_WearScale)); float3 baseCol=_Color.rgb*(0.9+wear*0.12); float3 N=normalize(i.normalWS); float3 V=normalize(i.viewDirWS); Light l=GetMainLight();
                float3 L=normalize(l.direction); float3 H=normalize(L+V); float ndl=saturate(dot(N,L)); float spec=pow(saturate(dot(N,H)),lerp(16,128,_Smoothness))*lerp(0.35,1.0,_Metallic);
                return half4(baseCol*(0.25+ndl*0.75)*l.color+spec, _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
