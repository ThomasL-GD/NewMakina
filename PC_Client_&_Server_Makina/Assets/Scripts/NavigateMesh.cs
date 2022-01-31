using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class NavigateMesh : MonoBehaviour
{
    [SerializeField] private float m_height;

    private void OnDrawGizmosSelected()
    {
        //Getting the mesh of the object
        if(!TryGetComponent(out MeshFilter filter)) return;
        Mesh mesh = filter.sharedMesh;

        // making a list of all the triangles of the mesh
        List<triangle> triangles = new List<triangle>();

        // Getting all of the triangles of the mesh 
        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;
        int[] tr = mesh.triangles;
        
        for (int i = 0; i < tr.Length;) 
            triangles.Add(new triangle(v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i]], tr[i++]));
        
        List<Intersections> intersections = new List<Intersections>();

        foreach (var triangle in triangles)
        {
            List<Intersections> localLine = new List<Intersections>();
            triangle.DrawHorizontalLineAlongMesh(transform.position,m_height,transform.rotation, transform.localScale, out localLine);
            foreach (var item in localLine)
            {
                bool canAdd = true;

                for (int i = 0; i < intersections.Count; i++)
                {
                   
                    if(intersections[i].position == item.position)
                    {
                        intersections[i].triangles.Add(item.triangles[0]);
                        canAdd = false;
                    }
                }
                if(canAdd)intersections.Add(item); 
            }
        }

        List<Link> links = new List<Link>();
        
        Gizmos.color = Color.red;
        
        for (int i = 0; i < intersections.Count; i++)
        {
            for (int j = i; j < intersections.Count; j++)
            {
                if(j==i) continue;
                for (int k = 0; k < intersections[i].triangles.Count; k++)
                {
                    for (int l = 0; l < intersections[j].triangles.Count; l++)
                    {
                        if(intersections[i].triangles[k] == intersections[j].triangles[l])
                            links.Add(new Link(intersections[i].position,intersections[j].position));
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
        
        Debug.Log(links.Count);

        foreach (var link in links) link.DrawLink();
    }

    class Link
    {
        public Vector3 A;
        public Vector3 B;

        public Link(Vector3 p_A, Vector3 p_B)
        {
            A = p_A;
            B = p_B;
        }

        public void DrawLink()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(A, B);
            Gizmos.color = Color.yellow;
            float radius = .1f;
            Gizmos.DrawWireSphere(A,radius);
            Gizmos.DrawWireSphere(B,radius);
        }
    }
    
    struct Intersections
    {
        public Vector3 position;
        public List<int> triangles;
    }
    
    struct triangle
    {
        public Vector3 vertex1;
        public Vector3 vertex2;
        public Vector3 vertex3;
        public Vector3 normal;
        public int triangleId;
        
        public triangle(Vector3 p_v1,Vector3 p_n1,Vector3 p_v2,Vector3 p_n2,Vector3 p_v3,Vector3 p_n3, int id)
        {
            vertex1 = p_v1;
            vertex2 = p_v2;
            vertex3 = p_v3;
            normal = (p_n1 + p_n2 + p_n3) / 3;
            triangleId = id;
        }

        private void DrawFace(Vector3 p_position, Camera p_cam, Quaternion p_rotation, Vector3 p_scale)
        {
            Vector3 v1 = Vector3.Scale(p_rotation * vertex1, p_scale);
            Vector3 v2 = Vector3.Scale(p_rotation * vertex2, p_scale);
            Vector3 v3 = Vector3.Scale(p_rotation * vertex3, p_scale);

            if ((CheckIfInCameraView(p_cam, p_position + v1) && CheckIfInCameraView(p_cam, p_position + v2)
                                                             && CheckIfInCameraView(p_cam, p_position + v3))) return;
            Gizmos.color = Color.blue;

            Gizmos.DrawLine(p_position + v1,p_position + v2);
            Gizmos.DrawLine(p_position + v2,p_position + v3);
            Gizmos.DrawLine(p_position + v3,p_position + v1);

            Color color = Gizmos.color;
            color.a = .5f;
            Gizmos.color = color;
            Mesh face = new Mesh();
            face.vertices = new[] {v1,v2,v3};
            face.triangles = new[] {0,1,2};
            face.normals = new[] {normal,normal,normal};
            
            Gizmos.DrawMesh(face,p_position + -Vector3.Scale(p_cam.transform.forward * .01f, p_scale));
        }

        public void DrawHorizontalLineAlongMesh(Vector3 p_position, float p_lineHeight, Quaternion p_rotation, Vector3 p_scale, out List<Intersections> p_intersections)
        {
            p_intersections = new List<Intersections>();
            Vector3 v1 = Vector3.Scale(p_rotation * vertex1, p_scale);
            Vector3 v2 = Vector3.Scale(p_rotation * vertex2, p_scale);
            Vector3 v3 = Vector3.Scale(p_rotation * vertex3, p_scale);
            
            bool tooHigh = v1.y >= p_lineHeight && v2.y >= p_lineHeight && v3.y >= p_lineHeight;
            bool tooLow = v1.y <= p_lineHeight && v2.y <= p_lineHeight && v3.y <= p_lineHeight;
            
            if (!tooHigh && !tooLow)
            {
                DrawFace(p_position,Camera.current,p_rotation,p_scale);

                Vector3 intersection;
                if (GetSegmentPlaneIntersection(v1, v2, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} });
                }
                    
                
                if (GetSegmentPlaneIntersection(v2, v3, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} });
                }
                
                if (GetSegmentPlaneIntersection(v3, v1, out intersection, p_lineHeight))
                {
                    intersection += p_position;
                    p_intersections.Add(new Intersections(){position = intersection, triangles = new List<int>(){triangleId} });
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
}
