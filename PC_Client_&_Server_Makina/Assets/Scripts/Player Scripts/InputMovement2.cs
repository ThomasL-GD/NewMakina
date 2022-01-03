using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class InputMovement2 : MonoBehaviour
{
    public float s_speed = 0;
    [Space,Header("Movement Stuff")]
    [SerializeField] float m_movementSpeed = 8.0f;
    [SerializeField,Range(0f, 0.3f),Tooltip("the time to transition smoothly between the player input direction")] float m_moveSmoothTimeGrounded = 0.3f;
    [SerializeField,Range(0f, 2f),Tooltip("the time to transition smoothly between the player input direction")] float m_slideSmoothTimeUp = 0.3f;
    [SerializeField,Range(0f, 2f),Tooltip("the time to transition smoothly between the player input direction")] float m_slideSmoothTimeDown = 0.3f;
    [SerializeField, Tooltip("The m_gravity independent from weight or volume applied to the player in m/s²")] float m_gravity = -9.18f;

    /// <summary/> the current pitch of the player's first person camera
    private float m_cameraPicth = 0f;
    
    /// <summary/> The current y velocity applied to the player
    float m_gVelocity = 0.0f;
    
    /// <summary/> The current Direction the input movement will push the player in
    Vector2 m_currentInputDir = Vector2.zero;
    
    /// <summary/> The current velocity of the player's movement
    Vector2 m_currentInputDirVelocity = Vector2.zero;
    
    /// <summary/> The current Direction of the player's sliding movement
    Vector3 m_slideDirection = Vector3.zero;
    
    /// <summary/> The current velocity of the player's sliding movement
    private Vector3 m_slideDirectionVelocity;
    
    [Space,Header("camera stuff"),SerializeField, Tooltip("the sensitivity of the mouse"), Range(0f,15f)]
    private float m_mouseSensitivity = 4f;
    [SerializeField, Tooltip("The camera transform (not the parent)")] private Transform m_cameraTr = null;
    [Space,SerializeField, Tooltip("The camera parent transform")] private Transform m_cameraParentTr = null;

    [Space, Header("Jump Stuff")] [SerializeField, Tooltip("the height at which the player will jump in m"), Min(0.0f)] private float m_playerJumpHeight = .5f;

    [SerializeField, Tooltip("The range at which the ground will be detected")] private float m_groundCheckRange = .2f;
    [SerializeField, Tooltip("the orientation at which the player will stop moving and start sliding")] private float m_maxSlope = 55f;
    [SerializeField, Tooltip("the speed at which the player will slide")] private float m_slideSpeed = 2f;
    [SerializeField, Tooltip("the key that the player will have to press to jump")] private KeyCode m_jumpKey = KeyCode.Space;
    
    [SerializeField, Tooltip("the character controller component")] CharacterController m_controller = null;
#if UNITY_EDITOR
    [Space]
    [Header("Editor Settings")]
    [SerializeField, Tooltip("A boolean to change the cursor lock and visibility mode")] private bool m_lockCursor = true;

#endif

    // Start is called before the first frame update
    void Start()
    {
        // Okay so hear me out
        // Sometimes... The people need to set stuff to the right boolean before compiling
        // But...
        // The people are retarded
        // Sooooo
        // yeah...
        
#if UNITY_EDITOR
        if(m_lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#else
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMouseLook();
        UpdateInput();
    }
    
    /// <summary/> Updating the mouses position
    void UpdateMouseLook()
    {
        // Fetching the player's input
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        // Calculating the camera's pitch
        m_cameraPicth -= targetMouseDelta.y * m_mouseSensitivity;
        
        //Clamping the pitch to avoid barrel rolls (._.)(.-.)(._.)
        m_cameraPicth = Mathf.Clamp(m_cameraPicth, -90.0f, 90.0f);

        // Applying the pitch
        m_cameraTr.localEulerAngles = Vector3.right * m_cameraPicth;

        //Calculating the camera's yaw
        Vector3 cameraYaw = Vector3.up * targetMouseDelta.x * m_mouseSensitivity;
        
        //Setting the camera and the player's new yaw
        transform.Rotate(cameraYaw);
        m_cameraParentTr.Rotate(cameraYaw);
    }

    /// <summary/> Updating the player's movement based on his direction inputs
    void UpdateInput()
    {
        // Checking whether the ground is within range and whether the player is grounded 
        bool grounded = CheckIfGrounded(out bool groundWithinRange, out Vector3 groundNormal ,out bool onSlope);

        // Setting the m_gravity to zero if the player is grounded and adding the gravitational acceleration to the player if he isn't
        if(grounded)
        {
            m_gVelocity = 0.0f;
            UpdateJump();
        }
        else if(!onSlope) m_gVelocity += m_gravity * Time.deltaTime;
        
        // Resetting the y velocity if the player bumps his head
        if (m_gVelocity < 0f)
        {
            float radius = m_controller.radius;
            Vector3 origin = transform.position + Vector3.up * (m_controller.height - radius);
            Debug.DrawLine(origin + Vector3.right *.2f + Vector3.up * radius,origin+ Vector3.right *.2f, Color.red);
            if(Physics.SphereCast(origin,radius, Vector3.up, out RaycastHit upHit, m_controller.skinWidth))
            {
                m_gVelocity = 0f;
            }
        }
        
        Vector3 targetSlideVelocity = Vector3.zero;

        // If the player is on a slope, slide him
        if (onSlope)
        {
            targetSlideVelocity.x += (1f - groundNormal.y) * groundNormal.x;
            targetSlideVelocity.z += (1f - groundNormal.y) * groundNormal.z;

            targetSlideVelocity = Vector3.ProjectOnPlane(targetSlideVelocity, groundNormal);
            targetSlideVelocity.Normalize();

            // m_currentInputDir = Vector2.zero;
        }
        
        if(targetSlideVelocity != Vector3.zero)
            m_slideDirection = Vector3.SmoothDamp(m_slideDirection, targetSlideVelocity, ref m_slideDirectionVelocity, m_slideSmoothTimeUp);
        else
            m_slideDirection = Vector3.SmoothDamp(m_slideDirection, targetSlideVelocity, ref m_slideDirectionVelocity, m_slideSmoothTimeDown);

        Vector3 slideVelocity = m_slideDirection * m_slideSpeed;
        
        if(grounded) m_slideDirection = Vector3.ProjectOnPlane(m_slideDirection,groundNormal);
        
        if(onSlope)
        {
            if (m_gVelocity >= slideVelocity.magnitude)
            {
                m_slideDirection = targetSlideVelocity *  m_gVelocity / m_slideSpeed;
            }
            m_gVelocity = 0f;
        }
            
        Vector3 velocity = Vector3.zero;
        
        
        // The direction of the player's Input
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        // Transition the player's input velocity smoothly from the wanted direction to the current one
        m_currentInputDir = Vector2.SmoothDamp(m_currentInputDir, targetDir, ref m_currentInputDirVelocity, m_moveSmoothTimeGrounded);
    
        // Fetching the input velocity
        Vector3 inputVelocity = transform.forward * m_currentInputDir.y + transform.right * m_currentInputDir.x;
        
        
        if(grounded || onSlope) inputVelocity = Vector3.ProjectOnPlane(inputVelocity,groundNormal);
        
        // Setting the velocity to the movement speed based of the player's input
        velocity = inputVelocity * m_movementSpeed + Vector3.down * m_gVelocity;
        
        // Projecting the velocity based on the ground normal 
        
#if UNITY_EDITOR
        Debug.DrawRay(transform.position + Vector3.up * m_controller.height/2,velocity);
        Debug.DrawRay(transform.position,velocity.normalized, Color.magenta);
        Debug.DrawRay(transform.position,Vector3.down * m_gVelocity, Color.cyan);
#endif
        
        // Moving the player
        m_controller.Move((velocity + slideVelocity) * Time.deltaTime);

        s_speed = (int) (velocity.magnitude + slideVelocity.magnitude);
        
        // Updating the camera
        UpdateCameraPosition();
    }

    /// <summary/>Function to call if the player is grounded
    /// <param name="p_groundetected"> reference to the boolean to know if the player actually is within ground detection range </param>
    /// <returns> whether or not the player is actually grounded </returns>
    private bool CheckIfGrounded(out bool p_groundetected, out Vector3 p_groundNormal, out bool p_onSlope)
    {
        //Shooting the spherecast from the bottom of the sphere with an offset so that the sphere begins incremented with the object
        RaycastHit groundHit;
        float rad = m_controller.radius;
        Vector3 position = transform.position;
        Vector3 origin = position + Vector3.up * rad;
        p_groundetected = Physics.SphereCast(origin, rad, Vector3.down, out groundHit, m_groundCheckRange);

#if UNITY_EDITOR
        Debug.DrawRay(groundHit.point,Vector3.up/4f);
#endif
        //Todo make dot asshole
        p_groundNormal = groundHit.normal;
        p_onSlope = Vector3.Angle(p_groundNormal,Vector3.up) >= m_maxSlope;
        
        bool grounded = p_groundetected && !p_onSlope && Vector3.Distance(groundHit.point, position + Vector3.up * rad) < m_controller.skinWidth + rad;
        
        // Returning whether or not the player is actually grounded based on the distance of the hit point from the player taking into account the character controller's skin width
        return grounded;
    }

    /// <summary/> Function to call when the player is grounded or in a situation where he can jump
    private void UpdateJump() {
        if(Input.GetKeyDown(m_jumpKey))
        {
            //Todo get set ass
            m_gVelocity = -Mathf.Sqrt(m_playerJumpHeight * 2f * m_gravity);
        }
    }
    
    /// <summary/> Updating the camera's position
    void UpdateCameraPosition() => m_cameraParentTr.position = transform.position;

#if UNITY_EDITOR
    #region CustomGizmo
    private void OnDrawGizmos()
    {
        
        // Checking if the player is grounded or is within ground detection range
        bool grounded = CheckIfGrounded(out bool groundWithinRange, out Vector3 groundNormal ,out bool onSlope);
        
        // Setting the color of these gizmos depending on wether 
        Gizmos.color = onSlope? Color.blue : grounded ? Color.green : groundWithinRange ?  Color.yellow : Color.red;

        Vector3 slopeVelocity = new Vector3();
        
        if (onSlope)
        {
            m_gVelocity = 0.0f;
            slopeVelocity.x += (1f - groundNormal.y) * groundNormal.x;
            slopeVelocity.z += (1f - groundNormal.y) * groundNormal.z;

            slopeVelocity = Vector3.ProjectOnPlane(slopeVelocity, groundNormal);
            
            Debug.DrawRay(transform.position,slopeVelocity.normalized, Color.blue);
        }
        
        // Drawing the ground check gizmo
        float rad = m_controller.radius;
        float checkRange = m_groundCheckRange;
        Vector3 pos = transform.position + Vector3.up * rad;
        if (!UnityEditor.Selection.Contains(gameObject))
        {
            Gizmos.color = Color.green;
            float height = m_controller.height;
            
            Gizmos.DrawLine(pos + Vector3.right * rad,pos + Vector3.right * rad + Vector3.up * (height - 2f * rad));
            Gizmos.DrawLine(pos + Vector3.left * rad,pos + Vector3.left * rad + Vector3.up * (height - 2f * rad));
            Gizmos.DrawLine(pos + Vector3.forward * rad,pos + Vector3.forward * rad + Vector3.up * (height - 2f * rad));
            Gizmos.DrawLine(pos + Vector3.back * rad,pos + Vector3.back * rad + Vector3.up * (height - 2f * rad));
            
            Gizmos.DrawWireSphere(pos,rad);
            Gizmos.DrawWireSphere(pos + Vector3.up * (height - 2f * rad),rad);
        }
        else
        {
            Gizmos.DrawLine(pos + Vector3.right * rad, pos + Vector3.right * rad + Vector3.down * checkRange);
            Gizmos.DrawLine(pos + Vector3.left * rad, pos + Vector3.left * rad + Vector3.down * checkRange);
            Gizmos.DrawLine(pos + Vector3.forward * rad, pos + Vector3.forward * rad + Vector3.down * checkRange);
            Gizmos.DrawLine(pos + Vector3.back * rad, pos + Vector3.back * rad + Vector3.down * checkRange);

            Gizmos.DrawWireSphere(pos, rad);
            Gizmos.DrawWireSphere(pos + Vector3.down * checkRange, rad);
        }
    }
    #endregion
#endif
}
