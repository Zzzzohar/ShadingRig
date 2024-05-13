Shader "SmallGenius/ShadingRigTestShader"
{
    Properties
    {
        _BaseColor("Base Color",color) = (1,1,1,1)
        _BaseMap("BaseMap", 2D) = "white" {}
        _Threshold("Threshold", Range(0, 1)) = 0.5

    }
 
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
 
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "ShadingRigFunction.hlsl"
 

            #define TWO_PI  6.28318530718
            #define PI      3.14159265359
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float3 normal           : NORMAL;
            };
 
            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float3 normalWS         : TEXCOORD1;
                float3 positionWS       : TEXCOORD2;
            };
 
                    CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            half _Threshold;
            CBUFFER_END
            TEXTURE2D (_BaseMap);SAMPLER(sampler_BaseMap);
            
 
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
 
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

 
            half4 frag(Varyings i) : SV_Target
            {
                half4 c;
                Light light = GetMainLight();

                float3 res = CalculateAllShadingRig(i.positionWS,i.normalWS);

                c.rgb = saturate(dot(i.normalWS,light.direction)) + res;
                c.rgb = step(_Threshold,c.rgb);
                
                
                return float4( c.rgb,1);
            }
            ENDHLSL
        }
    }
}