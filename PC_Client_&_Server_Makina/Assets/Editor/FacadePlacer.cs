using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

class FacadePlacer : EditorWindow
{
    /// <summary/> The material of the preview material
    private Material m_previewMat;
    /// <summary/> the window prefab
    private static GameObject m_facade;
    private static GameObject m_facadeTop;
    private static GameObject m_facadeBottom;
    /// <summary/> the corner prefab
    private static GameObject m_corner;
    
    /// <summary/> the height at which the object will be placed
    private static float m_lineHeight = 65.769f;
    /// <summary/> the y offset of the corner
    private static float m_cornerOffset = 0f;

    /// <summary/> the points on which the windows will be placed
    private static List<Point> m_points;
    private static List<Point> m_corners;

    /// <summary/> the selected object
    private GameObject m_selection;

    private static bool m_useCorners = true;
    private static int m_lines = 3;
    private static Vector3 m_previewRotationOffset = Vector3.zero;
    private static float m_assetWidth = 6.2f;
    private static float m_assetHeight = 6.5f;

    /// <summary/> The function called when the MenuItem is called to create the window
    [MenuItem("Tools/Facade Placer")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        FacadePlacer window = (FacadePlacer)GetWindow(typeof(FacadePlacer));
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Facade Placer");
        
        // Display the window that has been created
        window.Show();
        
        // Bring the window to the front
        window.Focus();
        
        // Updating the GUI of the window
        window.Repaint();

        if (m_previewRotationOffset == Vector3.zero) m_previewRotationOffset = new Vector3(-90f, 0f, 0f);
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
        // checking if the selected object is a GameObject
        if(m_facade == null) return;
        if(Selection.count == 0) return;
        
        GameObject selection = (GameObject)Selection.objects[0];
        if(selection==null || !((GameObject)Selection.objects[0]).TryGetComponent(out MeshFilter filter)) return;
        
        // Getting the mesh of the elected object
        Mesh mesh = filter.sharedMesh;
        m_selection = selection;
        
        // making a list of all the triangles of the mesh
        List<Triangle> triangles = new List<Triangle>();

        // Getting all of the triangles of the mesh 
        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;
        int[] tr = mesh.triangles;

        // Converting everyting to triangles
        for (int i = 0; i < tr.Length;) 
            triangles.Add(new Triangle(v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i]], tr[i++]));
        
        if(m_useCorners && m_corner != null) m_corners = new List<Point>();
        m_points = new List<Point>();

        m_lineHeight = v[0].y * m_selection.transform.lossyScale.y;

        float lineTop = m_lineHeight;
        for (uint i =1; i < v.Length; i++)
        {
            float y = v[i].y * m_selection.transform.lossyScale.y;
            m_lineHeight = Min(m_lineHeight, y);
            lineTop = Max(lineTop,y);
        }
        
        m_lineHeight += 0.005f;

        m_lines = CeilToInt((lineTop - m_lineHeight) / m_assetHeight);

        for (int lineNumber = 0; lineNumber < m_lines; lineNumber++)
        {
            FacadeState facadeState = lineNumber == 0 ? FacadeState.bottom : lineNumber == m_lines - 1 ? FacadeState.top : FacadeState.normal;
            
            // Getting the intersection of all the triangles on the line
            List<Intersections> intersections = new List<Intersections>();
            
            foreach (var triangle in triangles)
            {
                List<Intersections> localLine = new List<Intersections>();
                // finding the triangles on the line
                triangle.DrawHorizontalLineAlongMesh(selection.transform.position,m_lineHeight + (m_assetHeight * lineNumber),selection.transform.rotation, selection.transform.localScale, out localLine);
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

            // Getting all the links
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

            
            
            // Removing the double links and making the aligned links join
            for (int i = 0; i < links.Count; i++)
            {
                for (int j = 0; j < links.Count; j++)
                {
                    if(j==i) continue;
                    
                    bool connected = links[i].A == links[j].A || links[i].A == links[j].B || links[i].B == links[j].A || links[i].B == links[j].B;
                    
                    float dotAbs = Abs(Vector3.Dot((links[i].A - links[i].B).normalized, (links[j].A - links[j].B).normalized));
            
                    if (connected && (1f - dotAbs) < 0.01f)
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

            foreach (var link in links) link.DrawLink();
            

            MeshFilter meshFilter;

            // Getting the corners
            if(m_useCorners && m_corner != null)
            {

                // Getting the corners
                foreach (var link in links)
                {
                    Quaternion rotation = m_corner.transform.GetChild(0)!= null ? m_corner.transform.GetChild(0).rotation : m_corner.transform.rotation;
                    rotation = Quaternion.Euler(Vector3.up * Vector3.SignedAngle(Vector3.forward, new Vector3(link.normal.x,0,link.normal.z), Vector3.up) + rotation.eulerAngles);

                    m_corners.Add(new Point(link.A, rotation, facadeState,1f));
                    m_corners.Add(new Point(link.B, rotation, facadeState,1f));
                }

                // Removing corner dupes
                for (int i = 0; i < m_corners.Count; i++)
                {
                    for (int j = i+1; j < m_corners.Count; j++)
                    {
                        if (m_corners[i].position == m_corners[j].position)
                        {
                            m_corners.RemoveAt(j);
                        }
                    }
                }
                
                
                // Todo : make this into get component in children
                
                // Getting the mesh filter for the preview
                GameObject cornerPrefab = m_corner;
                if(!cornerPrefab.TryGetComponent(out meshFilter))
                {
                    for (int i = 0; i < cornerPrefab.transform.childCount; i++)
                    {
                        if(cornerPrefab.transform.GetChild(i).TryGetComponent(out meshFilter)) break;
                    
                        for (int j = 0; j < cornerPrefab.transform.GetChild(i).childCount; j++)
                    
                            if(cornerPrefab.transform.GetChild(i).GetChild(j).TryGetComponent(out meshFilter)) break;
                    }
                }
                Mesh cornerMesh = meshFilter.sharedMesh;
            
                // Setting the material before drawing 
                Material previewMatCorner = EditorGUIUtility.Load("Assets/Editor/PreviewMaterialCorner.mat") as Material;
                if (previewMatCorner != null) previewMatCorner.SetPass(0);
                
                foreach (var corner in m_corners)
                {
                    Graphics.DrawMeshNow(cornerMesh, corner.position, Quaternion.Euler(corner.rotation.eulerAngles + m_previewRotationOffset));
                }
            }

            
            // Getting the mesh filter for the preview
            GameObject prefab = m_facade;
            
            switch (facadeState)
            {
                case FacadeState.top:
                    prefab = m_facadeTop;
                    break;
                case FacadeState.bottom:
                    prefab = m_facadeBottom;
                    break;
            }
            
            if(!prefab.TryGetComponent(out meshFilter))
            {
                for (int i = 0; i < prefab.transform.childCount; i++)
                {
                    if(prefab.transform.GetChild(i).TryGetComponent(out meshFilter)) break;
                    
                    for (int j = 0; j < prefab.transform.GetChild(i).childCount; j++)
                    
                        if(prefab.transform.GetChild(i).GetChild(j).TryGetComponent(out meshFilter)) break;
                }
            }
            
            Mesh windowMesh = meshFilter.sharedMesh;
            
            // Setting the material before drawing 
            Material previewMat = EditorGUIUtility.Load("Assets/Editor/PreviewMaterial.mat") as Material;
            if (previewMat != null) previewMat.SetPass(0);

            foreach (var link in links)
            {
                
                float length = Vector3.Distance(link.A, link.B);
                int amount = RoundToInt(length / m_assetWidth);
                amount = Max(1, amount);
                float step = length / amount;
                float scale = step/m_assetWidth;
                
                for (int i = 0; i < amount; i++)
                {
                    Vector3 position = link.A + (link.B - link.A).normalized * (step * i + m_assetWidth * scale / 2f);
                    Quaternion prefabRotation;
                    prefabRotation = Quaternion.Euler(Vector3.up * Vector3.SignedAngle(Vector3.forward, new Vector3(link.normal.x,0,link.normal.z), Vector3.up) + m_previewRotationOffset);
                    
                    
                    GameObject preview = new GameObject
                    {
                        transform =
                        {
                            position = position,
                            rotation = prefabRotation,
                            localScale = new Vector3(scale,1,1)
                        }
                    };
                    
                    Matrix4x4 matrix = preview.transform.localToWorldMatrix;
                    
                    DestroyImmediate(preview);
                    
                    Graphics.DrawMeshNow(windowMesh, matrix);
                    prefabRotation = Quaternion.Euler(Vector3.up * Vector3.SignedAngle(Vector3.forward, new Vector3(link.normal.x,0,link.normal.z), Vector3.up) + m_facade.transform.eulerAngles);
                    m_points.Add(new Point(position,prefabRotation,facadeState,scale));
                }

            }
        }
    }

    enum FacadeState
    {
        normal,
        top,
        bottom
    }
    struct Point
    {
        public Vector3 position;
        public Quaternion rotation;
        public FacadeState facadeState;
        public float width;

        public Point(Vector3 p_position, Quaternion p_rotation, FacadeState p_facadeState, float p_width)
        {
            position = p_position;
            rotation = p_rotation;
            facadeState = p_facadeState;
            width = p_width;
        }
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
            if (Vector3.Dot(Camera.current.transform.forward, normal) >= 0) return;
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
            bool tooHigh = v1.y >= p_lineHeight+Epsilon && v2.y >= p_lineHeight+Epsilon && v3.y >= p_lineHeight+Epsilon;
            bool tooLow = v1.y <= p_lineHeight-Epsilon && v2.y <= p_lineHeight-Epsilon && v3.y <= p_lineHeight-Epsilon;
            
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
        m_facade = (GameObject) ObjectField("Window Prefab",m_facade,typeof(object),true);
        m_facadeTop = (GameObject) ObjectField("Window Prefab Top",m_facadeTop,typeof(object),true);
        m_facadeBottom = (GameObject) ObjectField("Window Prefab Bottom",m_facadeBottom,typeof(object),true);

        if (m_facadeTop == null &&  m_facade != null) m_facadeTop = m_facade;
        if (m_facadeBottom == null &&m_facade != null) m_facadeBottom = m_facade;
        
        if(m_facade == null) return;

        m_assetWidth = FloatField("asset width", m_assetWidth);
        m_assetHeight = FloatField("asset height", m_assetHeight);
        

        if(m_selection==null) return;
        if (Button("\nCenter!\n"))
        {
            m_lineHeight = m_selection.GetComponent<MeshFilter>().sharedMesh.bounds.center.y * m_selection.transform.lossyScale.y;
        }

        if (Button("\nPlace Windows!\n"))
        {
            var facadesParent = new GameObject(){name =  "facades", tag = "HoudinAllRight Select Ignore"};
            facadesParent.transform.SetParent(m_selection.transform);
            //Undo.RegisterCreatedObjectUndo(facadesParent, "created parent");

            var middleParent = new GameObject(){name = "middle facades"};
            var topParent = new GameObject(){name = "top facades"};
            var bottomParent = new GameObject(){name = "bottom facades"};

            middleParent.transform.SetParent(facadesParent.transform);
            topParent.transform.SetParent(facadesParent.transform);
            bottomParent.transform.SetParent(facadesParent.transform);
            
            if(m_points == null) return;
            
            //foreach (Point point in m_points)
            for(int i = 0; i < m_points.Count;i ++)
            {
                Point point = m_points[i];
                GameObject prefab = m_facade;
                GameObject parent = middleParent;
                switch (point.facadeState)
                {
                    case FacadeState.top:
                        prefab = m_facadeTop;
                        parent = topParent;
                        break;
                    case FacadeState.bottom:
                        prefab = m_facadeBottom;
                        parent = bottomParent;
                        break;
                }
                
                GameObject window = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                
                if (window == null) continue;
                
                Transform windowTransform = window.transform;
                windowTransform.position = point.position;
                windowTransform.rotation = point.rotation;
                
                Vector3 scale = windowTransform.localScale;
                windowTransform.localScale = new Vector3(scale.x * point.width,scale.y,scale.z);

                if (parent != null) windowTransform.SetParent(parent.transform);
            }

            if (m_useCorners && m_corner != null)
            {
                GameObject cornerParent = new GameObject(){name = "corners"};
                cornerParent.transform.parent = facadesParent.transform;

                foreach (Point corner in m_corners)
                {
                    GameObject cornerPrefab = PrefabUtility.InstantiatePrefab(m_corner) as GameObject;

                    if (cornerPrefab == null) continue;
                
                    Transform cornerTransform = cornerPrefab.transform;
                    cornerTransform.position = corner.position + Vector3.up * m_cornerOffset;
                    cornerTransform.rotation = corner.rotation;
                    if(cornerParent!= null)cornerTransform.SetParent(cornerParent.transform);
                }
            }
        }

        m_useCorners = Toggle("Place corners ?", m_useCorners);
        
        Space();
        m_previewRotationOffset = Vector3Field("Preview Offset", m_previewRotationOffset);
        Space();
        
        if(!m_useCorners) return;
        
        m_corner = (GameObject)ObjectField("corner prefab", m_corner, typeof(object), true);
        m_cornerOffset = FloatField("corner offset", m_cornerOffset);
    }
}
