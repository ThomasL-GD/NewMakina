using UnityEngine;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

public class WindowPlacer : EditorWindow
{
    GUIContent m_IconContent;
    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Window Placer")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        WindowPlacer window = (WindowPlacer)GetWindow(typeof(WindowPlacer));
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Window Placer");
        
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
    
    // This is called for each window that your tool is active in. Put the functionality of your tool here.
    public void SceneViewUpdate(EditorWindow window)
    {
        if(Selection.count == 0) return;
        GameObject selection = (GameObject)Selection.objects[0];
        if(selection==null || !((GameObject)Selection.objects[0]).TryGetComponent(out MeshRenderer meshRenderer)) return;
        
        Bounds selectionBounds = meshRenderer.bounds;
        Vector3 center = selectionBounds.center;
        Vector3 extents = selectionBounds.extents;
        DrawCube(center,extents *2f, Color.white);

        FindFaceNormal(center, extents.x * Vector3.right);
        FindFaceNormal(center, -extents.x * Vector3.right);
        FindFaceNormal(center, extents.z * Vector3.forward);
        FindFaceNormal(center, -extents.z * Vector3.forward);
    }

    private RaycastHit FindFaceNormal(Vector3 p_boundingBoxCenter,Vector3 p_chosenExtent)
    {
        
        Vector3 planeCenter = p_boundingBoxCenter + p_chosenExtent;
        Vector3 opposedPlaneCenter = p_boundingBoxCenter - p_chosenExtent;
        
        if (!Physics.Raycast(planeCenter, opposedPlaneCenter - planeCenter, out RaycastHit hit, p_chosenExtent.magnitude*2f)) return new RaycastHit();
        
        Debug.DrawLine(planeCenter, hit.point, Color.green);
        Debug.DrawRay(hit.point,hit.normal *4f, Color.red);
        return hit;
    }

    private void DrawCube(Vector3 p_selectionBoundsCenter, Vector3 p_selectionBoundsExtents, Color p_color)
    {
        Vector3 corner = p_selectionBoundsCenter - p_selectionBoundsExtents/2f;
        Vector3 nextCorner = corner;
        nextCorner.x += p_selectionBoundsExtents.x;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.y += p_selectionBoundsExtents.y;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.x -= p_selectionBoundsExtents.x;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.y -= p_selectionBoundsExtents.y;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.z += p_selectionBoundsExtents.z;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.y += p_selectionBoundsExtents.y;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.x += p_selectionBoundsExtents.x;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.y -= p_selectionBoundsExtents.y;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        nextCorner.x -= p_selectionBoundsExtents.x;
        Debug.DrawLine(corner,nextCorner,p_color);
        
        corner = nextCorner;
        corner.y += p_selectionBoundsExtents.y;
        nextCorner = corner;
        nextCorner.z -= p_selectionBoundsExtents.z;
        Debug.DrawLine(corner,nextCorner,p_color);

        corner.x += p_selectionBoundsExtents.x;
        nextCorner.x = corner.x;
        Debug.DrawLine(corner,nextCorner,p_color);

        corner.y -= p_selectionBoundsExtents.y;
        nextCorner.y = corner.y;
        Debug.DrawLine(corner,nextCorner,p_color);
    }
}
