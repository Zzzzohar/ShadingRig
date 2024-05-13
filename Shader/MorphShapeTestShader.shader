Shader "SmallGenius/MorphShapeTestShader"
{
    Properties
    {
        _BaseColor("Base Color",color) = (1,1,1,1)
        _BaseMap("BaseMap", 2D) = "white" {}
        _Threshold("Threshold", Range(0, 1)) = 0.5
        _Radius("Radius",Range(0,100)) = 50
        _Anisotropy("Anisotropy",Range(0,1)) = 0.5
        _Sharpness("Sharpness",Range(0,1)) = 0.5
        _Degrees("Rotation",Range(0,360)) = 0
        _Bulge("Bulge",Range(0,1)) = 0.5
        _Bend("Bend",Range(0,1)) = 0.5
        _Kw("Kw",Range(0,100)) = 10
        _Ks("Ks",Range(0,100)) = 0.05
        _R("R",Range(0,100)) = 10
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
            float4 _p1;
            float4 _p0;
            float _Radius;
            float _Anisotropy;
            float _Sharpness;
            float _Degrees;
            float _Kw;
            float _Bend;
            float _Bulge;
            float _Ks;
            float _R;
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

            float2x2 RotationMatrix(float angle)
            {
                float c = cos(angle);
                float s = sin(angle);
                return float2x2(c,-s,
                                s,c);
            }

            //当Bend，Bulge调太高的时候，会参数bias，需要使用mask来减少这个bias
            float maskTerm(float theta)
            {
              return (theta > -1.5708 && theta < 1.5708) ? 1.0 : 0.0;
            }

            //distance attenuation term
            // ω = f_s(1/k_s(R-||P_ws-P_l||))
            // f_s = smoothstep
            float distanceAttenuationTerm(float3 worldPos,float3 lightPos)
            {
                return smoothstep(0,1,(1/_Ks) * (_R - length(worldPos - lightPos)));
            }
            
            // λ = 1 - anisotropy
            // s = sharpness
            // x,y = uv
            // I(x,y) = e^(-λx^2 - 1/λ|y|^(2-s))

            // w := (w_x,w_y)^T = (Bulge,Bend)^T
            // w_x = Bulge
            // w_y = bend,
            // u = uv，R(θ_r)是一个旋转矩阵，然后整体就是旋转uv坐标
            // θ_w(u) = k_w w·(R(θ_r)u)
            // u' = (u_w,v_w) = w + R(θ_w(u))(u-w) // 就是扭曲之后的uv
            float IntensityDistributionTerm(float2 uv,float anisotropy,float sharpness)
            {
                float e = 2.7182818284590452353602874713527;
                float k_w = _Kw;
                float2 w = float2(_Bulge,_Bend);
                
                // Rotate the uv
                float angle = _Degrees * PI / 180;
                float2x2 rotMatrix = RotationMatrix(angle);
                uv = mul(rotMatrix,uv);
                
                float distortAngle = k_w * dot(w,uv);
                float mask = maskTerm(distortAngle);
                uv = w + mul(RotationMatrix(distortAngle),uv - w);
                
                float x = uv.x;
                float y = uv.y;
                anisotropy = min(anisotropy,0.9999999);
                float lambda = 1 - anisotropy;
                float s = sharpness;

                float termOne = -lambda * x * x;
                float termTwo = -1/lambda * pow(abs(y),2-s);

                return pow(e,termOne + termTwo) * mask;
                
            }

            
            
            half4 frag(Varyings i) : SV_Target
            {
                half4 c;
                Light light = GetMainLight();
                float2 uv = i.uv * _Radius - _Radius * 0.5;
                float3 res;
                // Add Anisotropy and Sharpness
                //Clamp Anisotropy [0,1)
                
                res = IntensityDistributionTerm(uv,_Anisotropy,_Sharpness);
                
                    
                return float4( res,1);
            }
            ENDHLSL
        }
    }
}