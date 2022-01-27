using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DropDownFeedback : MonoBehaviour {
    private LineRenderer m_lineRenderer = null;
    private GameObject m_targetGO = null;

    private void Start() {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_lineRenderer.useWorldSpace = true;

        m_targetGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_targetGO.GetComponent<MeshRenderer>().material.color = m_lineRenderer.endColor;
        Destroy(m_targetGO.GetComponent<SphereCollider>());
        AnimationCurve widthCurve = m_lineRenderer.widthCurve;
        m_targetGO.transform.localScale = Vector3.one * m_lineRenderer.endWidth * 2f;
    }

    private void Update() {
        
        transform.localPosition = Vector3.zero;
        
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 100000f, ~(1 << 3));

        Vector3 endline;
        if (hasHit) {
            endline = hitInfo.point;
        }
        else {
            endline = transform.position + Vector3.down * 100000f;
        }
        
        // Debug.DrawLine(transform.position, endline, Color.magenta);
        // Debug.Log($"positions : {transform.position}     &     {endline}");
        
        m_lineRenderer.SetPositions(new Vector3[2]{transform.position, endline});
        m_targetGO.transform.position = endline;
    }

    /// <summary> Needs to be called to destroy this type of object
    /// Don't use the basic Destroy function otherwise the sphere will subsist</summary>
    public void DestroyMe() {
        Destroy(m_targetGO);
        Destroy(gameObject);
    }
}