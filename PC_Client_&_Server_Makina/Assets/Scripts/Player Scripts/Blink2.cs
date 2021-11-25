using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink2 : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject m_blinkTarget;
    [SerializeField] private float m_range = 20f;

    private Collider m_lastHitCollider;
    private float m_lastHitDistance;
    
    void OnDrawGizmos()
    {
        RaycastHit hit;
        bool raycast = Physics.Raycast(m_camera.transform.position, m_camera.transform.forward, out hit, m_range);
        
        Vector3 target = m_blinkTarget.transform.position;
        
        if (raycast)
        {
            m_lastHitCollider = hit.collider;
            m_lastHitDistance = hit.distance;
            
            target = hit.collider.ClosestPointOnBounds(hit.point);
        }
        
        if (!raycast)
        {
            target = m_lastHitCollider.ClosestPointOnBounds(m_camera.transform.position + m_camera.transform.forward * m_lastHitDistance);

            float distance = Vector3.Distance(m_camera.transform.position, target);
            Vector3 direction = (target - m_camera.transform.position).normalized;
            if(Physics.Raycast(m_camera.transform.position, target - m_camera.transform.position, out hit, distance))
            {
                target = hit.collider.ClosestPointOnBounds(hit.point);
                m_lastHitCollider = hit.collider;
                m_lastHitDistance = hit.distance;
            }
            else
            {
                m_lastHitDistance = Vector3.Distance(m_camera.transform.position, target);
            }
        }

        //TODO disable it if to far
        
        m_blinkTarget.transform.position = target;
    }
}
