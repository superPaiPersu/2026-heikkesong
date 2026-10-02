Shader "Custom/Blocks/Wood"
{
    Properties
    {
        [MainColor] _Color ("Wood Color", Color) = (0.46, 0.20, 0.07, 1)
        _GrainScale ("Grain Scale", Float) = 5
        _GrainContrast ("Grain Contrast", Range(0, 1)) = 0.35
        _Smoothness ("Smoothness", Range(0, 1)) = 0.28
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
            float4 _Color; float _GrainScale; float _GrainContrast; float _Smoothness;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };
            Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionWS=TransformObjectToWorld(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.viewDirWS=GetWorldSpaceViewDir(o.positionWS); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 p=i.positionWS*_GrainScale; float grain=sin(p.x*2.7+p.y*0.8+sin(p.z*1.9)*1.8)*0.5+0.5;
                float3 baseCol=_Color.rgb*(1.0+(grain-0.5)*_GrainContrast); float3 n=normalize(i.normalWS); float3 v=normalize(i.viewDirWS);
                Light l=GetMainLight(); float ndl=saturate(dot(n,normalize(l.direction))); float3 h=normalize(normalize(l.direction)+v);
                float spec=pow(saturate(dot(n,h)),lerp(12,64,_Smoothness))*0.22;
                return half4(baseCol*(0.35+ndl*0.75)*l.color+spec, _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
