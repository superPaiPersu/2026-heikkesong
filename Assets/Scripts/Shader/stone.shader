Shader "Custom/Blocks/Stone"
{
    Properties
    {
        [MainColor] _Color ("Stone Color", Color) = (0.38, 0.41, 0.44, 1)
        _NoiseScale ("Rock Scale", Float) = 4
        _Roughness ("Roughness", Range(0, 1)) = 0.82
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
            float4 _Color; float _NoiseScale; float _Roughness;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };
            float hash(float3 p){ return frac(sin(dot(p,float3(17.13,31.71,11.37)))*43758.5); }
            Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionWS=TransformObjectToWorld(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.viewDirWS=GetWorldSpaceViewDir(o.positionWS); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 q=floor(i.positionWS*_NoiseScale); float n=hash(q)*0.58+hash(q+1.7)*0.27+hash(q+3.1)*0.15;
                float3 baseCol=_Color.rgb*(0.72+n*0.5); float3 N=normalize(i.normalWS); float3 V=normalize(i.viewDirWS); Light l=GetMainLight();
                float ndl=saturate(dot(N,normalize(l.direction))); float3 H=normalize(normalize(l.direction)+V); float spec=pow(saturate(dot(N,H)),lerp(8,30,1-_Roughness))*0.12*(1-_Roughness);
                return half4(baseCol*(0.32+ndl*0.78)*l.color+spec,_Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
