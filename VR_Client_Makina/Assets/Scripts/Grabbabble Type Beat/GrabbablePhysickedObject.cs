using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody))]
public abstract class GrabbablePhysickedObject : GrabbableObject {

    private bool m_hasTouchedGround = false;

    [HideInInspector] public bool m_dropDown = false;

    /// <summary> The layers this object will collide with, only layer 8 (ground) is selected by default </summary>
    protected LayerMask m_layersThatCollides = 1 << 8;

    /// <summary>Well, it was a transform but... err... I kinda amputated it so... it's just a position and rotation sticked together now... (￣ー￣; )ゞ </summary>
    [Serializable]
    protected struct AmputatedTransform {
        public Vector3 position;
        public Quaternion rotation;

        /// <summary/>Will simply amputate a transform to have an amputated one lol
        /// <param name="p_transform"/>The transform you want to amputate
        /// <returns/>The amputated transform
        public static AmputatedTransform AmputateTransform(Transform p_transform) {
            return new AmputatedTransform() {position = p_transform.position, rotation = p_transform.rotation};
        }
    }

    [SerializeField] [Range(1, 50)] [Tooltip("The number of values used to calculate the direction of the throw.\nUnit : values, can be seen as 1 value = 1 frame")]/**/ private int m_throwValuesNumber = 3;

    /// <summary/>The last positions & rotations of this object, its length depends on m_throwValuesNumber
    protected AmputatedTransform[] m_lastCoordinates = null;
    
    protected Rigidbody m_rb = null;
    
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        m_originalParent[0] = null;

        m_rb = GetComponent<Rigidbody>();
        m_lastCoordinates = new AmputatedTransform[m_throwValuesNumber];
    }
    
    public override void ActualiseParent() {
        base.ActualiseParent();

        if (m_isCaught || (!m_isCaught && !m_hasBeenCaughtInLifetime)) { //If it's either caught or spawned but never has been caught
            m_rb.isKinematic = true;
        }
        else { //If the item is let go
            m_rb.isKinematic = false;
            
            if(m_dropDown) return; // Watch out, this line can kill your code if you put line afterwards
                
            m_rb.velocity = (m_lastCoordinates[0].position - m_lastCoordinates[m_lastCoordinates.Length - 1].position) * ((1 / Time.fixedDeltaTime) / m_lastCoordinates.Length);
            //m_rb.angularVelocity += m_lastCoordinates[0].rotation.eulerAngles - m_lastCoordinates[m_lastCoordinates.Length - 1].rotation.eulerAngles;
            // m_rb.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            m_rb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
            
            //Debug.Log(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch));
        }
    }

    private void FixedUpdate() {
        
        //Will move every value to the next index
        for (int i = m_lastCoordinates.Length - 1; i > 0; i--) {
            m_lastCoordinates[i] = m_lastCoordinates[i - 1];
        }
    
        //Then, we store the last position & rotation of this object
        m_lastCoordinates[0] = AmputatedTransform.AmputateTransform(transform);
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
    }
}