using System;
using CustomMessages;
using Mirror;
using UnityEngine;

public class ElevatorBehavior : MonoBehaviour
{
    private float m_bottomPosition;
    
    [SerializeField, Tooltip("The top position that the elevator will go to")]
    private float m_topPosition;

    [HideInInspector]public bool m_activated;
    private bool m_goingUp = true;

    [HideInInspector]public int m_index;
    
    /// <summary/> The elevators speed in m/s
    private float m_speed = 10f;
    private float m_waitTime = 3f;
    private bool m_doneWaiting;

    private void Awake()=>
        SynchroniseElevators.OnReceiveElevatorInitialData += SetInitialData;
    

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
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, m_topPosition, pos.z);
            return;
        }

        if (!m_goingUp && transform.position.y < m_bottomPosition)
        {
            m_goingUp = true;
            m_activated = false;
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, m_bottomPosition, pos.z);
        }
    }
    
    public void ButtonActivateElevator(){
        NetworkClient.Send(new ElevatorActivation(){index = m_index});
    }
    
    public void ActivateElevator() {
        m_activated = true;
    }

    public void SetInitialData(float p_speed,float p_waitTime)
    {
        m_speed = p_speed;
        m_waitTime = p_waitTime;
    }

    private void OnDrawGizmos()
    {
        TryGetComponent(out BoxCollider boxCollider);
        Vector3 box = Quaternion.Euler(-90, 0, 0) * boxCollider.size;
        Vector3 origin = transform.position + Quaternion.Euler(-90, 0, 0) * boxCollider.center;
        Gizmos.DrawWireCube(origin + Vector3.up * m_topPosition, box);
    }
}
