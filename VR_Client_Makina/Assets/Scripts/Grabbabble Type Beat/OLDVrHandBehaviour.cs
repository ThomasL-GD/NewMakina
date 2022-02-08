#if false

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
}

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class OLDVrHandBehaviour : MonoBehaviour {

    [SerializeField] private Hands m_hand = Hands.Left;
    [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
    [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerGrabSensitivity = 0.2f;
    [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerLetGoSensitivity = 0.9f;

    private bool m_isPressed = false;

    //public List<GrabbableObject> m_objectsInRange = new List<GrabbableObject>();
    /*[HideInInspector]/**/ public List<GrabbableObject> m_objectsGrabbed = new List<GrabbableObject>();

    private Animator m_animator = null;
    private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");

    private void Start() {
        SynchronizeBeacons.OnNewBeacon += Subscribe;
        if (m_triggerGrabSensitivity < m_triggerLetGoSensitivity) Debug.LogWarning("What the fuck is that balancing ?!", this);

        if (TryGetComponent(out Animator animator)) {
            m_animator = animator;
        }
    }

    private void OnTriggerStay(Collider p_other) { //TODO Oh My God ! this is so old and broken plz fix this at some pôint

        GameObject other = p_other.gameObject;

        if (other.layer == 6 && other.TryGetComponent(out GrabbableObject script)) { // The Catchable Object layer
            
            //Debug.Log("Good old debug");

            if (!m_isPressed && OVRInput.Get(m_grabInput) >= m_triggerGrabSensitivity) { // If the player is pressing the trigger hard enough
                m_isPressed = true;

                if (!script.m_isGrabbable) return;

                m_objectsGrabbed.Add(script);
                script.m_originalParent.Add(transform);
                script.BeGrabbed();
                if(m_animator != null)m_animator.SetBool(IsGrabbing, true);
            }
            
            if (m_isPressed && OVRInput.Get(m_grabInput) < m_triggerLetGoSensitivity) { //If the player let go enough
                
                m_isPressed = false;

                m_objectsGrabbed.Remove(script);
                script.m_originalParent.Remove(transform);
                script.BeGrabbed();
                if(m_animator != null)m_animator.SetBool(IsGrabbing, false);
            }
        }
    }

    private void OnTriggerExit(Collider p_other) {

        GameObject other = p_other.gameObject;
        
        if(other.transform.parent == transform) return;
        
        // Checking if it's in the Catchable Object layer and has a GrabbableObject component
        if (other.layer == 6 && other.TryGetComponent(out GrabbableObject script)) { 
        
            for (int i = 0; i < m_objectsGrabbed.Count; i++) {
                if (m_objectsGrabbed[i] == script) {
                    m_objectsGrabbed.Remove(script);
                    script.m_originalParent.Remove(transform);
                    script.BeGrabbed();
                    return;
                }
            }
        }
    }

    /// <summary/> Will remove a catchable object from this hand, so it doesn't exist anymore for this object
    /// <param name="p_grabbableObject">The script of the object you want to remove</param>
    private void RemoveObjectFromExistence(GrabbableObject p_grabbableObject) {
        p_grabbableObject.OnDestroyGrabbable -= RemoveObjectFromExistence;
        
        for (int i = 0; i < m_objectsGrabbed.Count; i++) {
            if (m_objectsGrabbed[i] == p_grabbableObject) m_objectsGrabbed.RemoveAt(i);
        }
    }

    /// <summary>
    /// Will subscribe to the OnDestroyCatchable delegate from a CatchableObject
    /// </summary>
    /// <param name="p_grabbableObject">The script from which we will subscribe</param>
    private void Subscribe(GrabbableObject p_grabbableObject) {
        p_grabbableObject.OnDestroyGrabbable += RemoveObjectFromExistence; //TODO : I think this whole system is outdated if objects cannot be destroyed in the vr hand
    }
}

#endif