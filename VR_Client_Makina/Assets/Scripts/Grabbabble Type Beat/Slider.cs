using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : GrabbableObject {

    [SerializeField] private Transform m_minimumPosition = null;
    [SerializeField] private Transform m_maximumPosition = null;
    
    //TODO : All of this script but only once the entire grab architecture will be renewed

    private void OnDrawGizmos() {
        if (m_maximumPosition != null && m_minimumPosition != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_minimumPosition.position, m_maximumPosition.position);
        }
    }
}
