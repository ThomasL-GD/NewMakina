using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Player_Scripts;
using UnityEngine;

public class LeureTargeter : MonoBehaviour
{
    [SerializeField] private LineRenderer m_laser;
    [SerializeField] private InputMovement3 m_movement;

    [SerializeField] private float m_activateLaserTimer = 2f;
    [SerializeField] private float m_shotTimer = 1f;
    
    [SerializeField] private Vector3 m_rotationOffset;
    
    [Header("Animation")]
    
    [SerializeField] private Animator m_animator;
    [SerializeField] private string m_triggerName;
    
    
    [Header("Prompt")]
    [SerializeField] private KeyCode m_promptKeycode = KeyCode.F;
    [SerializeField,TextArea] private string m_promptText;

    private bool m_switcher = true; 
    // Update is called once per frame
    void Update()
    {
        if(LeurreMovement.m_instance == null) return;
        
        transform.LookAt(LeurreMovement.m_instance.transform.position);
        transform.Rotate(m_rotationOffset);
        
        m_laser.SetPosition(0, transform.position);
        m_laser.SetPosition(1, LeurreMovement.m_instance.transform.position);
        
        if(!m_switcher) return;
        m_switcher = false;
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        m_movement.m_isDead = true;
        yield return new WaitForSeconds(m_activateLaserTimer);
        
        m_laser.enabled = true;
        
        yield return new WaitForSeconds(m_shotTimer);

        m_laser.enabled =false;
        gameObject.SetActive(false);
        m_animator.SetTrigger(m_triggerName);

        m_movement.m_isDead = false;
        
        TutorialPrompt.OnPrompt?.Invoke(m_promptText, m_promptKeycode == KeyCode.None ? null : (KeyCode?)m_promptKeycode);
        ClientManager.OnReceiveDestroyLeure.Invoke(new DestroyLeure());
        ClientManager.OnReceiveActivateBlind.Invoke(new ActivateBlind());
    }
}
