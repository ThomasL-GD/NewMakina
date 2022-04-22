using Grabbabble_Type_Beat;
using UnityEngine;

public abstract class Slider<T> : GrabbableObject {

    [SerializeField] private Transform m_minimumPosition = null;
    [SerializeField] private Transform m_maximumPosition = null;
    
    public delegate T GetValueDelegator();
    public delegate void SetValueDelegator(T p_value);
    
    //TODO : All of this script but only once the entire grab architecture will be renewed

    private void OnDrawGizmos() {
        if (m_maximumPosition != null && m_minimumPosition != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_minimumPosition.position, m_maximumPosition.position);
        }
    }
}
