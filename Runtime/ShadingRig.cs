using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmallGenius.ShadingRig
{


    public class ShadingRig : MonoBehaviour
    {
        [Range(0,100f)]
        public float Radius = 25.0f;
        [Range(0,1f)]
        public float Anisotropy = 0.0f;
        [Range(0,1f)]
        public float Sharpness = 0.0f;
        [Range(0,360f)]
        public float Degrees = 0.0f;
        [Range(0,100f)]
        public float Kw = 0.0f;
        [Range(-10f,10f)]
        public float Bend = 0.0f;
        [Range(-10f,10f)]
        public float Bulge = 0.0f;
        [Range(0,100f)]
        public float Ks = 25.0f;
        [Range(0,100f)]
        public float R = 5.0f;
        [Range(0,10f)]
        public float NormalWeighting = 1.0f;
        [Range(-10.0f,100f)]
        public float Intensity = 1.0f;

    }
}