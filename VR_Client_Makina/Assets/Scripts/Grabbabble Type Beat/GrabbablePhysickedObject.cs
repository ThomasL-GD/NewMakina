using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody))]
public abstract class GrabbablePhysickedObject : GrabbableObject {

    [Header("Physics")]
    [SerializeField] [Range(1f, 1000f)] private float m_throwMultiplier;
    [SerializeField] [Range(1f, 1000f)] private float m_gravityMultiplier;

    private bool m_hasTouchedGround = false;

    //Dropdown type beat
    [HideInInspector] public bool m_mustDropDown = false;
    [HideInInspector] public GameObject m_prefabDropDownFeedback;
    private GameObject m_lineFeedback = null;

    /// <summary> The layers this object will collide with, only layer 8 (ground) is selected by default </summary>
    [SerializeField] protected LayerMask m_layersThatCollides = 1 << 8;
    
    protected Rigidbody m_rb = null;
    
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        m_rb = GetComponent<Rigidbody>();
    }
    
    public override void BeGrabbed(Transform p_parent, Vector3 p_offsetPositionInHand) {
        base.BeGrabbed(p_parent,p_offsetPositionInHand);

        if (m_mustDropDown && m_isCaught) {
            if (m_lineFeedback != null) {
                Debug.LogError("Wait ! You're gonna instantiate an object that already exist");
                m_lineFeedback.GetComponent<DropDownFeedback>().DestroyMe();
                m_lineFeedback = null;
            }

            m_lineFeedback = Instantiate(m_prefabDropDownFeedback, transform);
            
        }

        if (m_isCaught) {
            m_rb.isKinematic = true;
        }
    }

    public override void BeLetGo(OVRInput.Axis1D p_handInput) {
        base.BeLetGo(p_handInput);
        
        m_rb.isKinematic = false;
        
        if(m_mustDropDown) return; // Watch out, this line can kill your code if you put line afterwards

        OVRInput.Controller hand = OVRInput.Controller.None;
            
        //m_rb.velocity = (m_lastCoordinates[0].position - m_lastCoordinates[m_lastCoordinates.Length - 1].position) * ((1 / Time.fixedDeltaTime) / m_lastCoordinates.Length);
        //m_rb.angularVelocity += m_lastCoordinates[0].rotation.eulerAngles - m_lastCoordinates[m_lastCoordinates.Length - 1].rotation.eulerAngles;
        if (p_handInput.HasFlag(OVRInput.Axis1D.PrimaryHandTrigger) || p_handInput.HasFlag(OVRInput.Axis1D.PrimaryIndexTrigger)) {
            hand |= OVRInput.Controller.LTouch;
        }
        if (p_handInput.HasFlag(OVRInput.Axis1D.SecondaryHandTrigger) || p_handInput.HasFlag(OVRInput.Axis1D.SecondaryIndexTrigger)) {
            hand |= OVRInput.Controller.RTouch;
        }
        //Debug.LogWarning($"Theoretical velocity : {OVRInput.GetLocalControllerVelocity(hand)}");
        m_rb.velocity = OVRInput.GetLocalControllerVelocity(hand) * m_throwMultiplier;
        m_rb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(hand);
        
        //Debug.Log(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch));
            
    }

    private void OnCollisionEnter(Collision p_other) {
        if (((1 << p_other.gameObject.layer) & m_layersThatCollides) != 0) { //If this object collides with any layer specified with m_layersThatCollides
            transform.rotation = Quaternion.Euler(0,0,0);
            if(!m_hasTouchedGround)OnFirstTimeTouchingGround(p_other);
        }
    }

    /// <summary> Is called the first time this object touches the ground </summary>
    /// <remarks> Override this to have things done at this moment </remarks>
    protected virtual void OnFirstTimeTouchingGround(Collision p_other) {
        m_hasTouchedGround = true;

        if (m_mustDropDown) {
            m_lineFeedback.GetComponent<DropDownFeedback>().DestroyMe();
            m_lineFeedback = null;
        }
    }

    private void FixedUpdate() {
        m_rb.AddForce(Physics.gravity * m_gravityMultiplier);
    }
}