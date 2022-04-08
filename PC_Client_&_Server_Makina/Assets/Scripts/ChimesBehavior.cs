using System.Collections;
using UnityEngine;

public class ChimesBehavior : MonoBehaviour
{
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip[] m_sounds;
    private int m_previousClipIndex;
    [SerializeField] private float m_speed = 20f;
    [SerializeField] private float m_cooldown = 5f;

    [SerializeField] private Vector3 m_startPosition;
    [SerializeField] private Vector3 m_endPosition;

    private Vector3 m_startPositionGlobal;
    private Vector3 m_endPositionGlobal;
    
    [SerializeField] private LayerMask m_playerLayer;
    private bool m_moving = false;
    private bool m_inCoolDown = false;
    private float m_timer = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        m_startPositionGlobal = transform.position + transform.rotation * m_startPosition;
        m_endPositionGlobal = transform.position + transform.rotation * m_endPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(!m_moving) return;
        if (Vector3.Distance(m_audioSource.transform.position, m_endPositionGlobal) <= m_speed * Time.deltaTime)
        {
            m_moving = false;
            m_audioSource.Stop();
        }
        m_timer += Time.deltaTime;
        m_audioSource.transform.position = Vector3.MoveTowards(m_audioSource.transform.position, m_endPositionGlobal, m_timer * m_speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");
        if(m_moving || m_inCoolDown || (1 << other.gameObject.layer & m_playerLayer) == 0) return;
        Debug.Log("triggered correctly");
        m_audioSource.transform.position = m_startPositionGlobal;
        m_moving = true;
        m_timer = 0f;
        m_audioSource.clip = m_sounds[Random.Range(0, m_sounds.Length)];
        m_audioSource.Play();
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        m_inCoolDown = true;
        yield return new WaitForSeconds(m_cooldown);
        m_inCoolDown = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_startPosition.magnitude == 0) m_startPosition.z = -3f;
        if (m_endPosition.magnitude == 0) m_endPosition.z = 3f;

        //if (m_audioSource) m_audioSource.transform.position = transform.position + transform.rotation * m_startPosition;
        
        Gizmos.DrawLine(transform.position + transform.rotation * m_startPosition,transform.position + transform.rotation * m_endPosition);
    }
}
