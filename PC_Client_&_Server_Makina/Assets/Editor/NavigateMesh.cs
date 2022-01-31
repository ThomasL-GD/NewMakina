using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateMesh : MonoBehaviour
{
    [SerializeField] private float m_height;

    private void OnDrawGizmosSelected()
    {
        if(!TryGetComponent(out MeshFilter filter)) return;
        
        Mesh mesh = filter.sharedMesh;

        List<triangle> triangles = new List<triangle>();

        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;
        int[] tr = mesh.triangles;
        
        for (int i = 0; i < tr.Length;)
        {
            triangle triangle = new triangle(v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i++]], v[tr[i]], n[tr[i++]]);
            triangles.Add(triangle);
        }

        List<Vector3> intersections = new List<Vector3>();

        foreach (var triangle in triangles)
        {
            List<Vector3> localLine = new List<Vector3>();
            triangle.DrawHorizontalLineAlongMesh(transform.position,m_height,transform.rotation, transform.localScale, out localLine);
            foreach (var position in localLine)
            {
                bool canAdd = true;
                foreach (var intersection in intersections)
                {
                    if(intersection == position) canAdd = false;
                }
                if(canAdd)intersections.Add(position); 
            }
        }

        for (int i = 0; i < intersections.Count; i++)
        {
            for (int j = 0; j < intersections.Count; j++)
            {
                if(i != j &&intersections[i] == intersections[j])
                    intersections.RemoveAt(i);
            }
        }

        for (int i = 0; i < intersections.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(intersections[i], intersections[(i + 1)%intersections.Count]);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(intersections[i],0.1f);
        }
    }

    struct triangle
    {
        public Vector3 vertex1;
        public Vector3 vertex2;
        public Vector3 vertex3;
        public Vector3 normal;
        
        public triangle(Vector3 p_v1,Vector3 p_n1,Vector3 p_v2,Vector3 p_n2,Vector3 p_v3,Vector3 p_n3)
        {
            vertex1 = p_v1;
            vertex2 = p_v2;
            vertex3 = p_v3;
            normal = (p_n1 + p_n2 + p_n3) / 3;
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

        public void DrawHorizontalLineAlongMesh(Vector3 p_position, float p_lineHeight, Quaternion p_rotation, Vector3 p_scale, out List<Vector3> p_line)
        {
            p_line = new List<Vector3>();
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
                    p_line.Add(p_position + intersection);
                
                if (GetSegmentPlaneIntersection(v2, v3, out intersection, p_lineHeight))
                    p_line.Add(p_position + intersection);
                
                if (GetSegmentPlaneIntersection(v3, v1, out intersection, p_lineHeight))
                    p_line.Add(p_position + intersection);
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
