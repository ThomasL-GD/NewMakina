using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;


public class PrefabPicasso : EditorWindow, IPointerClickHandler
{
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("mouse click!");
    }
    
    /// <summary/> the minimum distance between placed prefabs
    private static float m_spacing = 20f;
    
    /// <summary/> The maximum array length that will memorise the prefabs and the placed locations
    private static int m_maximumArrayLength = 256;
    
    /// <summary/> The placed locations
    private static Vector3[] m_positions;
    
    /// <summary/> The prefabs that can be painted with
    private static GameObject[] m_prefabs;
    
    /// <summary/> The amount of prefabs in the array
    private static int m_addedPrefabs;
    
    /// <summary/> The amount of brushes that have been painted
    private static int m_InstatitedCount = 0;
    
    /// <summary/> The prent of the object to paint
    private static Transform m_parent;

    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Prefab Picasso &v")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        PrefabPicasso window = (PrefabPicasso)GetWindow(typeof(PrefabPicasso));
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Prefab Picasso");
        
        // Display the window that has been created
        window.Show();
        
        // Bring the window to the front
        window.Focus();
        
        // Updating the GUI of the window
        window.Repaint();
    }

    /// <summary>
    /// Adding GuiUpdate to the SceneView.duringSceneGui delegate called everytime
    /// the sceneView is repainted when the editor window is opened
    /// </summary>
    void OnEnable() => SceneView.duringSceneGui += SceneViewUpdate;

    /// <summary/> Adding GuiUpdate from the SceneView.duringSceneGui delegate when the editor window is closed
    private void OnDisable() => SceneView.duringSceneGui -= SceneViewUpdate;

    /// <summary/> When the player is pressing the control key and the mouse is, start painting prefabs
    /// <param name="p_sceneView"> the scene view element </param>
    void SceneViewUpdate(SceneView p_sceneView)
    {
        // Fetching the current event for synchronisation
        Event currentEvent = Event.current;
        
        // if the player is pressing the control key and the mouse is, start painting prefabs
        if (currentEvent.isMouse && currentEvent.control) UpdatePainting(p_sceneView, currentEvent);
    }

    /// <summary>
    /// Using the array of prefabs, and the spacing, both chosen by the user places prefabs where the mouse hits a
    /// collider respecting the social distancing 
    /// </summary>
    /// <param name="p_sceneView"> The scene view element used for the camera position and orientation </param>
    /// <param name="p_currentEvent"> The current event to fetch the mouse position in the scene </param>
    void UpdatePainting(SceneView p_sceneView, Event p_currentEvent)
    {
        // Returning if the settings aren't correctly filled in by the user 
        if (m_addedPrefabs == 0f || m_positions == null || m_prefabs == null) return;
        
        // Getting the mouse position
        Vector3 mousePos = p_currentEvent.mousePosition;
        
        // Getting the scale of the gui points relative to the screen's pixel density
        // In other words basically chill bruh
        float ppp = EditorGUIUtility.pixelsPerPoint;
        
        // Scaling the mouse position
        mousePos.y = p_sceneView.camera.pixelHeight - mousePos.y * ppp;
        mousePos.x *= ppp;

        // Checking if the screen center to mouse position raycast actually hits something
        // Yes this script is based off of the colliders not the mesh not surprise there
        if (!Physics.Raycast(p_sceneView.camera.ScreenPointToRay(mousePos), out RaycastHit hit)) return;
        
        // separating the hit point
        Vector3 hitPoint = hit.point;

        // checking all the previously placed position if the currently seeked real-estate is available 
        foreach (Vector3 pos in m_positions) if (Vector3.Distance(pos,hitPoint) < m_spacing) return;

        // Instantiating one of the randomly selected prefabs at the mouses position and settng it to a parent if the user chose so  
        GameObject spawnedGo = Instantiate(m_prefabs[Random.Range(0,m_addedPrefabs)],hitPoint,Quaternion.Euler(0f,0f,0f));
        if (m_parent != null) spawnedGo.transform.parent = m_parent;

        Vector3 rotation = spawnedGo.transform.rotation.eulerAngles;
        
        spawnedGo.transform.rotation = Quaternion.Euler(rotation.x ,rotation.y + 90f * Round(Random.Range(0,4)),rotation.z);
        
        // Updating the count of instantiated prefabs so that the positions can be correctly researched
        m_positions[m_InstatitedCount++] = hitPoint;
        
        // Looping the count of instantiated prefabs
        m_InstatitedCount %= m_maximumArrayLength;
    }
    
    
    /// <summary>
    /// Updating th GUI of the tool
    /// </summary>
    private void OnGUI()
    {
        Label("\nThe parent of the spawned prefab (null = no parent)");
        
        // the parent gameObject object field
        GameObject go = (GameObject)ObjectField(m_parent == null ? null : m_parent.gameObject, typeof(Object), true);
        m_parent = go == null? null : go.transform;

        // The Numeric variables
        m_spacing = Abs(FloatField("Spacing", m_spacing));
        m_maximumArrayLength = Abs(IntField("Maximum Array Length", 256));
        Label("\n");
        
        // The button to reset the grid and forget all the previously placed prefabs positions
        if (Button("\nReset Grid\n")) m_positions = new Vector3[m_maximumArrayLength];
        
        // Checking if the variables are null for later use
        // If they are fixing that
        if (m_prefabs == null) m_prefabs = new GameObject[m_maximumArrayLength];
        if (m_positions == null) m_positions = new Vector3[m_maximumArrayLength];
        
        Label("\nThe Prefabs to instantiate\n");
        
        //Creating an bject field for all the set prefab fields prefabs + 1
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
