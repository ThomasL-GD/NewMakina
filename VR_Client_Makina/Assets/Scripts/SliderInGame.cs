using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public /*abstract*/ class SliderInGame/*<T>*/ : MonoBehaviour {

    [SerializeField] private Transform m_minimumPosition = null;
    [SerializeField] private Transform m_maximumPosition = null;
    /*
    public delegate T GetValueDelegator();
    public delegate void SetValueDelegator(T p_value);
    */
    private List<VrHandBehaviour> m_handsInContact = new List<VrHandBehaviour>();

    private void OnDrawGizmos() {
        if (m_maximumPosition != null && m_minimumPosition != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_minimumPosition.position, m_maximumPosition.position);
        }
    }

    private void Update() {
        
        if (m_handsInContact.Count < 1) return;

        foreach (VrHandBehaviour hand in m_handsInContact) {
            if (hand.m_isPressingTrigger) {
                Vector3 minPos = m_minimumPosition.position;
                Vector3 maxPos = m_maximumPosition.position;
                Vector3 line = maxPos - minPos;
                transform.position = (Vector3.Dot(line, (hand.transform.position - minPos))) * line + minPos;
            }
        }
    }

    private void OnTriggerEnter(Collider p_other) {
        if (p_other.gameObject.layer == VrHandBehaviour.s_layer && p_other.TryGetComponent(out VrHandBehaviour script)) {
            m_handsInContact.Add(script);
        }
    }

    private void OnTriggerExit(Collider p_other) {
        if (p_other.gameObject.layer == VrHandBehaviour.s_layer && p_other.TryGetComponent(out VrHandBehaviour script)) {
            m_handsInContact.Remove(script);
        }
    }
}
