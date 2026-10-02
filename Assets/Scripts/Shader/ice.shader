Shader "Custom/Blocks/Ice"
{
    Properties
    {
        [MainColor] _Color ("Ice Color", Color) = (0.38, 0.82, 1, 0.58)
        _FresnelColor ("Ice Edge", Color) = (0.75, 0.95, 1, 1)
        _FresnelPower ("Edge Power", Range(0.5, 8)) = 2.2
        _Opacity ("Opacity", Range(0.1, 1)) = 0.58
        _SparkleScale ("Sparkle Scale", Float) = 9
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
            float4 _Color; float4 _FresnelColor; float _FresnelPower; float _Opacity; float _SparkleScale;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };
            float hash(float3 p){ return frac(sin(dot(p,float3(12.7,46.2,21.9)))*3271.3); }
            Varyings vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionWS=TransformObjectToWorld(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.viewDirWS=GetWorldSpaceViewDir(o.positionWS); return o; }
            half4 frag(Varyings i):SV_Target
            {
                float3 N=normalize(i.normalWS); float3 V=normalize(i.viewDirWS); float fres=pow(1-saturate(dot(N,V)),_FresnelPower);
                float sparkle=step(0.94,hash(floor(i.positionWS*_SparkleScale))); Light l=GetMainLight(); float lit=0.5+0.5*saturate(dot(N,normalize(l.direction)));
                float3 col=_Color.rgb*lit*l.color+_FresnelColor.rgb*(fres*1.1+sparkle*0.8); return half4(col,saturate(_Opacity+fres*0.3)*_Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
