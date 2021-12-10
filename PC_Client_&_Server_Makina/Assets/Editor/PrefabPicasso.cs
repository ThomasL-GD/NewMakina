using UnityEngine;
using UnityEditor;

public class PrefabPicasso : EditorWindow
{
    private static bool m_mouseDown;
    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Prefab Picasso")]
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
        EditorGUILayout.Toggle("Receive Movement: ", true);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= GuiUpdate;
    }

    void GuiUpdate(SceneView p_sceneView)
    {
        if(m_mouseDown) UpdateBrush();
        
        Event currentEvent = Event.current;

        /*
        if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDrag)
        {
            Debug.Log("yay");
        }

        /*
        if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown)
        {
            m_mouseDown = true;
            //UpdateBrush();
        }
        
        if(currentEvent.button != 0 || currentEvent.type == EventType.MouseUp)
        {
            m_mouseDown = false;
        }
        /**/
        //Todo : fix this
    }

    void UpdateBrush()
    {
        Debug.Log("Fix Mee");
    }
    
    private void OnGUI()
    {
        GUILayout.Button("yay this button works");
    }
}
