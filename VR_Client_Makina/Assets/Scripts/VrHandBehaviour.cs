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

    public List<CatchableObject> m_objectsInRange = new List<CatchableObject>();
    public List<CatchableObject> m_objectsGrabbed = new List<CatchableObject>();


    private void Start() {
        SynchronizeBeacons.OnNewBeacon += Subscribe;
    }

    // Update is called once per frame
    void Update() {
        
        if (!m_isPressed && OVRInput.Get(m_grabInput) >= m_triggerSensitivity) { // If the player press the trigger hard enough
            m_isPressed = true;
            
            foreach (CatchableObject script in m_objectsInRange) {
                if(!script.m_isGrabable) continue;
                
                m_objectsGrabbed.Add(script);
                script.m_originalParent.Add(transform);
                script.ActualiseParent();
            }
        }
        else if (m_isPressed && OVRInput.Get(m_grabInput) < m_triggerSensitivity) { //If the player let go enough
            m_isPressed = false;
            
            foreach (CatchableObject script in m_objectsInRange) {
                m_objectsGrabbed.Remove(script);
                script.m_originalParent.Remove(transform);
                script.ActualiseParent();
            }
        }
    }

    private void OnTriggerEnter(Collider p_other) {
        if (p_other.gameObject.layer == 6 && p_other.gameObject.TryGetComponent(out CatchableObject script)) { // The Catchable Object layer
            
            m_objectsInRange.Add(script);
        }
    }

    private void OnTriggerExit(Collider p_other) {
        if (p_other.gameObject.layer == 6 && p_other.gameObject.TryGetComponent(out CatchableObject script)) { // The Catchable Object layer
            
            m_objectsInRange.Remove(script);
        }
    }

    /// <summary>
    /// Will remove a catchable object from this hand, so it doesn't exist anymore for this object
    /// </summary>
    /// <param name="p_catchableObject">The script of the object you want to remove</param>
    private void RemoveObjectFromExistence(CatchableObject p_catchableObject) {
        p_catchableObject.OnDestroyCatchable -= RemoveObjectFromExistence;
        Debug.Log($"My list is : {m_objectsInRange} have {m_objectsInRange.Count} cells");

        for (int i = 0; i < m_objectsInRange.Count; i++) {
            if (m_objectsInRange[i] == p_catchableObject) m_objectsInRange.RemoveAt(i);
        }
        
        for (int i = 0; i < m_objectsGrabbed.Count; i++) {
            if (m_objectsGrabbed[i] == p_catchableObject) m_objectsGrabbed.RemoveAt(i);
        }
    }

    /// <summary>
    /// Will subscribe to the OnDestroyCatchable delegate from a CatchableObject
    /// </summary>
    /// <param name="p_catchableObject">The script from which we will subscribe</param>
    private void Subscribe(CatchableObject p_catchableObject) {
        p_catchableObject.OnDestroyCatchable += RemoveObjectFromExistence;
    }
}