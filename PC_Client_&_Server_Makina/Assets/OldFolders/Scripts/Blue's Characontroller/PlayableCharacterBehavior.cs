using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacterBehavior : MonoBehaviour {

    [Header("Movements On Ground")]
    [SerializeField] [Range(50f, 1000f)] private float m_maxVelocityOnGround = 100f;
    [SerializeField] [Range(1f, 500f)] private float m_accelerationOnGround = 10f;

    [Header("Sprint")]
    [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
    [SerializeField] [Range(50f, 1000f)] private float m_maxVelocitySprinting = 100f;
    [SerializeField] [Range(1f, 500f)] private float m_accelerationSprinting = 10f;
    
    [Header("Movements Midair")]
    [SerializeField] [Range(50f, 1000f)] private float m_maxVelocityMidair = 100f;
    [SerializeField] [Range(1f, 500f)] private float m_accelerationMidair = 10f;
    
    [Header("Jump")]
    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] [Range(1f, 500f)] private float m_jumpHeight = 2f;
    [SerializeField] [Range(0.01f, 1f)] private float m_groundDetectionLength = 0.2f;
    [SerializeField] [Range(0.01f, 1f)] private float m_coyoteTimeLol = 0.2f;
    [SerializeField] [Range(0.01f, 1f)] private float m_jumpKeyConservationTime = 0.2f;
    private bool m_hasPressedJumpRecently = false;
    
    [SerializeField] [Tooltip("For debug only.")] private bool m_isOnGround = false;
    //TODO : Find a better way to do it without several global booleans would be great, maybe a bitmask enum ?
    [SerializeField] [Tooltip("For debug only.")] private bool m_isDetectingGround = false;
    [SerializeField] [Tooltip("For debug only.")] private bool m_isCoyoting = false; //It is used to be sure GroundDetectionDelay() is called only once 
    
    [Header("Edge Grab")]
    [SerializeField] [Range(1f, 180f)] private float m_grabAngle = 80f;
    
    [Header("Physicsssssssssssssssssssssss")]
    [SerializeField] [Range(1f, 500f)] private float m_gravityStrength = 9.81f;
    [SerializeField] [Range(0f, 1f)] private float m_minimumDrag = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float m_maximumDrag = 1f;
    

    [Header("DeBuG   щ（ﾟДﾟщ）")]
    [SerializeField] [Tooltip("For debug only.")] private Vector3 m_velocity = Vector3.zero;
    [SerializeField] [Tooltip("For debug only.")] private float m_drag = 1f; // m_drag is a value between 0 and 1
    [SerializeField] [Tooltip("For debug only.")] private Vector2 m_inputDirection = Vector2.zero;
    
    [SerializeField] [Tooltip("For debug only.")] private float m_magnitude = 0;
    [SerializeField] [Tooltip("For debug only.")] private float m_delta = 0;
    [SerializeField] [Tooltip("For debug only.")] private Vector3 m_acc = Vector3.zero;
    [SerializeField] [Tooltip("For debug only.")] private float m_accMagn = 0;

    private CharacterController m_charaController = null;
    void Start() {
        m_charaController = GetComponent<CharacterController>();
        gameObject.layer = 12; // The playable character layer, is used to avoid our own raycasts to hit ourselves
    }

    // Update is called once per frame
    void Update() {

        #region ground detection

        RaycastHit hitSphere;
        m_isDetectingGround = Physics.SphereCast(transform.position + (-transform.up * (m_charaController.height/2 - m_charaController.radius)) /*The bottom of this chara*/, m_charaController.radius, Vector3.down, out hitSphere, m_groundDetectionLength, ~(1 << 12)/*This means it detects any layer except the 12th one*/);

        if (m_isDetectingGround) { //If we detect the ground, we are on it and not coyoting
            m_isOnGround = true;
            m_isCoyoting = false;
        }
        else if (!m_isDetectingGround && m_isOnGround && !m_isCoyoting && m_velocity.y < 0) { //Is true the frame when we detected the ground but not anymore and we are going down
            m_isCoyoting = true;
            StartCoroutine(GroundDetectionDelay());
        }
        else if(m_velocity.y > 0) m_isOnGround = false;
        
        #endregion

        
        #region acceleration calculation

        //We use the appropriate values depending if the player is on ground or not
        float accelerationPossible;
        float maxVelocity;
        if (m_isDetectingGround) {
            if (Input.GetKey(m_sprintKey)) {
                accelerationPossible = m_accelerationSprinting;
                maxVelocity = m_maxVelocitySprinting;
            }
            else{
                accelerationPossible = m_accelerationOnGround;
                maxVelocity = m_maxVelocityOnGround;
            }
        }
        else{
            accelerationPossible = m_accelerationMidair;
            maxVelocity = m_maxVelocityMidair;
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        //We create a Vector3 that contains the direction and strength of the input
        Vector3 acceleration = transform.right * (inputX);
        acceleration += transform.forward * (inputY);

        acceleration = Vector3.ClampMagnitude(acceleration, 1f); //This clamp is used in case of diagonal direction, so any analogical directional input will have a max strength of 1 (-̀ᴗ-́)و 
        m_inputDirection = new Vector2(acceleration.x, acceleration.z).normalized; // We just attribute this value to use it in the FixedUpdate for the drag calculation

        acceleration *= accelerationPossible; // We multiply the current acceleration (magnitude between 0 and 1) by the acceleration strength that we have in serialized field
        
        //This is the calculation of the remaining acceleration needed to reach the max acceleration
        float delta = Mathf.Clamp(maxVelocity - (new Vector2(m_velocity.x, m_velocity.z) + (new Vector2(acceleration.x, acceleration.z) * Time.deltaTime)).magnitude,0f,Mathf.Infinity);
        m_delta = delta;

        acceleration *= Time.deltaTime;
        acceleration = Vector3.ClampMagnitude(acceleration, delta);
        m_accMagn = acceleration.magnitude / Time.deltaTime;

        #endregion

        
        #region slope adaptation

        // project on plane
        if (m_isDetectingGround) {
            RaycastHit hitRay;
            bool isRaycastHitting = Physics.Raycast(transform.position, Vector3.down, out hitRay,m_charaController.height / 2 + m_groundDetectionLength, ~(1 << 12));

            if (!isRaycastHitting) hitRay = hitSphere; // If the raycast did not hit, we use the sphereCast data
            
            acceleration = Vector3.ProjectOnPlane(acceleration, hitRay.normal);
            Debug.Log(hitRay.normal);
            m_acc = acceleration;
        }

        #endregion


        #region gravity

        if (!m_isDetectingGround) m_velocity += Vector3.down * m_gravityStrength * Time.deltaTime;
        else if (m_velocity.y < 0) m_velocity.y *= 0.99f; //If we touch the ground, we remove any gravity leftovers

        #endregion

        
        #region jump

        if (Input.GetKeyDown(m_jumpKey)) {
            m_hasPressedJumpRecently = true;
            StopCoroutine(JumpKeyConservation()); // We stop the coroutine before starting a new one in case one was still running (I honestly don't know if it truly works like that but the API told me so ¯\_(ツ)_/¯ 
            StartCoroutine(JumpKeyConservation());
        }

        if (m_hasPressedJumpRecently && (m_isDetectingGround || m_isCoyoting)) { //TODO this ain't working, we can make several jumps in a row
            m_hasPressedJumpRecently = false;
            acceleration += Vector3.up * m_jumpHeight; //TODO Make math for m_jumpHeight to be actually equal to the height jump
        }
        
        #endregion
        
        
        
        // We add the calculated acceleration of this frame to the velocity before applying it ⊂(◉‿◉)つ 
        m_velocity += acceleration;
         
        //Velocity appliance__________________________________________________________ヽ༼ ಠ益ಠ ༽ﾉ THE LINE BELOW IS WHERE EVERYTHING COMES TO SENSE ヽ༼ ಠ益ಠ ༽ﾉ____________________________________________________________________________
        m_charaController.Move(m_velocity * Time.deltaTime);
        m_magnitude = m_velocity.magnitude;
    }

    //TODO Get this out of the fixed update
    private void FixedUpdate() {
        Vector2 planarVelocity = new Vector2(m_velocity.x, m_velocity.z);
        m_drag = Mathf.Pow((Vector2.Dot(planarVelocity.normalized, m_inputDirection.normalized) + 1f)/2f, .5f);

        m_drag = Mathf.Lerp(m_minimumDrag, m_maximumDrag, m_drag);

        m_velocity.x *= m_drag; // m_drag is a value between 0 and 1
        m_velocity.z *= m_drag; // m_drag is a value between 0 and 1
    }

    /// <summary>
    /// Is used to tell the code the character is still touching the ground for a few amount of time after we stopped touching the ground
    /// </summary>
    IEnumerator GroundDetectionDelay() {
        
        yield return new WaitForSeconds(m_coyoteTimeLol);

        if(!m_isDetectingGround)m_isOnGround = false;
        m_isCoyoting = false;
    }

    /// <summary>
    /// Is used to tell the code the jump key is pressed for a little more time than one frame
    /// </summary>
    IEnumerator JumpKeyConservation() {
        
        yield return new WaitForSeconds(m_jumpKeyConservationTime);

        if (!Input.GetKeyDown(m_jumpKey)) m_hasPressedJumpRecently = false;
    }


    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            m_charaController = GetComponent<CharacterController>();
        
            Gizmos.DrawSphere(transform.position + (-transform.up * (m_charaController.height/2 - m_charaController.radius)), m_charaController.radius);
            Gizmos.DrawSphere(transform.position + (-transform.up * ((m_charaController.height/2 - m_charaController.radius) + m_groundDetectionLength)), m_charaController.radius);
            
            Debug.DrawRay(transform.position, m_acc * 500f, Color.magenta);
            Debug.DrawRay(transform.position, m_velocity * 500f, Color.green);
            
        }
    }
}