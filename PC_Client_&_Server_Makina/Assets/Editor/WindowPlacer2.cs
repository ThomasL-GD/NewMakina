using System.Collections.Generic;
using NUnit.Compatibility;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

class WindowPlacer2 : EditorWindow
{
    private Material m_previewMat;
    private static GameObject m_window;
    private static GameObject m_parent;
    
    private static float m_spacing=1f;
    private static float m_lineHeight=0f;
    private static float m_margin;

    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Window Placer 2")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        WindowPlacer2 window = (WindowPlacer2)GetWindow(typeof(WindowPlacer2));
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Window Placer 2");
        
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
    
    public void SceneViewUpdate(EditorWindow window)
    {
        if(Selection.count == 0) return;
        
        GameObject selection = (GameObject)Selection.objects[0];
        if(selection==null || !((GameObject)Selection.objects[0]).TryGetComponent(out MeshFilter filter)) return;
        
        Mesh mesh = filter.sharedMesh;
        
        
        // making a list of all the triangles of the mesh
        List<Triangle> triangles = new List<Triangle>();

        // Getting all of the triangles of the mesh 
        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;
        int[] tr = mesh.triangles;
        
        for (int i = 0; i < tr.Length;) 
            triangles.Add(new Triangle(v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i]], tr[i++]));
        
        List<Intersections> intersections = new List<Intersections>();

        foreach (var triangle in triangles)
        {
            List<Intersections> localLine = new List<Intersections>();
            triangle.DrawHorizontalLineAlongMesh(selection.transform.position,m_lineHeight,selection.transform.rotation, selection.transform.localScale, out localLine);
            foreach (var item in localLine)
            {
                bool canAdd = true;

                for (int i = 0; i < intersections.Count; i++)
                {
                   
                    if(intersections[i].position == item.position)
                    {
                        intersections[i].triangles.Add(item.triangles[0]);
                        intersections[i].normal.Add(item.normal[0]);
                        canAdd = false;
                    }
                }
                if(canAdd)intersections.Add(item); 
            }
        }

        List<Link> links = new List<Link>();
        
        Handles.color = Color.red;
        
        for (int i = 0; i < intersections.Count; i++)
        {
            for (int j = i; j < intersections.Count; j++)
            {
                if(j==i) continue;
                for (int k = 0; k < intersections[i].triangles.Count; k++)
                {
                    for (int l = 0; l < intersections[j].triangles.Count; l++)
                    {
                        if (intersections[i].triangles[k] == intersections[j].triangles[l])
                            links.Add(new Link(intersections[i].position, intersections[j].position,intersections[i].normal[k]));
                    }
                }
            }
        }

        
        for (int i = 0; i < links.Count; i++)
        {
            for (int j = i; j < links.Count; j++)
            {
                if(j==i) continue;
                
                bool connected = links[i].A == links[j].A || links[i].A == links[j].B || links[i].B == links[j].A || links[i].B == links[j].B;
                
                float dotAbs = Mathf.Abs(Vector3.Dot((links[i].A - links[i].B).normalized, (links[j].A - links[j].B).normalized));
        
                if (connected && Mathf.Approximately(dotAbs, 1f))
                {
                    if (links[i].A == links[j].A)
                    {
                        links[i].A = links[i].B;
                        links[i].B = links[j].B;
                    }else if (links[i].A == links[j].B)
                    {
                        links[i].B = links[j].A;
                    }else if (links[i].B == links[j].A)
                    {
                        links[i].B = links[j].B;
                    }else if (links[i].B == links[j].B)
                    {
                        links[i].B = links[i].A;
                        links[i].A = links[j].A;
                    }
                    links.RemoveAt(j);
                }
            }
        }
        
        // Debug.Log(links.Count);
        foreach (var link in links) link.DrawLink();

        Material previewMat = Resources.Load("Editor/Tools Material/PreviewMaterial", typeof(Material)) as Material;

        previewMat.SetPass(0);
        Graphics.DrawMeshNow(m_window.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh,Vector3.one * 10,m_window.transform.rotation);
    }
    
    class Link
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 normal;

        public Link(Vector3 p_A, Vector3 p_B, Vector3 p_normal)
        {
            A = p_A;
            B = p_B;
            normal = p_normal;
        }

        public void DrawLink()
        {
            Handles.color = Color.red;
            Handles.DrawLine(A, B);
            Handles.color = Color.yellow;
            float radius = .1f;
            Handles.DrawWireCube(A,radius * Vector3.one);
            Handles.DrawWireCube(B,radius * Vector3.one);
            Handles.color = Color.green;
            Vector3 origin = A + (B - A) / 2;
            Handles.DrawLine(origin, origin+ (normal *1f));
        }
    }
    
    struct Intersections
    {
        public Vector3 position;
        public List<int> triangles;
        public List<Vector3> normal;
    }
    
    struct Triangle
    {
        public Vector3 vertex1;
        public Vector3 vertex2;
        public Vector3 vertex3;
        public Vector3 normal;
        public int triangleId;
        public Triangle(Vector3 p_v1,Vector3 p_n1,Vector3 p_v2,Vector3 p_n2,Vector3 p_v3,Vector3 p_n3, int id)
        {
            vertex1 = p_v1;
            vertex2 = p_v2;
            vertex3 = p_v3;
            normal = (p_n1 + p_n2 + p_n3) / 3;
            triangleId = id;
        }

        private void DrawFace(Vector3 p_position, Camera p_cam, Quaternion p_rotation, Vector3 p_scale)
        {
            Vector3 v1 = p_rotation * Vector3.Scale( vertex1, p_scale);
            Vector3 v2 = p_rotation * Vector3.Scale(vertex2, p_scale);
            Vector3 v3 = p_rotation * Vector3.Scale(vertex3, p_scale);

            if ((CheckIfInCameraView(p_cam, p_position + v1) && CheckIfInCameraView(p_cam, p_position + v2)
                                                             && CheckIfInCameraView(p_cam, p_position + v3))) return;
            Handles.color = Color.blue;

            Handles.DrawLine(p_position + v1,p_position + v2);
            Handles.DrawLine(p_position + v2,p_position + v3);
            Handles.DrawLine(p_position + v3,p_position + v1);
        }

        public void DrawHorizontalLineAlongMesh(Vector3 p_position, float p_lineHeight, Quaternion p_rotation, Vector3 p_scale, out List<Intersections> p_intersections)
        {
            p_intersections = new List<Intersections>();
            Vector3 v1 = p_rotation * Vector3.Scale(vertex1, p_scale);
            Vector3 v2 = p_rotation * Vector3.Scale(vertex2, p_scale);
            Vector3 v3 = p_rotation * Vector3.Scale(vertex3, p_scale);
            Vector3 n = p_rotation * normal;
            bool tooHigh = v1.y >= p_lineHeight && v2.y >= p_lineHeight && v3.y >= p_lineHeight;
            bool tooLow = v1.y <= p_lineHeight && v2.y <= p_lineHeight && v3.y <= p_lineHeight;
            
            if (!tooHigh && !tooLow)
            {
                //DrawFace(p_position,Camera.current,p_rotation,p_scale);

                Vector3 intersection;
                if (GetSegmentPlaneIntersection(v1, v2, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} , normal = new List<Vector3>(){n}});
                }
                    
                
                if (GetSegmentPlaneIntersection(v2, v3, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} , normal = new List<Vector3>(){n}});
                }
                
                if (GetSegmentPlaneIntersection(v3, v1, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} , normal = new List<Vector3>(){n}});
                }
            }
        }
        
        float GetSignedDistFromPlane(Vector3 p_point, float p_planeHeight)
        {
            return p_point.y - p_planeHeight;
        }

        bool GetSegmentPlaneIntersection(Vector3 p_point1, Vector3 p_point2, out Vector3 p_intersection, float p_planeHeight)
        {
            float d1 = GetSignedDistFromPlane(p_point1, p_planeHeight),
                d2 = GetSignedDistFromPlane(p_point2, p_planeHeight);

            p_intersection = Vector3.zero;
            if (d1 * d2 > 0) return false;
            
            float t = d1 / (d1 - d2);
            p_intersection = p_point1 + t * (p_point2 - p_point1);
            return true;
        }
        
        private bool CheckIfInCameraView(Camera p_cam, Vector3 p_point)
        {
            Vector2 viewportPoint = p_cam.WorldToViewportPoint(p_point);
            
            return viewportPoint.x !< 0 && viewportPoint.x !> 1 && viewportPoint.y !< 0 && viewportPoint.y !> 1;
        }
    }
    
    private void OnGUI()
    {
        
        m_window = (GameObject) ObjectField("windowPrefab",m_window,typeof(object),true);
        m_previewMat = (Material) ObjectField("windowPrefab",m_previewMat,typeof(object),true);
        
        if(m_window == null) return;
        
        m_spacing=FloatField("spacing", m_spacing);
        m_margin=FloatField("horizontal", m_margin);
        m_lineHeight=FloatField("lineHeight", m_lineHeight);
        
        m_parent = (GameObject) ObjectField("parent",m_parent,typeof(object),true);
    }
}
