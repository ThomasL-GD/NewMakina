using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabPicasso : EditorWindow
{
    private static bool m_mouseDown;
    private static float m_spacing;
    private static float m_placingSpeed;
    private static int m_maximumArrayLength = 256;
    private static Vector3[] positions;
    private static GameObject[] m_prefabs;
    private static GameObject m_cursor;
    
    
    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Prefab Picasso &v")]
    static void Init()
    {
        PrefabPicasso window = (PrefabPicasso)GetWindow(typeof(PrefabPicasso));
        window.titleContent = new GUIContent("Prefab Picasso");
        window.Show();
        window.Focus();
        window.Repaint();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += GuiUpdate;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= GuiUpdate;
    }

    void GuiUpdate(SceneView p_sceneView)
    {
        Tools.current = Tool.Custom;
        if(m_mouseDown) UpdateBrush(p_sceneView);
        
        Event currentEvent = Event.current;

        if (currentEvent.control && currentEvent.button == 1 && currentEvent.type == EventType.MouseDown)
        {
            m_mouseDown = true;
            UpdateBrush(p_sceneView);
        }
        
        if (!currentEvent.control || (currentEvent.button == 1 && currentEvent.type == EventType.MouseUp))
        {
            m_mouseDown = false;
            m_cursor.transform.position = Vector3.zero;
        }
    }

    private static float m_timer;

    void UpdateBrush(SceneView p_sceneView)
    {
        Vector3 mousePos = Event.current.mousePosition;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        mousePos.y = p_sceneView.camera.pixelHeight - mousePos.y * ppp;
        mousePos.x *= ppp;
        
        Physics.Raycast(p_sceneView.camera.ScreenPointToRay(mousePos), out RaycastHit hit);
        if (m_cursor != null) m_cursor.transform.position = hit.point;
    }
    
    private void OnGUI()
    {
        m_cursor = (GameObject)EditorGUILayout.ObjectField(m_cursor, typeof(Object), true);
        m_spacing = Mathf.Abs(EditorGUILayout.FloatField("Spacing", 5f));
        m_placingSpeed = Mathf.Abs(EditorGUILayout.FloatField("Placing Speed", .2f));
        
        GUILayout.Label(" ");
        GUILayout.Label(" ");
        
        m_maximumArrayLength = Mathf.Abs(EditorGUILayout.IntField("Maximum Array Length", 256));
        
        GUILayout.Label(" ");
        GUILayout.Label(" ");

        if (m_prefabs == null) m_prefabs = new GameObject[m_maximumArrayLength];
        
        for (int i = 0; i < m_prefabs.Length; i++)
        {
            m_prefabs[i]= (GameObject) EditorGUILayout.ObjectField(m_prefabs[i] == null?null:m_prefabs[i], typeof(Object), true);
            if (m_prefabs[i] == null) break;
        }
    }
}
