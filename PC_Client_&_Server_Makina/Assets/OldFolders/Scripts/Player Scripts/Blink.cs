using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Blink : MonoBehaviour
{
    [SerializeField] private Transform m_camera = null;
    [SerializeField] private GameObject m_blinkTarget = null;

    [SerializeField] private float m_blinkRange = 10f;
    [SerializeField] private float m_maxBlinkSurfaceNormal = 90f;
    
    [SerializeField] private KeyCode m_blinkKey = KeyCode.Mouse0;

    [SerializeField] private LayerMask m_layerMask;
    
    [SerializeField] private float m_coolDown = 5f;

    private bool m_canBlink = false;
    
    private Vector3 m_defaultPosition = Vector3.zero;
    
    private RaycastHit m_hit;
    
    private bool m_gonnaTeleport = false;
    
    [HideInInspector] public bool m_timeStop = false;
    // Start is called before the first frame update
    void Start()
    {
        m_defaultPosition = m_blinkTarget.transform.position;
        StartCoroutine(BlinkCooldown());
    }

    // Update is called once per frame
    void Update()
    {
        
        bool raycast = false;

        if (Input.GetKey(m_blinkKey))
        {
            if(m_canBlink)
            {
                m_timeStop = true;
                raycast = Physics.Raycast(m_camera.position, m_camera.forward, out m_hit, m_blinkRange, m_layerMask);
                if (!raycast)
                    raycast = Physics.Raycast(m_camera.position + m_camera.forward * m_blinkRange, Vector3.down, out m_hit, m_layerMask);
                
                else if(Mathf.Abs(Vector3.Angle(m_hit.normal, Vector3.down)) < 90f)
                    raycast = Physics.Raycast(m_hit.point, Vector3.down, out m_hit, m_layerMask);
                
                if (raycast && Mathf.Abs(Vector3.Angle(m_hit.normal, Vector3.up)) <= m_maxBlinkSurfaceNormal)
                {
                    m_blinkTarget.transform.position = m_hit.point;
                    m_gonnaTeleport = true;
                }
                else
                {
                    m_blinkTarget.transform.position = m_defaultPosition;
                    m_gonnaTeleport = false;
                }
            }
        }
        else
        {
            m_timeStop = false;
            if (m_gonnaTeleport)
            {
                transform.position = m_blinkTarget.transform.position + Vector3.up;
                StartCoroutine(BlinkCooldown());
                m_gonnaTeleport = false;
            }

            m_blinkTarget.transform.position = m_defaultPosition;
        }
    }

    IEnumerator BlinkCooldown()
    {
        m_canBlink = false;
        yield return new WaitForSeconds(m_coolDown);
        m_canBlink = true;
    }
}
