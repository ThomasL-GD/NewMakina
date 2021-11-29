using System;
using System.Collections.Generic;
using CatchThoseHands;
using Synchronizers;
using UnityEngine;

namespace CatchThoseHands {
    public enum Hands : int { // Watch out ! The CatchableObject abstract class depends on this, read it before messing with this plz ح(-̀ж-́)ง
        Left = 0,
        Right = 1
    }

    public struct HandRelationship {
        public Transform handTransform;
        public bool isGrabbedByThis;
    }
}

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class VrHandBehaviour : MonoBehaviour {

    [SerializeField] private Hands m_hand = Hands.Left;
    [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
    [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerSensitivity = 0.7f;

    private bool m_isPressed = false;

    public List<GrabbableObject> m_objectsInRange = new List<GrabbableObject>();
    public List<GrabbableObject> m_objectsGrabbed = new List<GrabbableObject>();


    private void Start() {
        SynchronizeBeacons.OnNewBeacon += Subscribe;
    }

    // Update is called once per frame
    void Update() {
        
        if (!m_isPressed && OVRInput.Get(m_grabInput) >= m_triggerSensitivity) { // If the player press the trigger hard enough
            m_isPressed = true;
            
            foreach (GrabbableObject script in m_objectsInRange) {
                if(!script.m_isGrabbable) continue;
                
                m_objectsGrabbed.Add(script);
                script.m_originalParent.Add(transform);
                script.ActualiseParent();
            }
        }
        else if (m_isPressed && OVRInput.Get(m_grabInput) < m_triggerSensitivity) { //If the player let go enough
            m_isPressed = false;
            
            foreach (GrabbableObject script in m_objectsInRange) {
                m_objectsGrabbed.Remove(script);
                script.m_originalParent.Remove(transform);
                script.ActualiseParent();
            }
        }
    }

    private void OnTriggerEnter(Collider p_other) {
        if (p_other.gameObject.layer == 6 && p_other.gameObject.TryGetComponent(out GrabbableObject script)) { // The Catchable Object layer
            
            m_objectsInRange.Add(script);
        }
    }

    private void OnTriggerExit(Collider p_other) {
        if (p_other.gameObject.layer == 6 && p_other.gameObject.TryGetComponent(out GrabbableObject script)) { // The Catchable Object layer
            
            m_objectsInRange.Remove(script);
        }
    }

    /// <summary>
    /// Will remove a catchable object from this hand, so it doesn't exist anymore for this object
    /// </summary>
    /// <param name="p_grabbableObject">The script of the object you want to remove</param>
    private void RemoveObjectFromExistence(GrabbableObject p_grabbableObject) {
        p_grabbableObject.OnDestroyGrabbable -= RemoveObjectFromExistence;
        Debug.Log($"My list is : {m_objectsInRange} have {m_objectsInRange.Count} cells");

        for (int i = 0; i < m_objectsInRange.Count; i++) {
            if (m_objectsInRange[i] == p_grabbableObject) m_objectsInRange.RemoveAt(i);
        }
        
        for (int i = 0; i < m_objectsGrabbed.Count; i++) {
            if (m_objectsGrabbed[i] == p_grabbableObject) m_objectsGrabbed.RemoveAt(i);
        }
    }

    /// <summary>
    /// Will subscribe to the OnDestroyCatchable delegate from a CatchableObject
    /// </summary>
    /// <param name="p_grabbableObject">The script from which we will subscribe</param>
    private void Subscribe(GrabbableObject p_grabbableObject) {
        p_grabbableObject.OnDestroyGrabbable += RemoveObjectFromExistence;
    }
}