using System.Collections;
using UnityEngine;

namespace Player_Scripts
{
    public class InputMovement3 : MonoBehaviour
    {
        [SerializeField, HideInInspector, Min(0f)] private float m_edgeAutoStopCheckDistance = .25f;
        [SerializeField, HideInInspector, Range(0f,1f)] private float m_minSpeedFactor=.5f;
        [SerializeField, HideInInspector] private bool m_edgeSafety = true;

        [SerializeField, HideInInspector, Tooltip("the player's character controller")]private CharacterController m_controller;
        [SerializeField, HideInInspector, Tooltip("The camera transform (not the parent)")] private Transform m_cameraTr;
        [SerializeField, HideInInspector, Tooltip("The camera parent transform")] private Transform m_cameraParentTr;
    
        [SerializeField, HideInInspector, Min(0f),Tooltip("the player's input speed in m/s")]private float m_maxMovementSpeed = 10f;
        [SerializeField, HideInInspector, Min(0f),Tooltip("the player's input speed in m/s")]private float m_maxMovementSpeedSprinting = 10f;

        [SerializeField, HideInInspector, Tooltip("The acceleration curve of that player's speed progression will follow")] private AnimationCurve m_accelerationBehaviorCurve;
        [SerializeField, HideInInspector, Tooltip("The deceleration curve of that player's speed progression will follow")] private AnimationCurve m_decelerationBehaviorCurve;
    
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to accelerate to full speed"), Min(0f)]private float m_accelerationTime = .3f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to decelerate to an idle speed"), Min(0f)]private float m_decelerationTime = .5f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to accelerate to full speed"), Min(0f)]private float m_accelerationTimeSprinting = .5f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to decelerate to an idle speed"), Min(0f)]private float m_decelerationTimeSprinting = .5f;
        
        [SerializeField, HideInInspector, Tooltip("Boolean to check wether or not the player will have a different acceleration and deceleration in mid air")]private bool m_airAcceleration;
        
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to accelerate to full speed"), Min(0f)]private float m_accelerationTimeAirborn = .5f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to decelerate to an idle speed"), Min(0f)]private float m_decelerationTimeAirborn = .5f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to accelerate to full speed"), Min(0f)]private float m_accelerationTimeSprintingAirborn = .5f;
        [SerializeField, HideInInspector, Tooltip("The Time it will take the player to decelerate to an idle speed"), Min(0f)]private float m_decelerationTimeSprintingAirborn = .5f;
        
        [SerializeField, HideInInspector, Tooltip("The Sprint Key")] private KeyCode m_sprintKey;
        [SerializeField, HideInInspector, Tooltip("Boolean to check wether or not the player will be able to start sprinting while in mid air")]private bool m_canSprintInMidAir;
        [SerializeField, HideInInspector, Tooltip("The Speed of the player's speed transition rotation in °/²"), Min(0f)]private float m_directionChangeSpeed = 100f;
        [SerializeField, HideInInspector, Tooltip("The Speed of the player's speed transition rotation in °/²"), Min(0f)]private float m_directionChangeSpeedAirborn = 100f;

        [SerializeField, HideInInspector, Tooltip("the sensitivity of the mouse"), Range(0f,15f)] private float m_mouseSensitivity = 4f;
    
        [SerializeField, HideInInspector,Tooltip("current player's movement state")]private AccelerationState m_movementState = AccelerationState.idle;
    
        [SerializeField, HideInInspector, Tooltip("The gravitational acceleration of the player in m/s²") ]private float m_gravityAcceleration = 16f;
        [SerializeField, HideInInspector, Tooltip("The gravitational acceleration of the player on a slope in m/s²") ]private float m_slideAcceleration = 16f;
        [SerializeField, HideInInspector, Tooltip("The gravitational acceleration of the player in m/s²") ]private float m_maxSlideSpeed = 250f;
        [SerializeField, HideInInspector, Tooltip("The minimum slide deceleration value") ] private float m_minimumSlideDeceleration = .5f;
        [SerializeField, HideInInspector, Tooltip("The gravitational acceleration of the player in m/s²") ]private float m_maxFallSpeed = 250f;
        [SerializeField, HideInInspector, Tooltip("The distance until which the player will detect the ground")]private float m_groundCheckRange;
        [SerializeField, HideInInspector, Tooltip("The maximum slope inclination that the player can move on without sliding")]private float m_maxSlope;
        [SerializeField, HideInInspector, Tooltip("The Slide Deceleration in m/s²")] private float m_slideDeceleration;
    
        [SerializeField, HideInInspector, Tooltip("the height at which the player will jump in m"), Min(0.0f)] private float m_playerJumpHeight = .5f;
        [SerializeField, HideInInspector, Tooltip("the tolerance time of the jump in s"), Min(0.0f)] private float m_playerJumpToleranceTime = .5f;
        [SerializeField, HideInInspector, Tooltip("Will the player jump using the ground normal when grounded")]private bool m_jumpUsingGroundNormal;
        [SerializeField, HideInInspector, Tooltip("Will the player jump when on slope")]private bool m_jumpOnSlope= true;
        [SerializeField, HideInInspector, Tooltip("Will the player jump using the ground normal when on slope")]private bool m_jumpOnSlopeUsingGroundNormal = true;
        [SerializeField, HideInInspector, Tooltip("Will the player's jump reset his input velocity")]private bool m_antiClimbSafety = true;
        [SerializeField, HideInInspector, Tooltip("The Jump Key")] private KeyCode m_jumpKey;

        [SerializeField, HideInInspector, Tooltip("boolean to switch wether we want the player to have a headBob or not")] private bool m_headBob = true;
        [SerializeField, HideInInspector, Tooltip("The speed at which the head will bob in oscillations per second")] private float m_headBobSpeed = .8f;
        [SerializeField, HideInInspector, Tooltip("The intensity of the bob displacement in m")] private float m_headBobIntensity = .2f;
        [SerializeField, HideInInspector, Tooltip("the head bob animation curve")]private AnimationCurve m_headBobAnimationCurve;
    
        [SerializeField, HideInInspector, Tooltip("whether or not we use the smooth stepping")]private bool m_smoothStepping = true;
        [SerializeField, HideInInspector, Tooltip("the sensitivity of the step detection")]private float m_stepSensitivity = 100f;
        [SerializeField, HideInInspector, Tooltip("the speed of the of the stepping")]private float m_smoothingSpeed = 10f;

        /// <summary> The different states player's acceleration
        /// could be: idle, accelerating, sustaining, decelerating</summary>
        private enum AccelerationState
        {
            idle,
            accelerating,
            sustaining,
            decelerating
        }
    
        /// <summary> The different of the player's groundedness
        /// could be: grounded, groundDetected, onSlope, airborn</summary>
        private enum GroundTouchingState
        {
            grounded,
            groundDetected,
            onSlope,
            airborn
        }

#if UNITY_EDITOR
        [SerializeField, HideInInspector, Tooltip("A boolean to change the cursor lock and visibility mode")]private bool m_lockCursor = true;
        [SerializeField, HideInInspector, Tooltip("The speed of the player")]private float s_speed;
        [SerializeField, HideInInspector, Tooltip("The player's acceleration position on the abscess")]private float s_curvePositionX;
        [SerializeField, HideInInspector, Tooltip("The player's head bob position on the abscess")]private float s_headBobCurvePositionX;
        [SerializeField, HideInInspector, Tooltip("The angle between Vector3.forward and the player's input")] private float s_inputAngle;
        [SerializeField, HideInInspector, Tooltip("The angle between Vector3.forward and the player's input velocity")] private float s_inputVelocityAngle;
        [SerializeField, HideInInspector, Tooltip("The ground detection state odf the player")] private GroundTouchingState s_groundTouchingState;
#endif
    
        /// <summary/> The displacement the player wil get from a slope
        private Vector3 m_slopeDisplacement = Vector3.down;
    
        /// <summary/> The current pitch of the player's first person camera
        private float m_cameraPitch;

        /// <summary/> The velocity of the player's movement based his input
        private Vector2 m_currentInputVelocity;
    
        /// <summary/> The timer to synchronise the acceleration curves
        private float m_accelerationTimer;
    
        /// <summary/> The initial speed during an acceleration curve
        private float m_initialSpeed;
    
        /// <summary/> The initial speed during an acceleration curve
        private float m_originalCameraHeight;
    
        /// <summary/> The gravity
        [SerializeField, HideInInspector]private Vector3 m_gravity;
    
        /// <summary/> The slide velocity
        [SerializeField, HideInInspector]private float m_slideVelocity;

        /// <summary/> If this boolean is active : the player will jump when he gets grounded
        [SerializeField,HideInInspector]private bool m_jump;
    
        private Coroutine m_jumpCoroutine;
        private bool m_smoothing;
        private float m_targetVelocity;
        private float m_headBobPreviousTime;
        private bool m_sprinting;
        private Vector2 m_inputDirection;


        // Start is called before the first frame update
        void Start()
        {
            m_originalCameraHeight = m_cameraTr.position.y;
            m_cameraParentTr.position = transform.position;
            
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
            
            Vector3 displacement = new Vector3();

            UpdateGroundDetection(out var groundTouchingState, out var groundNormal);
            UpdateJump(groundTouchingState, groundNormal);
            UpdateSlide(ref displacement, groundTouchingState,groundNormal);
            UpdateGravity(ref displacement, groundTouchingState);
            UpdatePlayerInput(ref displacement, groundNormal, groundTouchingState);
        
            // The way the stepping works is that I compare the movement the player would've done with a simple
            // transform.Translate to the one done by the Character controller.Move (Only on the y).
            // TO be accurate we use a same value of delta time
            float previous = transform.position.y;
            float deltaTime = Time.deltaTime;
        
            // Moving the player
            m_controller.Move(displacement * deltaTime);
        
            // Calculating the movement
            float difference = Mathf.Abs(previous + displacement.y * deltaTime) - Mathf.Abs(transform.position.y);
            bool snappedMovement = Mathf.Round(difference * m_stepSensitivity) / m_stepSensitivity != 0f;
            UpdateCameraPosition(snappedMovement);
        
        
            if (m_headBob) UpdateHeadBob(new Vector2(displacement.x, displacement.z).magnitude, groundTouchingState);
        
#if UNITY_EDITOR
            s_groundTouchingState = groundTouchingState;
#endif
        }

        #region Terrain
    
        /// <summary>
        /// The function to check if the player is grounded
        /// Uses a sphere cast based on the radius of the player going straight down
        /// </summary>
        /// <param name="p_groundTouchingState"> The ground touching state reference to change to different states : grounded, ground within detection range, airborn , ons lope</param>
        /// <param name="p_groundNormal"> The normal of the ground on which the player is (only relevant if the player is grounded or ground detected) </param>
        private void UpdateGroundDetection(out GroundTouchingState p_groundTouchingState, out Vector3 p_groundNormal)
        {
            p_groundNormal = Vector3.up;
            p_groundTouchingState = GroundTouchingState.airborn;
        
            
            //Shooting the spherecast from the bottom of the sphere with an offset so that the sphere begins incremented with the object
            float rad = m_controller.radius;
            Vector3 origin = transform.position + Vector3.up * rad;

            bool groundDetected = Physics.SphereCast(origin, rad, Vector3.down, out var groundHit, m_groundCheckRange);

        
            if(!groundDetected) return;
            p_groundTouchingState = GroundTouchingState.groundDetected;
        
            Physics.Raycast(groundHit.point + Vector3.up * 0.25f, Vector3.down, out RaycastHit hit, 1f);
            p_groundNormal = hit.normal;
        
            float distance = Vector3.Distance(origin , groundHit.point);
            float range = m_controller.skinWidth + rad + Mathf.Epsilon + m_controller.minMoveDistance;
            bool grounded = distance <= range;

#if UNITY_EDITOR
            Debug.DrawRay(groundHit.point,Vector3.up * 0.25f);
        
            Debug.DrawLine(origin, origin  + (groundHit.point - origin).normalized * rad);
#endif

            if (!grounded) return;
            p_groundTouchingState = GroundTouchingState.grounded;

            bool onSlope = Vector3.Angle(p_groundNormal,Vector3.up) >= m_maxSlope;

            if (!onSlope) return;
        
            p_groundTouchingState = GroundTouchingState.onSlope;
        }


        /// <summary/> This coroutine will extends the amount of time the player will be able to jump
        IEnumerator JumpCoroutine()
        {
            m_jump = true;
            yield return new WaitForSeconds(m_playerJumpToleranceTime);
            m_jump = false;
        }
    
        /// <summary/> updates the player's jump based on whether or not he is grounded and pressing the right key
        /// <param name="p_groundTouchingState"> Whether the player is grounded or whatever  </param>
        /// <param name="p_groundNormal"> The ground Normal </param>
        private void UpdateJump(GroundTouchingState p_groundTouchingState, Vector3 p_groundNormal)
        {
            // Checking for a surface above the player to make him bump his head
            if(p_groundTouchingState != GroundTouchingState.grounded && p_groundTouchingState != GroundTouchingState.onSlope)
            {
                float rad = m_controller.radius;
                Vector3 origin = transform.position + Vector3.up * (m_controller.height - rad);

                bool bumpedHead = Physics.SphereCast(origin, rad, Vector3.up, out var hit, m_controller.skinWidth + rad + Mathf.Epsilon + m_controller.minMoveDistance);

                if (bumpedHead && m_gravity.y > 0f) m_gravity.y = 0f;
            }

            // Getting the input
            if(Input.GetKeyDown(m_jumpKey))
            {
                if(m_jumpCoroutine != null) StopCoroutine(m_jumpCoroutine);
                m_jumpCoroutine = StartCoroutine(JumpCoroutine());
            }
        
            // Checking whether or not the player is in a situation in which he can jump
            bool canJump = p_groundTouchingState == GroundTouchingState.grounded || (m_jumpOnSlope && p_groundTouchingState == GroundTouchingState.onSlope);
        
            // Returning if the player can't jump
            if(!canJump) return;

            Vector3 jumpDir;
        
            // Jumping with the grounded conditions if the player is grounded
            if(p_groundTouchingState == GroundTouchingState.grounded && m_jump)
            {
                jumpDir = m_jumpUsingGroundNormal ? p_groundNormal : Vector3.up;
                m_gravity.y = 0f;
                m_gravity += jumpDir * Mathf.Sqrt(m_playerJumpHeight * 2f * m_gravityAcceleration);
                m_jump = false;
                StopCoroutine(m_jumpCoroutine);
                return;
            }

            // Jumping with the On slope condition if the player is on slope
            if(p_groundTouchingState == GroundTouchingState.onSlope && m_jump)
            {
                jumpDir = m_jumpOnSlopeUsingGroundNormal ? p_groundNormal : Vector3.up;

                if (m_jumpOnSlopeUsingGroundNormal && m_antiClimbSafety)
                {
                    m_currentInputVelocity = Vector2.zero;
                    ResetCurveTimer();
                }
                
                m_currentInputVelocity = Vector2.zero; // Resetting the player's input velocity to avoid climbing
                m_gravity.y = 0f;
                m_gravity += jumpDir * Mathf.Sqrt(m_playerJumpHeight * 2f * m_gravityAcceleration);
                m_jump = false;
                StopCoroutine(m_jumpCoroutine);
            }
        }
    
        /// <summary>
        /// Updates the player's slide
        /// </summary>
        /// <param name="p_displacement"> the displacement reference to which the players slide displacement will be added </param>
        /// <param name="p_groundTouchingState"> Whether the player is grounded or whatever </param>
        /// <param name="p_groundNormal"> The normal of the ground on which the player is standing </param>
        private void UpdateSlide(ref Vector3 p_displacement, GroundTouchingState p_groundTouchingState, Vector3 p_groundNormal)
        {
            // If the player is sliding : accelerate
            if(p_groundTouchingState == GroundTouchingState.onSlope)
            {

                m_slideVelocity = Mathf.Max(m_slideVelocity, -m_gravity.y);
                m_slideVelocity += m_slideAcceleration * Time.deltaTime;
                m_slideVelocity = Mathf.Min(m_slideVelocity, m_maxSlideSpeed);
            }
            else // if not : decelerate
            {
                m_slideVelocity -= m_slideDeceleration * Time.deltaTime;
                m_slideVelocity = Mathf.Max(0, m_slideVelocity);
            
            }
        
            // Once the player stops decelerating reset the slide direction to straight down
            if(m_slideVelocity == 0)
            {
                m_slopeDisplacement = Vector3.down;
                return;
            }
        
            // Projecting the velocity on the plane under the player
            m_slopeDisplacement = Vector3.ProjectOnPlane(m_slopeDisplacement, p_groundNormal).normalized;
        
            // Calculating he sliding speed of the player based on the angle of the slope
            float speed = Vector3.Angle(p_groundNormal, Vector3.up) / 90f;
        
            // Clamping the minimum acceleration to .5f
            speed = Mathf.Max(m_minimumSlideDeceleration, speed);
        
            // Setting the new Displacement
            m_slopeDisplacement *= m_slideVelocity * speed;

#if UNITY_EDITOR
            Debug.DrawRay(transform.position, m_slopeDisplacement.normalized, Color.blue);
#endif
        
            // Adding the displacement to the thing
            p_displacement += m_slopeDisplacement;
        }
    
        /// <summary>
        /// Updates the player's gravity
        /// </summary>
        /// <param name="p_displacement"> the displacement reference to which the gravity will be added </param>
        /// <param name="p_groundTouchingState"> Whether the player is grounded or whatever </param>
        private void UpdateGravity(ref Vector3 p_displacement, GroundTouchingState p_groundTouchingState)
        {
            // Checking whether or not we should apply the gravity
            bool applyGravity = p_groundTouchingState == GroundTouchingState.airborn || p_groundTouchingState == GroundTouchingState.groundDetected;
        
            // if should apply gravity
            //      add gravity acceleration
            // else
            //      if m_gravity.y <=0 
            //          Gravity reset
            //      else
            //          conserve Gravity
            m_gravity = applyGravity ?  m_gravity + Vector3.down * (m_gravityAcceleration * Time.deltaTime) : m_gravity.y<=0? Vector3.zero: m_gravity;
        
            // clamp the falling speed to the maximum falling speed
            m_gravity.y = Mathf.Min(m_gravity.y, m_maxFallSpeed);
        
            // Add the gravity to the thing
            p_displacement += m_gravity;
        }

        #endregion
        
        #region Movement

        /// <summary/> Adds te players input to the displacement
        /// <param name="p_displacement"> the displacement to add the player's input too</param>
        /// <param name="p_groundNormal"> the ground normal of that the player is standing on </param>
        /// <param name="p_groundTouchingState"> the ground touching state of the player </param>
        private void UpdatePlayerInput(ref Vector3 p_displacement, Vector3 p_groundNormal, GroundTouchingState p_groundTouchingState)
        {
            float accelerationTime;
            float decelerationTime;

            float directionChangeSpeed;
            
            if (!m_airAcceleration || p_groundTouchingState == GroundTouchingState.grounded)
            {
                directionChangeSpeed = m_directionChangeSpeed;
                m_sprinting = Input.GetKey(m_sprintKey);
                accelerationTime = m_sprinting? m_accelerationTimeSprinting : m_accelerationTime;
                decelerationTime = m_sprinting? m_decelerationTimeSprinting : m_decelerationTime;
            }
            else
            {
                directionChangeSpeed = m_directionChangeSpeedAirborn;
                if(m_canSprintInMidAir) m_sprinting = Input.GetKey(m_sprintKey);
                
                accelerationTime = m_sprinting? m_accelerationTimeSprintingAirborn : m_accelerationTimeAirborn;
                decelerationTime = m_sprinting? m_decelerationTimeSprintingAirborn : m_decelerationTimeAirborn;
            }
            
            float maxMovementSpeed = m_sprinting? m_maxMovementSpeedSprinting : m_maxMovementSpeed;

            //Getting the player input
            Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            m_inputDirection = Vector2.MoveTowards(m_inputDirection,playerInput,directionChangeSpeed * Time.deltaTime);
            
            // Setting the different states
            // No input and immobile : Idle
            // No input and mobile : decelerating
            // Yes input and yes at max running speed : sustaining
            // Yes input and max running speed is inferior : decelerating
            // Yes input and no at max running speed : accelerating
            //
            // decelerating & accelerating are temporary states.
            // That means that they are transition states between Idle and Sustain. The are transitioned between by user defined curves called  m_accelerationBehaviorCurve & m_decelerationBehaviorCurve
            // (ง ͠° ͟ل͜ ͡°)ง
            if (playerInput == Vector2.zero)
            {
                if (m_currentInputVelocity == Vector2.zero)
                {
                    m_movementState = AccelerationState.idle;
                    //m_inputDirection = Vector2.zero;
                }
                else
                {
                    // Resetting the curve timer
                    if (m_movementState != AccelerationState.decelerating || m_targetVelocity != 0f)
                    {
                        ResetCurveTimer();
                        m_targetVelocity = 0f;
                    }
                    else m_accelerationTimer += Time.deltaTime;
                
                    m_movementState = AccelerationState.decelerating;
                }
            }
            else
            {
                if (Mathf.Approximately(m_currentInputVelocity.magnitude,maxMovementSpeed))
                    m_movementState = AccelerationState.sustaining;
                else if (m_currentInputVelocity.magnitude > maxMovementSpeed)
                {
                    if(m_movementState != AccelerationState.decelerating)
                    {
                        m_movementState = AccelerationState.decelerating;
                        ResetCurveTimer();
                        m_targetVelocity = maxMovementSpeed;
                    }else m_accelerationTimer += Time.deltaTime;
                }
                else
                {
                    if (m_movementState != AccelerationState.accelerating) ResetCurveTimer();
                    else m_accelerationTimer += Time.deltaTime;
                
                    m_movementState = AccelerationState.accelerating;
                }
            }

            //Switch case for the different acceleration curves goes brrrr
            switch (m_movementState)
            {
                case AccelerationState.accelerating:
                
                    // Calculating the time the acceleration will take based on the factor between the starting speed and the maximum
                    float time = accelerationTime * (1 - m_initialSpeed / maxMovementSpeed);
                
                    // calculating the position of the acceleration on the curve
                    float curvePositionX = m_accelerationBehaviorCurve.Evaluate(m_accelerationTimer / time);
                
#if UNITY_EDITOR
                    s_curvePositionX = curvePositionX;
#endif
                
                    // Calculating the speed and adding it to the current input
                    float speed = curvePositionX * (maxMovementSpeed - m_initialSpeed) + m_initialSpeed;
                    m_currentInputVelocity = m_inputDirection * speed;
                
                    break;
                case AccelerationState.sustaining:
                    // Linear transition between directions when going at full speed
                    m_currentInputVelocity = m_inputDirection * maxMovementSpeed;
                    break;
                case AccelerationState.decelerating:

                    float speedToRemove = m_initialSpeed - m_targetVelocity;
                    // Calculating the time the deceleration will take based on the factor between the starting speed and the maximum
                    time = decelerationTime * speedToRemove / maxMovementSpeed;

                    // calculating the position of the deceleration on the curve
                    curvePositionX = m_decelerationBehaviorCurve.Evaluate(m_accelerationTimer / time);
                    
#if UNITY_EDITOR
                    s_curvePositionX = curvePositionX;
#endif
                
                    
                    // Calculating the speed and adding it to the current input
                    speed = m_targetVelocity + (curvePositionX * speedToRemove);
                    
                    // Debug.Log($"target: {m_targetVelocity} initial: {m_initialSpeed} time: {time} speed: {speed}");
                    m_currentInputVelocity = Vector3.ClampMagnitude(m_currentInputVelocity,1f) * speed;
                
                    break;
            }

            // Calculating the input displacement
            var t = transform;
            Vector3 inputDisplacement = t.right * m_currentInputVelocity.x + t.forward * m_currentInputVelocity.y;
        
            // Buffering the horizontal push of the gravity based on the player's input
            Vector3 gravityHorizontal = new Vector3(m_gravity.x,0f, m_gravity.z);
            gravityHorizontal = Vector3.ClampMagnitude(gravityHorizontal, (gravityHorizontal + inputDisplacement * Time.deltaTime).magnitude );
            inputDisplacement = Vector3.ProjectOnPlane(inputDisplacement, p_groundNormal);
            m_gravity.x = gravityHorizontal.x;
            m_gravity.z = gravityHorizontal.z;
            
            // Edge security
            if (m_edgeSafety && m_movementState == AccelerationState.decelerating && p_groundTouchingState == GroundTouchingState.grounded && inputDisplacement.magnitude <= m_minSpeedFactor * maxMovementSpeed)
            {
                if (!Physics.Raycast(Vector3.up + transform.position + inputDisplacement * Time.deltaTime, Vector3.down, m_edgeAutoStopCheckDistance + 1f))
                {
                    inputDisplacement = Vector3.zero;
                    m_currentInputVelocity = Vector2.zero;
                }
            }
            
            // diplacement groig dfs<fksdgf
            p_displacement += inputDisplacement;

#if UNITY_EDITOR
            s_speed = m_currentInputVelocity.magnitude;

            s_inputAngle = Vector2.SignedAngle(m_inputDirection, Vector2.up);
            s_inputVelocityAngle = Vector2.SignedAngle(m_currentInputVelocity, Vector2.up);
#endif
        }

        /// <summary/> resets the curve timer by resetting the timer and getting a new initial speed
        private void ResetCurveTimer()
        {
            m_accelerationTimer = 0f;
            m_initialSpeed = m_currentInputVelocity.magnitude;
        }

        #endregion

        #region Head Bob

        /// <summary>
        /// The function called tu update the head's position using the headbob curve and the time
        /// </summary>
        /// <param name="p_movementSpeed"> the magnitude of the bob </param>
        /// <param name="p_groundTouchingState"> the ground touching state of the player </param>
        private void UpdateHeadBob(float p_movementSpeed, GroundTouchingState p_groundTouchingState)
        {
            // Getting the period of the curve by inverting the speed 
            float invertedSpeed = 1 / m_headBobSpeed;
            
            // if the player isn't grounded : reset the position
            if (p_groundTouchingState != GroundTouchingState.grounded || p_movementSpeed == 0f)
            {
                m_headBobPreviousTime = 0f;
                m_cameraTr.localPosition = Vector3.MoveTowards(m_cameraTr.localPosition, Vector3.up * m_originalCameraHeight, invertedSpeed* Time.deltaTime);
                return;
            }
            
            float speed = Mathf.Min(p_movementSpeed, m_maxMovementSpeedSprinting);

            m_headBobPreviousTime += Time.deltaTime * speed/m_maxMovementSpeedSprinting;

            float intensity = m_headBobIntensity * (speed / m_maxMovementSpeedSprinting);
            
            float time = (m_headBobPreviousTime % invertedSpeed)/ invertedSpeed;
            
            s_headBobCurvePositionX = time;
            m_cameraTr.localPosition =  Vector3.up * (m_originalCameraHeight + m_headBobAnimationCurve.Evaluate(time) * (intensity / 2f));
        }

        #endregion
    
        #region Camera

        /// <summary/> Updating the camera's rotation position
        void UpdateMouseLook()
        {
            // Fetching the player's input
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
            // Calculating the camera's pitch
            m_cameraPitch -= mouseDelta.y * m_mouseSensitivity;
        
            //Clamping the pitch to avoid barrel rolls (._.)(.-.)(._.)
            m_cameraPitch = Mathf.Clamp(m_cameraPitch, -90.0f, 90.0f);

            // Applying the pitch
            m_cameraTr.localEulerAngles = Vector3.right * m_cameraPitch;

            //Calculating the camera's yaw
            Vector3 cameraYaw = Vector3.up * (mouseDelta.x * m_mouseSensitivity);
        
            //Setting the camera and the player's new yaw
            m_cameraParentTr.Rotate(cameraYaw);
            transform.Rotate(cameraYaw);
        }
    
        /// <summary/> Updating the camera's position
        void UpdateCameraPosition(bool p_snapped)
        {
            if(!m_smoothStepping  || !p_snapped && !m_smoothing)
            {
                m_cameraParentTr.position = transform.position;
                return;
            }

        
            m_smoothing = true;
            Vector3 targetPosition = transform.position;
            Vector3 startingPosition = new Vector3(targetPosition.x, m_cameraParentTr.position.y, targetPosition.z);
            float movementSpeed = m_smoothingSpeed * (Vector3.Distance(startingPosition, targetPosition) + 1f);
            
            m_cameraParentTr.position = Vector3.MoveTowards(startingPosition, targetPosition,movementSpeed * Time.deltaTime);
            if (transform.position == m_cameraParentTr.position) m_smoothing = false;
        }

        #endregion
    
#if UNITY_EDITOR
        #region CustomGizmo
        private void OnDrawGizmos()
        {
            UpdateGroundDetection(out GroundTouchingState groundTouchingState, out Vector3 groundNormal);
        
            switch (groundTouchingState)
            {
                case GroundTouchingState.airborn:
                    Gizmos.color = Color.red;
                    break;
                case GroundTouchingState.grounded:
                    Gizmos.color = Color.green;
                    break; 
                case GroundTouchingState.groundDetected:
                    Gizmos.color = Color.yellow;
                    break;
                case GroundTouchingState.onSlope:
                    Gizmos.color = Color.blue;
                    break;
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
}