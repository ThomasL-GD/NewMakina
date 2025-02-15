using UnityEngine;

public class ElevatorBehaviorLocal : MonoBehaviour
{
    private float m_bottomPosition;
    
    [SerializeField, Tooltip("The top position that the elevator will go to")] private float m_topPosition;

    [SerializeField] private Light m_light;
    [SerializeField] private Color m_lightColorOff = Color.green;
    [SerializeField] private Color m_lightColorOn = Color.red;
    [HideInInspector]public bool m_activated;
    private bool m_goingUp = true;

    private int m_index;
    
    /// <summary/> The elevators speed in m/s
    private float m_speed = 10f;
    private float m_waitTime = 3f;
    private bool m_doneWaiting;

    private void Start()
    {
        float posY = transform.position.y;
        m_bottomPosition = posY;
        m_topPosition = posY + m_topPosition;
        m_speed = 5f;
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
        ActivateElevator();
    }
    
    public void ActivateElevator() {
        m_activated = true;
        m_light.color = m_lightColorOn;
    }

    public void SetInitialData(float p_speed,float p_waitTime, int p_index)
    {
        m_speed = 5f;
        m_waitTime = p_waitTime;
        m_index = p_index;
    }

    private void OnDrawGizmos()
    {
        TryGetComponent(out BoxCollider boxCollider);
        Vector3 box = Quaternion.Euler(-90, 0, 0) * boxCollider.size;
        Vector3 origin = transform.position + Quaternion.Euler(-90, 0, 0) * boxCollider.center;
        Gizmos.DrawWireCube(origin + Vector3.up * m_topPosition, box);
    }
}
