using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecenterVrRig : MonoBehaviour {

    [SerializeField] private Transform m_anchor = null;
    [HideInInspector] public Vector3 m_startPos = Vector3.zero;

    private void OnEnable() {
        m_startPos = transform.position;
    }

    // Update is called once per frame
    void Update() {

        if (OVRInput.GetDown(OVRInput.Button.Two) && m_anchor != null) {
            transform.position -= m_anchor.position;
            transform.position += m_startPos;
        }
        
    }
}
