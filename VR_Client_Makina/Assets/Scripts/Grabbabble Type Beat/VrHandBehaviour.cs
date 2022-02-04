using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class VrHandBehaviour : MonoBehaviour {
    
    [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
    [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerGrabSensitivity = 0.2f;
    //[SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerLetGoSensitivity = 0.9f;

    [CanBeNull] private GrabbableObject m_objectHeld = null;
    public bool isFree => m_objectHeld == null && m_isPressingTrigger;
    private bool m_isPressingTrigger = false;

    private Animator m_animator = null;
    private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");
    public static int s_layer = -1;

    private void Start() {
        
#if UNITY_EDITOR
        //if (m_triggerGrabSensitivity < m_triggerLetGoSensitivity) Debug.LogWarning("What the fuck is that balancing ?!", this);
        if (s_layer != -1 && s_layer != gameObject.layer) Debug.LogError($"Eyo ! Both hands have different layers ?! Or there is more than two hands ???? Anyway, i'll consider {gameObject.layer} as the last correct layer", this);
#endif
        
        s_layer = gameObject.layer;

        if (TryGetComponent(out Animator animator)) {
            m_animator = animator;
        }
    }

    private void Update() {
        m_isPressingTrigger = OVRInput.Get(m_grabInput) >= m_triggerGrabSensitivity; //if the trigger is pressed enough, the boolean becomes true
    }

    /// <summary>Will attach an object to this hand</summary>
    /// <param name="p_whoAmIEvenCatching">The object which is being grabbed by this hand</param>
    public void Catch(GrabbableObject p_whoAmIEvenCatching) {
        if(m_objectHeld != null) Debug.LogWarning($"The object {m_objectHeld.gameObject.name} is already in this hand but i'm supposed to grab {p_whoAmIEvenCatching.gameObject.name} so what do i do ?!", this);
        m_objectHeld = p_whoAmIEvenCatching;
        p_whoAmIEvenCatching.transform.SetParent(transform);
    }
}