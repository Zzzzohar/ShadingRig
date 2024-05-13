using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SmallGenius.ShadingRig
{
    [ExecuteAlways]
    public class ShadingRigManager : MonoBehaviour
    {
        //所有的rig data
        public ShadingRigData shadingRigData;
        public bool isDebug = false;
        public bool isAnimation = false;
        public Material mat;
        //创建一个用于传递strueturedbuffer的buffer
        private ComputeBuffer structuredBuffer;

        
        public List<ShadingRigFinalData> shadingRigFinalDatas = new List<ShadingRigFinalData>();

        private void OnEnable()
        {
            if (mat == null)
                mat = this.GetComponent<Renderer>().sharedMaterial;
            
            if (shadingRigData.pointOfPivot == null)
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer == null)
                {
                    Debug.LogError("当前物体没有Renderer组件，无法添加Pivot点，请先添加Renderer组件。");
                    return;
                }
                GameObject pointOfPivot = new GameObject("PointOfPivot");
                pointOfPivot.transform.position = renderer.bounds.center;
                pointOfPivot.transform.SetParent(transform);
                shadingRigData.pointOfPivot = pointOfPivot;
                var iconContent = EditorGUIUtility.IconContent("sv_label_1");
                var texture = iconContent.image as Texture2D;
                typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic).Invoke(
                    null, new object[]{ pointOfPivot, texture});
            }
            //Check shading rig
            CheckShadingRigData();
            ShadingRigsDataToManager();
        }

        
        public void CheckShadingRigData()
        {
            var shadingRigs = GetComponentsInChildren<ShadingRig>();
            if (shadingRigs.Length != shadingRigData.shadingRigs.Count && shadingRigs.Length != 0)
            {
                shadingRigData.shadingRigs.Clear();
                foreach (ShadingRig shadingRig in shadingRigs)
                {
                    shadingRigData.shadingRigs.Add(shadingRig);
                }
            }
        }
        public void ShadingRigsDataToManager()
        {
            shadingRigData.ShadingRigDatas.Clear();
            //将所有子类数据传递进去
            foreach (ShadingRig shadingRig in shadingRigData.shadingRigs)
            {
                shadingRigData.ShadingRigDatas.Add(new SingleShadingRigData(shadingRig.gameObject));
                // 获取ShadingRigDatas列表的最后一个元素
                SingleShadingRigData lastData = shadingRigData.ShadingRigDatas[shadingRigData.ShadingRigDatas.Count - 1];
                lastData.Anisotropy = shadingRig.Anisotropy;
                lastData.Bend = shadingRig.Bend;
                lastData.Bulge = shadingRig.Bulge;
                lastData.Degrees = shadingRig.Degrees;
                lastData.Intensity = shadingRig.Intensity;
                lastData.Ks = shadingRig.Ks;
                lastData.Kw = shadingRig.Kw;
                lastData.NormalWeighting = shadingRig.NormalWeighting;
                lastData.R = shadingRig.R;
                lastData.Radius = shadingRig.Radius;
                lastData.Sharpness = shadingRig.Sharpness;
                shadingRigData.ShadingRigDatas[shadingRigData.ShadingRigDatas.Count - 1] = lastData;
            }
        }
        public void BuildStructuredBuffer()
        {
            if (shadingRigData.ShadingRigDatas.Count == 0)
                return;
            if(isAnimation)
                ShadingRigsDataToManager();
            mat.SetVector("_p0",shadingRigData.pointOfPivot ? shadingRigData.pointOfPivot.transform.position : Vector3.zero);
            shadingRigFinalDatas.Clear();
            foreach (SingleShadingRigData singleShadingRigData in shadingRigData.ShadingRigDatas)
            {
                ShadingRigFinalData shadingRigFinalData = new ShadingRigFinalData();
                shadingRigFinalData.rigPostion = singleShadingRigData.Rig.transform.position;
                shadingRigFinalData.info0 = new Vector4(
                    singleShadingRigData.Radius,singleShadingRigData.Anisotropy,
                    singleShadingRigData.Sharpness,singleShadingRigData.Degrees);
                shadingRigFinalData.info1 = new Vector4(
                    singleShadingRigData.Kw,singleShadingRigData.Bend,
                    singleShadingRigData.Bulge,singleShadingRigData.Ks);
                shadingRigFinalData.info2 = new Vector4(
                    singleShadingRigData.R,singleShadingRigData.NormalWeighting,
                    singleShadingRigData.Intensity,0);
                shadingRigFinalDatas.Add(shadingRigFinalData);
            }

            if (structuredBuffer == null || shadingRigData.ShadingRigDatas.Count == shadingRigFinalDatas.Count)
            {
                structuredBuffer = new ComputeBuffer(shadingRigFinalDatas.Count, 16 * 4);
            }
            
            structuredBuffer.SetData(shadingRigFinalDatas);
            mat.SetBuffer("_ShadingRigDatas",structuredBuffer);
            mat.SetInt("_ShadingRigDatasCount",shadingRigFinalDatas.Count);
            
        }
        private void Update()
        {
            CheckShadingRigData();
            //TODO 优化
            BuildStructuredBuffer();
        }
        
        private void OnDisable()
        {
            structuredBuffer?.Release();
        }
        //editor部分
        //Gizmos
        private void OnDrawGizmos()
        {
            // if (isDebug)
            // {
            //     foreach (SingleShadingRigData singleShadingRigData in shadingRigData.ShadingRigDatas)
            //     {
            //         Gizmos.color = Color.red;
            //         // 绘制线框模式的球体
            //         Gizmos.DrawWireSphere(singleShadingRigData.Rig.transform.position, 0.15f);
            //         
            //     }
            // }
        }
    }

}
