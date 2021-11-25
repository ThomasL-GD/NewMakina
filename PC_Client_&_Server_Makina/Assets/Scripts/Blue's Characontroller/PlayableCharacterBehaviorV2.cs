using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacterBehaviorV2 : MonoBehaviour {

    [SerializeField] [Range(1f, 15f)] [Tooltip("The speed of the chara.\nUnit : meters per second")] private float m_maxSpeed = 5f;
    [SerializeField] [Range(1f, 15f)] [Tooltip("The speed gain of the chara.\nUnit : meters per second per second")] private float m_accelerationOnGround = 5f;
    
    [SerializeField] [Range(0f, 1f)] private float m_minimumDrag = 0f;
    [SerializeField] [Range(0f, 1f)] private float m_maximumDrag = 1f;
    
    private Vector3 m_velocity = Vector3.zero;

    private CharacterController m_charaController = null;
    [SerializeField] [Tooltip("For debug only.")] private float m_drag = 0f;
    private Vector2 m_inputDirection = Vector2.zero;

    // Start is called before the first frame update
    void Start() {
        m_charaController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update() {
        
        Vector3 acceleration = Vector3.zero;
        float maxVelocityForThisFrame = 0f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        acceleration = Vector3.ClampMagnitude(transform.forward * vertical + transform.right * horizontal, 1f);
        m_inputDirection = new Vector2(acceleration.x, acceleration.z).normalized;

        acceleration *= m_accelerationOnGround * Time.deltaTime;



        m_velocity += acceleration;
        
        
        
            Vector2 planarVelocity = new Vector2(m_velocity.x, m_velocity.z);
            m_drag = (Vector2.Dot(planarVelocity.normalized, m_inputDirection.normalized) + 1f)/2f;

            float temp = m_drag;

            m_drag = Mathf.Lerp(m_minimumDrag, m_maximumDrag, m_drag);
            
            Debug.Log($"Dot : {temp}                Lerp : {m_drag}");

            maxVelocityForThisFrame = m_drag * m_maxSpeed; // m_drag is a value between 0 and 1
        

        m_velocity = Vector3.ClampMagnitude(m_velocity, m_maxSpeed);
        
        
        m_charaController.Move(m_velocity * Time.deltaTime);
            
        Debug.DrawRay(transform.position, acceleration / Time.deltaTime, Color.magenta);
        Debug.DrawRay(transform.position, m_velocity, Color.green);

    }
    

    //TODO Get this out of the fixed update
    private void FixedUpdate() {
    }

}
