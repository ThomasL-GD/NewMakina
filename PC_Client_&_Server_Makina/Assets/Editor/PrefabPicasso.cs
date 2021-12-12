using UnityEngine;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

public class PrefabPicasso : EditorWindow
{
    private static bool m_mouseDown;
    private static float m_spacing = 20f;
    private static int m_maximumArrayLength = 256;
    private static Vector3[] m_positions;
    private static GameObject[] m_prefabs;
    private static int m_addedPrefabs;
    private static int m_InstatitedCount = 0;
    private static Transform m_parent;
    private static bool m_painting;
    
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
        m_painting = false;
        SceneView.duringSceneGui += GuiUpdate;
    }

    private void OnDisable() => SceneView.duringSceneGui -= GuiUpdate;

    void GuiUpdate(SceneView p_sceneView)
    {
        Tools.current = Tool.Custom;
        
        Event currentEvent = Event.current;
        
        if (currentEvent.isMouse && currentEvent.control) UpdatePainting(p_sceneView, currentEvent);
    }

    void UpdatePainting(SceneView p_sceneView, Event p_currentEvent)
    {
        if (m_addedPrefabs == 0f || m_positions == null || m_prefabs == null) return;
        
        Vector3 mousePos = p_currentEvent.mousePosition;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        mousePos.y = p_sceneView.camera.pixelHeight - mousePos.y * ppp;
        mousePos.x *= ppp;

        if (!Physics.Raycast(p_sceneView.camera.ScreenPointToRay(mousePos), out RaycastHit hit)) return;
        Vector3 hitPoint = Vector3.zero;
        hitPoint = hit.point;

        foreach (Vector3 pos in m_positions)
        {
            if (Vector3.Distance(pos,hitPoint) < m_spacing)
            {
                return;
            }
        }


        m_positions[m_InstatitedCount] = hitPoint;
        GameObject spawnedGo = Instantiate(m_prefabs[Random.Range(0,m_addedPrefabs)],hitPoint,Quaternion.Euler(0f,0f,0f));
        
        if (m_parent != null) spawnedGo.transform.parent = m_parent;
        m_InstatitedCount++;
        m_InstatitedCount = m_InstatitedCount % m_maximumArrayLength;
    }
    
    
    
    private void OnGUI()
    {
        Label("\nThe parent of the spawned prefab (null = top)");
        GameObject go =
            (GameObject)ObjectField(m_parent == null ? null : m_parent.gameObject, typeof(Object), true);
        m_parent = go == null? null : go.transform;

        m_spacing = Abs(FloatField("Spacing", m_spacing));
        m_maximumArrayLength = Abs(IntField("Maximum Array Length", 256));
        Label("\n");
        
        if (Button("\nReset Grid\n")) m_positions = new Vector3[m_maximumArrayLength];
        
        
        if (m_prefabs == null) m_prefabs = new GameObject[m_maximumArrayLength];
        if (m_positions == null) m_positions = new Vector3[m_maximumArrayLength];
        
        Label("\nThe Prefabs to instantiate\n");
        
        for (int i = 0; i < m_prefabs.Length; i++)
        {
            m_prefabs[i]= (GameObject) ObjectField(m_prefabs[i] == null?null:m_prefabs[i], typeof(Object), true);
            if (m_prefabs[i] == null)
            {
                m_addedPrefabs = i;
                break;
            }
        }
    }
}
