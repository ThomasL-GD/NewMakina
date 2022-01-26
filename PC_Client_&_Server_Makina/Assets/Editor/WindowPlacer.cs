using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Window Placer Tool")]
public class WindowPlacer : EditorTool
{
    [SerializeField] Texture2D m_ToolIcon;
    [SerializeField] GameObject m_prefab;
    [SerializeField] float m_offset;

    
    GUIContent m_IconContent;
    
    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "Window Placer Tool",
            tooltip = "Window Placer Tool"
        };
    }
    
    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }
    
    // This is called for each window that your tool is active in. Put the functionality of your tool here.
    public override void OnToolGUI(EditorWindow window)
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
