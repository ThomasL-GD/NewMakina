using UnityEditor;
using UnityEngine;

public class InputMovement3 : MonoBehaviour
{
    [SerializeField, Tooltip("the player's character controller")]private CharacterController m_controler;
    
    [SerializeField, Min(0f),Tooltip("the player's input speed in m/s")]private float m_movementSpeed = 10f;
    
    [SerializeField, Min(0f),Tooltip("The Speed of the player's acceleration in m/s²")]private float m_accelerationSpeed = 100f;
    [SerializeField, Min(0f),Tooltip("The Speed of the player's deceleration in m/s²")]private float m_decelerationSpeed = 100f;
    [SerializeField, Min(0f),Tooltip("The Speed of the player's deceleration in °/²")]private float m_sustainDirectionChangeSpeed = 100f;

    [SerializeField, Tooltip("the sensitivity of the mouse"), Range(0f,15f)]
    private float m_mouseSensitivity = 4f;
    [SerializeField, Tooltip("The camera transform (not the parent)")] private Transform m_cameraTr = null;
    [Space,SerializeField, Tooltip("The camera parent transform")] private Transform m_cameraParentTr = null;
    /// <summary/> the current pitch of the player's first person camera
    private float m_cameraPicth = 0f;

    [SerializeField] private float s_inputAngle = 0f;
    [SerializeField] private float s_inputVelocityAngle = 0f;
    [SerializeField] private float s_lookAngle = 0f;
    /// <summary/> The velocity of the player's movement based his input
    Vector2 m_currentInputVelocity;
    
    #if UNITY_EDITOR
    [Tooltip("A boolean to change the cursor lock and visibility mode"), SerializeField,]private bool m_lockCursor = true;

    #endif


    [SerializeField]private float s_speed = 0;
    
    /// <summary/> The different states player's acceleration
    private enum AccelerationState
    {
        idle,
        accelerating,
        sustaining,
        decelerating,
        WRONG
    }

    /// <summary>
    /// <para>The current player's movement state</para> 
    /// <para>could be: idle, accelerating, sustaining, decelerating</para> 
    /// </summary>
    [SerializeField]private AccelerationState m_movementState = AccelerationState.idle;

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
        Vector3 displacement = new Vector3();
        
        UpdateMouseLook();
        UpdatePlayerInput(ref displacement);
        UpdateCameraPosition();
        
        m_controler.Move(displacement * Time.deltaTime);
    }

    #region Movement
    
    /// <summary/> Adds te players input to the displacement
    /// <param name="p_displacement"> the displacement to add the player's input too</param>
    private void UpdatePlayerInput(ref Vector3 p_displacement)
    {
        
        //Getting the player input
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Setting the different states
        // No input and immobile : Idle
        // No input and mobile : decelerating
        // Yes input and yes at max running speed : sustaining
        // Yes input and no at max running speed : accelerating
        // (ง ͠° ͟ل͜ ͡°)ง
        if (playerInput == Vector2.zero)
        {

            if (m_currentInputVelocity == Vector2.zero)
                m_movementState = AccelerationState.idle;
            else
                m_movementState = AccelerationState.decelerating;
        }
        else
        {
            if (Mathf.Approximately(m_currentInputVelocity.magnitude,m_movementSpeed))
                m_movementState = AccelerationState.sustaining;
            else if (m_currentInputVelocity.magnitude > m_movementSpeed)
                m_movementState = AccelerationState.WRONG;
            else
                m_movementState = AccelerationState.accelerating;
        }

        //Switch case for the different accelerations go brrr
        switch (m_movementState)
        {
            case AccelerationState.accelerating: 
                GoToTargetVelocity2D(ref m_currentInputVelocity, playerInput * m_movementSpeed,m_accelerationSpeed * Time.deltaTime);
            break;
            case AccelerationState.sustaining:
                m_currentInputVelocity = Vector3.RotateTowards(m_currentInputVelocity,playerInput,m_sustainDirectionChangeSpeed * Time.deltaTime, 0f);
                break;
            case AccelerationState.decelerating:
                GoToTargetVelocity2D(ref m_currentInputVelocity, Vector3.zero, m_decelerationSpeed * Time.deltaTime);
            break;
        }

        p_displacement += transform.right * m_currentInputVelocity.x + transform.forward * m_currentInputVelocity.y;
        s_speed = p_displacement.magnitude;

        s_inputAngle = Vector2.SignedAngle(playerInput, Vector2.up);
        s_inputVelocityAngle = Vector2.SignedAngle(m_currentInputVelocity, Vector2.up);
    }

    /// <summary/> transitions smoothly to a chosen value (basically Vector3.MoveTowards)
    /// <param name="p_current"> the current position reference ( will be replaced) </param>
    /// <param name="p_target"> the target velocity </param>
    /// <param name="p_transitionSpeed"> the speed at which the player will transition </param>
    private void GoToTargetVelocity2D(ref Vector2 p_current, Vector3 p_target, float p_transitionSpeed) =>
        p_current = Vector2.MoveTowards(p_current, p_target, p_transitionSpeed);
    
    #endregion

    #region Camera

    /// <summary/> Updating the camera's rotation position
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
    
    /// <summary/> Updating the camera's position
    void UpdateCameraPosition() => m_cameraParentTr.position = transform.position;

    #endregion
}