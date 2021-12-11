using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ReplaceObjectByPrefab : EditorWindow
{
    [FormerlySerializedAs("source")] public Object m_prefab;

    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Replace Go By Prefab")]
    static void Init()
    {
        ReplaceObjectByPrefab window = (ReplaceObjectByPrefab)GetWindow(typeof(ReplaceObjectByPrefab));
        window.titleContent = new GUIContent("Replace Object By Prefab");
        
        window.Show();
        window.Focus();
        window.Repaint();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("The prefab to replace the selected objects by");
        m_prefab = EditorGUILayout.ObjectField(m_prefab, typeof(Object),true);

        GameObject prefabGo = (GameObject) m_prefab;
        
        if(prefabGo == null) return;
        if(Selection.gameObjects.Length == 0) return;
        
        GUILayout.Label(" ");
        if (GUILayout.Button("Replace Selected !"))
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Vector3 pos = go.transform.position;
                Quaternion rot = go.transform.rotation;

                Instantiate(prefabGo, pos,rot);
                DestroyImmediate(go);
            }
        }
    }
}
