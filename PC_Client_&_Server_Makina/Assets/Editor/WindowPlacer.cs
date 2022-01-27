using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

public class WindowPlacer : EditorWindow
{
    GUIContent m_IconContent;
    private float m_offsetSideRed;
    private float m_offsetSideGreen;
    private float m_offsetSideBlue;
    private float m_offsetSideYellow;
    
    private float m_heightSide;

    private float m_spacing=1f;
    
    private float m_objectOffsetRed;
    private float m_objectOffsetGreen;
    private float m_objectOffsetBlue;
    private float m_objectOffsetYellow;
    
    private Vector3 m_angleOffset;

    private List<Positions> m_points;
    private GameObject m_window;

    struct Positions
    {
        public Vector3 position;
        public Quaternion rotation;
    }
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
        if(Selection.count == 0)
        {
            m_points = new List<Positions>();
            return;
        }
        GameObject selection = (GameObject)Selection.objects[0];
        if(selection==null || !((GameObject)Selection.objects[0]).TryGetComponent(out MeshRenderer meshRenderer)) 
        {
            m_points = new List<Positions>();
            return;
        }
        
        Bounds selectionBounds = meshRenderer.bounds;
        Vector3 center = selectionBounds.center;
        Vector3 extents = selectionBounds.extents;

        float diagonal = extents.magnitude * 4f;
        
        DrawCube(center,extents *2f, Color.white);

        Vector3 offset = Vector3.forward * m_offsetSideRed + Vector3.up * m_heightSide;
        
        RaycastHit redHit = FindFaceNormal(center+offset, extents.x * Vector3.right + Vector3.up * m_heightSide, Color.red);
        
        offset = Vector3.forward * m_offsetSideGreen + Vector3.up * m_heightSide;
        RaycastHit greenHit = FindFaceNormal(center+offset, -extents.x * Vector3.right+Vector3.up * m_heightSide, Color.green);
        
        offset = Vector3.right * m_offsetSideBlue + Vector3.up * m_heightSide;
        RaycastHit blueHit = FindFaceNormal(center+offset, extents.z * Vector3.forward+Vector3.up * m_heightSide, Color.cyan);
        
        offset = Vector3.right * m_offsetSideYellow + Vector3.up * m_heightSide;
        RaycastHit yellowHit = FindFaceNormal(center+offset, -extents.z * Vector3.forward+Vector3.up * m_heightSide, Color.yellow);

        m_points = new List<Positions>();
        
        //Red Side
        FindWallEdges(redHit.normal,redHit.point,out Vector3 e1,out Vector3 e2, out Vector3 wallDir, diagonal, Color.red);
        FindWindowPositions(e1, e2,wallDir,redHit.normal,m_objectOffsetRed,Color.red);
        
        
        //Green Side
        FindWallEdges(greenHit.normal,greenHit.point,out e1,out e2, out wallDir, diagonal, Color.green);
        FindWindowPositions(e1, e2,wallDir,greenHit.normal,m_objectOffsetGreen,Color.green);
        
        //Blue Side
        FindWallEdges(blueHit.normal,blueHit.point,out e1,out e2, out wallDir, diagonal, Color.cyan);
        FindWindowPositions(e1, e2,wallDir,blueHit.normal,m_objectOffsetBlue,Color.cyan);
        
        //Yellow Side
        FindWallEdges(yellowHit.normal,yellowHit.point,out e1,out e2, out wallDir, diagonal, Color.yellow);
        FindWindowPositions(e1, e2,wallDir,yellowHit.normal,m_objectOffsetYellow,Color.yellow);
    }

    void FindWindowPositions(Vector3 p_e1, Vector3 p_e2, Vector3 p_wallDir, Vector3 WallNormal, float p_offset, Color p_color)
    {
        int amountOfPossibleWidows = (int)Floor(Vector3.Distance(p_e1, p_e2)/m_spacing);
    
        for (int i = 0; i < amountOfPossibleWidows+1; i++)
        {
            Vector3 origin = (p_e2 + p_wallDir * i * m_spacing) + WallNormal + p_wallDir * p_offset;
            if (!Physics.Raycast(origin, -WallNormal, out RaycastHit hit, 2f)) continue;
            DrawCube(hit.point, Vector3.one *.5f, p_color);
            Quaternion rotation = Quaternion.Euler(Vector3.up * Vector3.Angle(Vector3.forward, new Vector3(hit.normal.x,0,hit.normal.z))+ m_angleOffset);
            m_points.Add(new Positions(){position = hit.point,rotation = rotation});
        }
    }

    private void FindWallEdges(Vector3 p_wallNormal,Vector3 hitPoint, out Vector3 p_edge1, out Vector3 p_edge2, out Vector3 p_wallDir, float p_diagonal, Color p_color)
    {
        Handles.color = p_color;
        p_wallDir = Vector3.ProjectOnPlane(new Vector3(1, 0, 1), p_wallNormal).normalized;
        Physics.Raycast(hitPoint + p_wallDir * (p_diagonal / 2), -p_wallDir, out RaycastHit hit, p_diagonal);
        Handles.DrawLine(hit.point, hitPoint);
        p_edge1 = hit.point;
        
        
        Physics.Raycast(hitPoint - p_wallDir * (p_diagonal / 2), p_wallDir, out hit, p_diagonal);
        Handles.DrawLine(hit.point, hitPoint);
        p_edge2 = hit.point;
    }
    
    private Vector3 GetTop(Vector3 p_hitPoint, float height)
    {
        Vector3 origin = p_hitPoint + Vector3.up * height * 2f;
        Physics.Raycast(origin, Vector3.down, out RaycastHit hit, height * 2f);
        Debug.DrawLine(origin, hit.point, Color.blue);
        return hit.point;
    }
    
    private RaycastHit FindFaceNormal(Vector3 p_boundingBoxCenter,Vector3 p_chosenExtent, Color p_color)
    {
        
        Vector3 planeCenter = p_boundingBoxCenter + p_chosenExtent;
        Vector3 opposedPlaneCenter = p_boundingBoxCenter - p_chosenExtent;
        
        if (!Physics.Raycast(planeCenter, opposedPlaneCenter - planeCenter, out RaycastHit hit, p_chosenExtent.magnitude*2f)) return new RaycastHit();

        Handles.color = p_color;
        Handles.DrawLine(planeCenter+hit.normal, hit.point);
        
        return hit;
    }

    private void DrawCube(Vector3 p_selectionBoundsCenter, Vector3 p_selectionBoundsExtents, Color p_color)
    {
        Handles.color = p_color;
        
        Vector3 corner = p_selectionBoundsCenter - p_selectionBoundsExtents/2f;
        Vector3 nextCorner = corner;
        nextCorner.x += p_selectionBoundsExtents.x;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.y += p_selectionBoundsExtents.y;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.x -= p_selectionBoundsExtents.x;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.y -= p_selectionBoundsExtents.y;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.z += p_selectionBoundsExtents.z;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.y += p_selectionBoundsExtents.y;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.x += p_selectionBoundsExtents.x;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.y -= p_selectionBoundsExtents.y;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        nextCorner.x -= p_selectionBoundsExtents.x;
        Handles.DrawLine(corner,nextCorner);
        
        corner = nextCorner;
        corner.y += p_selectionBoundsExtents.y;
        nextCorner = corner;
        nextCorner.z -= p_selectionBoundsExtents.z;
        Handles.DrawLine(corner,nextCorner);

        corner.x += p_selectionBoundsExtents.x;
        nextCorner.x = corner.x;
        Handles.DrawLine(corner,nextCorner);

        corner.y -= p_selectionBoundsExtents.y;
        nextCorner.y = corner.y;
        Handles.DrawLine(corner,nextCorner);
    }
    
    private void OnGUI()
    {
        Label("\n Offset x");
        m_offsetSideBlue = FloatField("offset blue", m_offsetSideBlue);
        m_offsetSideGreen = FloatField("offset green", m_offsetSideGreen);
        m_offsetSideRed = FloatField("offset red", m_offsetSideRed);
        m_offsetSideYellow = FloatField("offset yellow", m_offsetSideYellow);
        Label("\n Offset y");
        m_heightSide = FloatField("offset height", m_heightSide);
        Space();
        m_spacing = FloatField("spacing", m_spacing);
        Space();
        m_objectOffsetRed = FloatField("object offset red", m_objectOffsetRed);
        m_objectOffsetGreen = FloatField("object offset green", m_objectOffsetGreen);
        m_objectOffsetBlue = FloatField("object offset blue", m_objectOffsetBlue);
        m_objectOffsetYellow = FloatField("object offset yellow", m_objectOffsetYellow);
        Space();
        m_window = (GameObject) ObjectField("windowPrefab",m_window,typeof(object),true);
        Space();
        m_angleOffset = Vector3Field("Angle Offset", m_angleOffset);

        if (Button("\n Place Windows\n") && m_points.Count != 0)
        {
            var parent = new GameObject();
            parent.name = "window parent";
            
            Undo.RegisterCreatedObjectUndo(parent, "created parent");
            
            foreach (var point in m_points)
            {
                var placedWindow = Instantiate(m_window,point.position,point.rotation);
                placedWindow.transform.parent = parent.transform;
            }
        }
    }
}
