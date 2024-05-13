using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShadingRigtest : MonoBehaviour
{
    //TODO
    //Editor Pane
    //Runtime Data
    //K帧插值器
    //Scriptable Object 数据存储准备
    //
    
    //--------------------------------------Test Property
    public GameObject p1;//rig点位置
    public GameObject p0;//质心，后续可以配置

    public Material mat;
    
    
    // Start is called before the first frame update
    void OnEnable()
    {
        if (!mat)
            mat = this.GetComponent<Renderer>().sharedMaterial;

    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_p1",p1.transform.position);
        Shader.SetGlobalVector("_p0",p0.transform.position);
        Shader.SetGlobalVector("_pc",this.GetComponent<Renderer>().bounds.center);
    }
}
