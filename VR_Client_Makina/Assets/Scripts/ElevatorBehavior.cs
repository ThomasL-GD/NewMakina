using System;
using CustomMessages;
using Grabbabble_Type_Beat;
using Mirror;
using Network;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ElevatorBehavior : MonoBehaviour {
    private float m_bottomPosition;
    
    [SerializeField, Tooltip("The top position that the elevator will go to")] private float m_topPosition;
    
    [SerializeField] private Light m_light;
    [SerializeField] private Color m_lightColorOff = Color.green;
    [SerializeField] private Color m_lightColorOn = Color.red;

    private bool m_activated;
    private bool m_goingUp = true;

    private int m_index;
        
    public delegate void ElevatorLocalActivation(OVRInput.Controller p_handUsed);
    public static ElevatorLocalActivation OnElevatorLocalActivation;
    
    /// <summary/> The elevators speed in m/s
    private float m_speed = 10f;
    private float m_waitTime = 3f;
    private bool m_doneWaiting;
    private void Start()
    {
        float posY = transform.position.y;
        m_bottomPosition = posY;
        m_topPosition = posY + m_topPosition;
    }
    private void Update()
    {
        if(!m_activated) return;
        
        transform.position += (m_goingUp ? Vector3.up : Vector3.down) * m_speed * Time.deltaTime;

        if (m_goingUp && transform.position.y > m_topPosition)
        {
            m_goingUp = false;
            m_activated = false;
            m_light.color = m_lightColorOff;
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, m_topPosition, pos.z);
            return;
        }

        if (!m_goingUp && transform.position.y < m_bottomPosition)
        {
            m_goingUp = true;
            m_activated = false;
            m_light.color = m_lightColorOff;
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, m_bottomPosition, pos.z);
        }
    }
    
    public void ButtonActivateElevator(){
        NetworkClient.Send(new ElevatorActivation(){index = m_index});
    }
    
    public void ActivateElevator() {
        m_activated = true;
        m_light.color = m_lightColorOn;
    }

    public void SetInitialData(float p_speed,float p_waitTime, int p_index)
    {
        m_speed = p_speed;
        m_waitTime = p_waitTime;
        m_index = p_index;
    }

    private void OnTriggerEnter(Collider p_other) {
        if (p_other.gameObject.layer == VrHandBehaviour.s_layer) {
            MyNetworkManager.singleton.SendVrData(new ElevatorActivation(){index = m_index});
            OnElevatorLocalActivation?.Invoke(p_other.gameObject.GetComponent<VrHandBehaviour>().GetHandSide());
        }
    }
}
