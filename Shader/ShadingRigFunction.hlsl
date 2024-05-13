#ifndef SHADINGRIGFUNCTION_HLSL
#define SHADINGRIGFUNCTION_HLSL

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


float4 _p0; // pivot

struct ShadingRigFinalData
{
    float4 rigPostion;
    float4 info0; // Radius, Anisotropy, Sharpness, Degrees
    float4 info1; // Kw, Bend, Bulge, Ks
    float4 info2; // R, NormalWeighting, Intensity
};

StructuredBuffer<ShadingRigFinalData> _ShadingRigDatas;
int _ShadingRigDatasCount;

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
float distanceAttenuationTerm(float3 worldPos,float3 lightPos,float ks,float R)
{
    return smoothstep(0,1,(1/ks) * (R - length(worldPos - lightPos)));
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
float IntensityDistributionTerm(float2 texcoord,float anisotropy,float sharpness,float degrees,float kw,float bulge,float bend)
{

    float e = 2.71828f;
    float2 w = float2(bulge, bend);

    float angle = degrees / 57.295799f;
    float2x2 rotationMatrix = RotationMatrix(angle);
    float2 uv = mul(rotationMatrix, texcoord);
    
    float distortAngle = kw * dot(w, uv);
    // 在这里应用mask项
    uv = w + mul(RotationMatrix(distortAngle), uv - w);

    float x = uv.x;
    float y = uv.y;

    float lambda = 1 - anisotropy;
    float index_x = lambda * x * x;
    float index_y = (1/lambda) * pow(abs(y), 2 - sharpness);
    return pow(e, (-index_x - index_y)) * maskTerm(distortAngle);
}

//NormalWeightingTerm
// γ = N()
float3 NormalWeightingTerm(float3 posWS,float3 posCenter,float3 normalWS,float normalWeighting)
{
    //目的是球形映射。
    float3 normalizePosOffset = normalize(posWS - posCenter);
    return normalize(lerp(normalWS,normalizePosOffset,normalWeighting));
}

float3 CalculateSingleShadingRig(float3 positionWS,float3 normalWS,
    float3 rigPostion,
    float radius,float anisotropy,float sharpness,float degrees,
    float kw,float bend,float bulge,float ks,
    float R,float normalWeighting,float intensity)
{
    //自己组建rig光源空间的三个基向量
    float3 up = float3(0,1,0); //temp y
    float3 lz = normalize(rigPostion - _p0);
    float3 lx = cross(lz,normalize(cross(lz,up)));
    float3 ly = cross(lx,lz);
    lx = normalize(lx);
    ly = -normalize(ly);

    //像素到rig光源的单位向量
    float3 v = normalize(positionWS - _p0);
    //世界空间转rig光源空间
    float3 vl = float3(dot(v,lx),dot(v,ly),dot(v,lz));
    //Lit-Sphere Extension
    float theta = atan2(vl.y,vl.x);
    float cosTheta = cos(theta);
    float sinTheta = sin(theta);
    float r = acos(vl.z);
    float2 uv = float2(cosTheta,sinTheta) * (r / PI) * radius;

    float3 res;
    res = IntensityDistributionTerm(uv,anisotropy,sharpness,degrees,kw,bulge,bend);
    float disAttTerm = distanceAttenuationTerm(positionWS,rigPostion,ks,R);
    res = res * disAttTerm;
    float3 normalWeightingTermFactor = NormalWeightingTerm(positionWS,_p0,normalWS,normalWeighting);
    float normalWeightingTerm = dot(normalWeightingTermFactor,v);
    res = res * normalWeightingTerm;
    res = res * intensity;
    return res;
}

float3 CalculateAllShadingRig(float3 positionWS,float3 normalWS)
{
    float3 res = float3(0,0,0);
    for(int i = 0; i < _ShadingRigDatasCount; i++)
    {
        ShadingRigFinalData data = _ShadingRigDatas[i];
        res += CalculateSingleShadingRig(positionWS,normalWS,
            data.rigPostion,
            data.info0.x,data.info0.y,data.info0.z,data.info0.w,
            data.info1.x,data.info1.y,data.info1.z,data.info1.w,
            data.info2.x,data.info2.y,data.info2.z);
    }
    return res;
}


#endif