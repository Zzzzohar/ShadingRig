using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace SmallGenius.ShadingRig
{
    [CustomEditor(typeof(ShadingRigManager))]
    [CanEditMultipleObjects]
    public class ShadingRigManagerEditor : Editor
    {
       private ShadingRigManager _shadingRigManagerObj;
       private bool isEditing = false;
       
       private SerializedProperty shadingRigDataProperty;

       private void OnEnable()
       {
           _shadingRigManagerObj = (ShadingRigManager)target;
           
           shadingRigDataProperty = serializedObject.FindProperty("shadingRigData");
       }

       private void OnDisable()
       {
           
       }



       public override void OnInspectorGUI()
       {
           _shadingRigManagerObj = target as ShadingRigManager;
           
           GUILayout.Label("Editor:" ,EditorStyles.boldLabel);
           GUILayout.Space(5);

           EditorGUI.indentLevel++;
           GUILayout.BeginHorizontal();
           if (GUILayout.Button("Add One Rig"))
           {
               GUIContent iconContent = EditorGUIUtility.IconContent("sv_label_0");
               Texture2D texture = iconContent.image as Texture2D;
               Renderer renderer = _shadingRigManagerObj.GetComponent<Renderer>();
               GameObject rig = new GameObject("Rig");
               _shadingRigManagerObj.shadingRigData.ShadingRigDatas.Add(new SingleShadingRigData(rig));
               rig.transform.position = renderer.bounds.center + _shadingRigManagerObj.transform.forward * renderer.bounds.size.z/2;
               rig.transform.SetParent(_shadingRigManagerObj.transform);
               ShadingRig shadingRig = rig.AddComponent<ShadingRig>();
               _shadingRigManagerObj.shadingRigData.shadingRigs.Add(shadingRig);
               // https://github.com/halak/unity-editor-icons 里面有所有的icon
               iconContent = EditorGUIUtility.IconContent("sv_label_0");
               texture = iconContent.image as Texture2D;
               typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic).Invoke(
                   null, new object[]{ rig, texture});
                   
           }
           
           if (GUILayout.Button("Clear All Rig"))
           {
               if (_shadingRigManagerObj && _shadingRigManagerObj.shadingRigData != null)
               {
                   _shadingRigManagerObj.shadingRigData.ShadingRigDatas.Clear();
                   _shadingRigManagerObj.shadingRigData.shadingRigs.Clear();
               }
               
           }
           GUILayout.EndHorizontal();
           GUILayout.FlexibleSpace();
          ////点击Editor之后，所有pivot都会显示，然后控制。
          //isEditing = GUILayout.Toggle(isEditing,EditorGUIUtility.IconContent("EditCollider" ,"编辑Rig Pivot"),
          //    "Button", GUILayout.Width(35), GUILayout.Height(25));
           
           EditorGUI.BeginChangeCheck();
           EditorGUILayout.PropertyField(shadingRigDataProperty,true);
           if (EditorGUI.EndChangeCheck())
           {
               
           }
           
           GUILayout.FlexibleSpace();
           EditorGUILayout.LabelField("如果是需要使用K好的动画的话，需要勾选Animation,如果想直接在Manager上控制看效果的话，就不要勾选Animation");
           _shadingRigManagerObj.isAnimation = GUILayout.Toggle(_shadingRigManagerObj.isAnimation, "Animation");
           serializedObject.ApplyModifiedProperties();
       }
       private void OnSceneGUI()
       {
           _shadingRigManagerObj.isDebug = true;
           if (isEditing)
           {
               Event e = Event.current;
               if (e.type == EventType.MouseDown && e.button == 0)
               {
                   // 获取鼠标点击的位置
                   Vector2 mousePosition = e.mousePosition;
                   // 将GUI坐标转换为屏幕坐标
                   float ppp = EditorGUIUtility.pixelsPerPoint;
                   mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y * ppp;
                   mousePosition.x *= ppp;

                   // 获取鼠标点击位置的游戏对象
                   GameObject selectedObject = HandleUtility.PickGameObject(mousePosition, false);

                   // 检查这个游戏对象是否是你想要控制的物体
                   if (selectedObject == _shadingRigManagerObj.shadingRigData.ShadingRigDatas[0].Rig)
                   {
                       // 处理你的物体的交互事件
                       // ...
                   }
                   else
                   {
                       // 阻止这个点击事件传递到其他的控件
                       HandleUtility.AddDefaultControl(0);
                   }
               }
               
           }
       }
       
    }
    
}


