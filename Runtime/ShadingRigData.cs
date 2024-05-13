
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallGenius.ShadingRig
{
    [Serializable]
    public struct SingleShadingRigData
    {
        public GameObject Rig;
        public float Radius;
        public float Anisotropy;
        public float Sharpness;
        public float Degrees;
        public float Kw;
        public float Bend;
        public float Bulge;
        public float Ks;
        public float R;
        public float NormalWeighting;
        public float Intensity;
        
        //TODO 单个ShadingRig的数据结构，仅用于Editor帮助，后续都会转成structuredbuffer
        public SingleShadingRigData(GameObject rigObj)
        {
            Rig = rigObj;
            Radius = 25.0f;
            Anisotropy = 0.0f;
            Sharpness = 0.0f;
            Degrees = 0.0f;
            Kw = 0.0f;
            Bend = 0.0f;
            Bulge = 0.0f;
            Ks = 25.0f;
            R = 5.0f;
            NormalWeighting = 1.0f;
            Intensity = 1.0f;
        }
    }

    [Serializable]
    public struct ShadingRigFinalData
    {
        public Vector4 rigPostion;
        public Vector4 info0; // Radius, Anisotropy, Sharpness, Degrees
        public Vector4 info1; // Kw, Bend, Bulge, Ks
        public Vector4 info2; // R, NormalWeighting, Intensity
            
    }
    
    [Serializable]
    public class ShadingRigData 
    {
        //论文的Pivot center 和 p1  我用的一个。。如果有问处理一下就行
        public GameObject pointOfPivot;//轴心点，所有人统一，初始就是中心点，后续可以自定义 TODO toggle
        //[HideInInspector]
        public List<ShadingRig> shadingRigs = new List<ShadingRig>();//存储每一个子类的数据
        
        public List<SingleShadingRigData> ShadingRigDatas = new List<SingleShadingRigData>();//用于manager显示的数据
    }
}