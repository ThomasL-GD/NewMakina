using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LeurreMovement : MonoBehaviour
{
    private float m_speed = 10f;
    private float m_gravity = 9.8f;

    [SerializeField] private CharacterController m_characterController;
    private float m_gravityVelocity;

    // Update is called once per frame
    void Update()
    {
        m_gravityVelocity = m_characterController.isGrounded ? 0f : m_gravityVelocity + m_gravity * Time.deltaTime;
        m_gravityVelocity = Mathf.Min(m_gravityVelocity,250f);
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.2f);
        Vector3 direction = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        m_characterController.Move(direction * (m_speed * Time.deltaTime) + Vector3.down * (m_gravityVelocity * Time.deltaTime));
        
        NetworkClient.Send(new LeureTransform(){position = transform.position, rotation = transform.rotation});
    }

   public void SetSpeedAndGravity(float p_speed, float p_gravity)
   {
       m_speed = p_speed;
       m_gravity = p_gravity;
   }
}
