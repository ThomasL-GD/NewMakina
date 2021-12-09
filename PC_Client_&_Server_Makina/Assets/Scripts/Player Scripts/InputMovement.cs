using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class InputMovement : MonoBehaviour
{
    [SerializeField] private CharacterController m_characterController = null;
    
    [SerializeField] [Tooltip("The horizontal input acceleration of the player on ground in m/s²")]
    private float m_groundedAcceleration = 20f;
    [SerializeField] [Tooltip("The horizontal input acceleration of the player in the air in m/s²")]
    private float m_airAcceleration = 20f;
    

    [SerializeField] [Tooltip("The max running speed of the player in m/s")]
    private float m_maxRunSpeed = 10f;
    
    [SerializeField] [Tooltip("The max running speed of the player in m/s")]
    private float m_maxSprintSpeed = 20f;

    [SerializeField][Range(0f,1f)] private float m_minDragGrounded = 0.9f;
    [SerializeField][Range(0f,1f)] private float m_maxDragGrounded = 1f;
    
    [SerializeField][Range(0f,1f)] private float m_minDragInAir = 0.9f;
    [SerializeField][Range(0f,1f)] private float m_maxDragInAir = 1f;
    
    [SerializeField][Tooltip("Gravitational pull acceleration in m/s² +- 0.05f")]private float m_gravity = 9.8f;
    
    [SerializeField][Tooltip("the radius of the ground check spherecast based of the radius of the character controller")]
    [Range(0f,1f)]private float m_radiusMultiplier = .05f;

    [SerializeField][Tooltip("the minimum range at which the player will detect the ground in meters")]
    private float m_groundCheckRange = .5f;
    
    [SerializeField] [Tooltip("The height of the player's jump in m")]
    private float m_jumpHeight = 0f;
    
    // [SerializeField] [Tooltip("The amount of tolerance applied to the jump key in s")]
    // private float m_jumpToleranceTime = .05f;

    private Vector3 m_playerInput;
    
    [Header(" ")]
    
    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
    
    public Vector3 m_velocity = Vector3.zero;
    public float m_speed = 0f;

    [SerializeField]private float m_ledgeGrabDistance = 2f;
    [SerializeField]private float m_maxLedgeGrabHeight = 2f;
    [SerializeField]private float m_maxLedgeGrabHeightGrounded = 2f;

    private bool m_doingAction = false;
    [SerializeField]private float m_ledgeGrabOffset;
    [SerializeField] private float m_climbSpeed = 30f;
    [SerializeField] private float m_endClimbTolerance = .25f;
    [SerializeField]private float m_ledgeCheckMinHeight = .25f;
    
    private delegate void ActionUpdate();
    private ActionUpdate onActionUpdate;

    private Vector3 m_pullSource;
    [SerializeField] private float m_actionBailTime = 1f;

    public bool m_isDead = false;

    public static InputMovement instance;

    private bool m_grounded;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else
        {
            Debug.LogWarning("there are more than one input movement class in this scene",this);
            gameObject.SetActive(false);
        }
        if (!TryGetComponent(out m_characterController))
            Debug.LogWarning("<color=red>Error: </color>This Object has a PlayerMovementV25 Script but no Character Controller", gameObject);
        else if (!m_characterController.enabled)
            Debug.LogWarning("<color=red>Error: </color>This Object has a PlayerMovementV25 Script but no <color=yellow>active</color> Character Controller", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_isDead)
        {
            m_velocity = Vector3.zero;
            return;
        }
        
        if (m_doingAction)
        {
            PullToPoint();
            return;
        }

        #region Normal Movement

        // Changing the MaxRunSpeed based on if the player's state
        float maxRunSpeed = Input.GetKey(m_sprintKey) ? m_maxSprintSpeed : m_maxRunSpeed;
        
        //Getting values for the sphereCasts
        float radius = m_characterController.radius * m_radiusMultiplier;
        Vector3 origin = transform.position + Vector3.down * (m_characterController.height * .5f - radius);

        //Checking if the player is touching the ceiling to cancel his y velocity
        bool touchingCeiling = Physics.SphereCast(transform.position, radius, Vector3.up, out RaycastHit upHit, m_characterController.height / 2 + .1f);
        if( touchingCeiling && m_velocity.y>0f)m_velocity.y = 0f;
        
        //GroundChecking
        m_grounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, m_groundCheckRange);
        
        // Player input
        m_playerInput = GetPlayerInput();
        
        
        // Calculating the delta to make sure that the player doesn't go over his max velocity 
        Vector2 velocityXandZ = new Vector2(m_velocity.x, m_velocity.z);
        Vector2 playerInputPlane = new Vector2(m_playerInput.x, m_playerInput.z);
        
        float delta = maxRunSpeed - (velocityXandZ + (playerInputPlane * Time.deltaTime)).magnitude;
        delta = Mathf.Clamp(delta, 0f ,Mathf.Infinity);
        
        //Adding the delta time and the delta clamp
        m_playerInput *= Time.deltaTime;
        m_playerInput = Vector3.ClampMagnitude(m_playerInput, delta);
        
        
        //Adding the player input to the velocity
        m_velocity += m_playerInput;

        
        // Adding Slope
        if(m_grounded)
        {
            RaycastHit slopeHit;
            
            Physics.Raycast(transform.position, Vector3.down, out slopeHit, Vector3.Distance(transform.position, origin)+ m_groundCheckRange +.1f);
            m_velocity = Vector3.ProjectOnPlane(m_velocity, slopeHit.normal) +Vector3.up *.01f;

            Debug.DrawRay(origin, Vector3.down * (m_groundCheckRange + .1f));
        }
        
        //Gravity
         m_velocity.y *= .99f;
        if(!m_grounded) m_velocity += Vector3.down * (m_gravity * Time.deltaTime);
        
        //Edge Grab
        LedgeClimb(m_grounded);

        if (m_doingAction) return;
        
        //Jump
        if(m_grounded && Input.GetKeyDown(m_jumpKey)) m_velocity.y = Mathf.Sqrt(m_jumpHeight * 2f * m_gravity);
        
        
        //TODO Little step
        //TODO Vault
        
        //Move
        m_characterController.Move(m_velocity * Time.deltaTime);
        m_speed = m_velocity.magnitude;
        
        #endregion
        
        Debug.DrawRay( transform.position, m_velocity.normalized *2, Color.magenta);
    }

    /// <summary>
    /// ActionUpdate will be called to update the player's position based on his 
    /// </summary>
    private void PullToPoint()
    {
        Vector3 velocity = (m_pullSource - transform.position).normalized * (Time.deltaTime * m_climbSpeed);
        if (Vector3.Distance(transform.position, m_pullSource) < velocity.magnitude * Time.deltaTime + m_endClimbTolerance)
        {
            m_doingAction = false;
            return;
        }
        m_characterController.Move(velocity);
    }


    /// <summary>
    /// Testing if the playercan grab an edge
    /// Teleporting him / calling an animation to climb
    /// </summary>
    /// <param name="p_grounded"> player touching ground state </param>
    private void LedgeClimb(bool p_grounded)
    {
        // Checking that the player is not grounded and pressing the jump/grab key
        if (!p_grounded && Input.GetKey(m_jumpKey) || p_grounded && Input.GetKeyDown(m_jumpKey))
        {
            Vector3 pos = transform.position;
            float height = m_characterController.height/2;
            float radius = m_characterController.radius;

            float grabHeight = p_grounded ? m_maxLedgeGrabHeightGrounded : m_ledgeCheckMinHeight;
            
            // Forward face detection
            if (Physics.CapsuleCast(pos + Vector3.down * grabHeight,pos + Vector3.up * (m_maxLedgeGrabHeight + height - radius),  radius,transform.forward, out RaycastHit hit, m_ledgeGrabDistance))
            {
                pos = new Vector3(hit.point.x, transform.position.y - height + radius, hit.point.z);
                Vector3 horizontalHitPoint = new Vector3(hit.point.x, pos.y, hit.point.z);

                // Finding a point above the collision point at the max grab height to find the top of the collider
                Vector3 startPoint = horizontalHitPoint + Vector3.up * (m_maxLedgeGrabHeight + height) + transform.forward * .1f;
                Collider hitCollider = hit.collider;
                
                
                
                // Checking if we can find the top edge of the hit collider
                bool hitRaycast = Physics.Raycast(startPoint, Vector3.down, out hit, (m_maxLedgeGrabHeight + height + radius));
                int i = 0;
                
                // Checking if the vertical raycast hit the same collder as the vertical raycast
                while (hitRaycast && hit.collider != hitCollider)
                {
                    
                    // Setting the start point to the player's position but at the height of the previous hit
                    startPoint = new Vector3(pos.x, hit.point.y, pos.z);
                    
                    // Shooting a new horizontal raycast
                    hitRaycast = Physics.Raycast(startPoint, transform.forward, out hit, m_ledgeGrabDistance);
                    
                    if (hitRaycast)
                    {
                        // Shooting a new vertical raycast to test the collider again
                        startPoint = hit.point + Vector3.up * (m_maxLedgeGrabHeight - (hit.point.y - transform.position.y));
                        hitCollider = hit.collider;
                        hitRaycast = Physics.Raycast(startPoint, Vector3.down, out hit, m_maxLedgeGrabHeight);
                    }

                    // StackOverflow Safety
                    i++;
                    if (i >= 32)
                    {
                        hitRaycast = false;
                        
                    }
                }
            
                // If hitRaycast is true that means that an appropriate position for the player to gab has been found
                if (hitRaycast)
                {
                    
                    // Setting the top ond bottom point for the Capsule test
                    Vector3 pointA = hit.point + Vector3.up * (radius + .1f) + transform.forward * m_characterController.radius/2;
                    Vector3 pointB = hit.point + Vector3.up * (height * 2 - radius) + transform.forward * m_characterController.radius/2;

                    // Checking if the player is under a roof to avoid going through roofs
                    Ray ray = new Ray(transform.position,Vector3.up);
                    if(!Physics.SphereCast(ray,m_characterController.radius , Mathf.Abs(pointA.y - transform.position.y)))
                    {
                        Collider[] capsuleHits = Physics.OverlapCapsule(pointA, pointB, radius);

                        if (capsuleHits.Length < 1)
                        {
                            m_velocity.y = 0;
                            
                            m_pullSource = (pointA + pointB) * .5f;

                            m_doingAction = true;
                            StartCoroutine(BailAction());
                            //TODO
                            //onActionUpdate += PullToPoint;
                        }
                    }
                }
            }
        }
    }

    IEnumerator BailAction()
    {
        yield return new WaitForSeconds(m_actionBailTime);
        m_doingAction = false;
    }
    
    private void FixedUpdate()
    {
        if (m_isDead) return;
        
        // if (m_doingAction) return;
        
        // Normalizing the input and velocity... No shit *shrug*
        Vector2 normalizedVelocity = new Vector2(m_velocity.x,m_velocity.z).normalized;
        Vector2 normalizedInput = new Vector2(m_playerInput.x,m_playerInput.z).normalized;
        
        // Calculating the the normalized do product
        float normalizedDotProduct = (Vector3.Dot(normalizedVelocity, normalizedInput) + 1f)/2f;

        float drag = 1f;
        // Clamping the drag based on whether the player is grounded or not
        if (m_grounded)
            drag = Mathf.Clamp(m_minDragGrounded + (1f - m_minDragGrounded) * normalizedDotProduct, m_minDragGrounded, m_maxDragGrounded);
        else
            drag = Mathf.Clamp(m_minDragInAir + (1f - m_minDragInAir) * normalizedDotProduct, m_minDragInAir, m_maxDragInAir);
        
        // Applying the drag
        m_velocity = MultiplyVectorXandZ(m_velocity, drag);
    }
    
// N0

    public Vector3 MultiplyVectorXandZ(Vector3 p_vector, float p_factor)
    {
        p_vector.x *= p_factor;
        p_vector.z *= p_factor;
        
        return p_vector;
    }
    
    Vector3 GetPlayerInput()
    {

        Vector3 playerInput = new Vector3();

        playerInput += transform.forward * Input.GetAxisRaw("Vertical");
        playerInput += transform.right * Input.GetAxisRaw("Horizontal");

        playerInput = Vector3.ClampMagnitude(playerInput, 1);

        playerInput *= m_grounded?m_groundedAcceleration:m_airAcceleration;

        return playerInput;
    }
    
    private void OnDrawGizmosSelected()
    {
        CharacterController cc = GetComponent<CharacterController>();
        float radius = cc.radius * m_radiusMultiplier;
        Vector3 origin = transform.position + Vector3.down * (cc.height * .5f - radius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(origin + Vector3.down * m_groundCheckRange, radius);
        
        Gizmos.DrawRay(origin + Vector3.right * radius, Vector3.down * m_groundCheckRange);
        Gizmos.DrawRay(origin + Vector3.left * radius, Vector3.down * m_groundCheckRange);
        Gizmos.DrawRay(origin + Vector3.forward * radius, Vector3.down * m_groundCheckRange);
        Gizmos.DrawRay(origin + Vector3.back * radius, Vector3.down * m_groundCheckRange);
        
        Vector3 pos = transform.position;
        float height = cc.height/2;
        radius = cc.radius;
        // Forward face detection
        if (Physics.CapsuleCast(pos + Vector3.down * m_ledgeCheckMinHeight,pos + Vector3.up * (m_maxLedgeGrabHeight + height - radius),  radius,transform.forward, out RaycastHit hit, m_ledgeGrabDistance))
        {
            Gizmos.color = Color.magenta;
            pos = new Vector3(hit.point.x, transform.position.y - height + radius, hit.point.z);
            Gizmos.DrawWireSphere(pos + Vector3.down * m_ledgeCheckMinHeight ,radius);
            Gizmos.DrawWireSphere(pos + Vector3.up * (m_maxLedgeGrabHeight + height - radius),radius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point,.15f);
            
            Vector3 horizontalHitPoint = new Vector3(hit.point.x, pos.y, hit.point.z);

            // Finding a point above the collision point at the max grab height to find the top of the collider
            Vector3 startPoint = horizontalHitPoint + Vector3.up * (m_maxLedgeGrabHeight + height) + transform.forward * .1f;
            Collider hitCollider = hit.collider;
            
            
            
            // Checking if we can find the top edge of the hit collider
            bool hitRaycast = Physics.Raycast(startPoint, Vector3.down, out hit, (m_maxLedgeGrabHeight + height + radius));
            Debug.DrawLine(startPoint,hitRaycast?hit.point: startPoint + Vector3.down * (m_maxLedgeGrabHeight + height), Color.green);
            int i = 0;
            Gizmos.color = Color.cyan;
            if(hitRaycast && hit.collider != hitCollider) Gizmos.DrawSphere(hit.point, .1f);
            else if (hitRaycast)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(hit.point, .1f);
            }
            
            //TODO add grab based on camera
            
            // Checking if the vertical raycast hit the same collider as the vertical raycast
            while (hitRaycast && hit.collider != hitCollider)
            {
                
                // Setting the start point to the player's position but at the height of the previous hit
                startPoint = new Vector3(transform.position.x, hit.point.y -.1f, transform.position.z);
                
                // Shooting a new horizontal raycast
                hitRaycast = Physics.Raycast(startPoint, transform.forward, out hit, m_ledgeGrabDistance);
                
                Debug.DrawLine(startPoint,/*hitRaycast?hit.point: Vector3.down * (m_maxLedgeGrabHeight + height)*/startPoint + transform.forward*2, Color.green);
                Gizmos.color = Color.red;
                if(hitRaycast) Gizmos.DrawSphere(hit.point, .1f);
                if (hitRaycast)
                {
                    // Shooting a new vertical raycast to test the collider again
                    startPoint = hit.point + Vector3.up * (m_maxLedgeGrabHeight - (hit.point.y - transform.position.y));
                    hitCollider = hit.collider;
                    hitRaycast = Physics.Raycast(startPoint, Vector3.down, out hit, m_maxLedgeGrabHeight);
                    Debug.DrawLine(startPoint,hitRaycast?hit.point: Vector3.down * (m_maxLedgeGrabHeight + height), Color.cyan);
                    Gizmos.color = Color.red;
                    if(hitRaycast) Gizmos.DrawSphere(hit.point, .1f);
                }

                // StackOverflow Safety
                i++;
                if (i >= 32)
                {
                    hitRaycast = false;
                    Debug.Log("nope stack overflow");
                }
            }
            
            // If hitRaycast is true that means that an appropriate position for the player to gab has been found
            if (hitRaycast)
            {
                // Setting the top ond bottom point for the Capsule test
                Vector3 pointA = hit.point + Vector3.up * (radius + .1f) + transform.forward * m_characterController.radius/2;
                Vector3 pointB = hit.point + Vector3.up * (height * 2 - radius) + transform.forward * m_characterController.radius/2;

                // Checking if the player is under a roof to avoid going through roofs
                Ray ray = new Ray(transform.position,Vector3.up);
                if(!Physics.SphereCast(ray,m_characterController.radius , Mathf.Abs(pointA.y - transform.position.y)))
                {
                    Collider[] capsuleHits = Physics.OverlapCapsule(pointA, pointB, radius);
                    
                    Gizmos.color = Color.red;
                    if (capsuleHits.Length == 0)
                    {
                        Gizmos.color = Color.green;
                        m_velocity.y = 0;
                        
                        m_pullSource = (pointA + pointB) * .5f;
                    }
                    Gizmos.DrawWireSphere(pointA, radius);
                    Gizmos.DrawWireSphere(pointB, radius);
                }
            }
        }
        
    }
}