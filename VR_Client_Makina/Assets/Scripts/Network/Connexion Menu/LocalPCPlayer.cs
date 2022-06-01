using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPCPlayer : MonoBehaviour {
    
    [SerializeField] private Transform[] m_path = null;
    [SerializeField] private bool m_mustLoopPath = true;
    [SerializeField, Range(0f, 50f)] private float m_speed = 15f;
    [SerializeField, Range(0f, 5f)] private float m_uncertainty = 15f;

    private int m_currentPathIndex = 0;

    private bool m_isRunning = false;

    private void OnEnable() {
        m_isRunning = true;
    }

    // Update is called once per frame
    void Update() {
        if (!m_isRunning) return;
        Transform transform1 = transform;
        Vector3 position = transform1.position;
        position += ((m_path[m_currentPathIndex].position - position).normalized * (m_speed * Time.deltaTime));
        transform1.position = position;
        transform1.LookAt(m_path[m_currentPathIndex].position);
        
        if (!((transform.position - m_path[m_currentPathIndex].position).magnitude < m_uncertainty)) return; //If we've the position we were seeking
            m_currentPathIndex++;
            
            if (m_currentPathIndex < m_path.Length) return; // If we've reached the end of the array
                switch (m_mustLoopPath) {
                        case true:
                            m_currentPathIndex = 0;
                            break;
                        case false:
                            m_isRunning = false;
                            break;
                }
    }

    private void OnDrawGizmosSelected() {
        if (m_path == null) return;
        Gizmos.color = Color.yellow;
        for (int index = 0; index < m_path.Length; index++) {
            Gizmos.DrawWireSphere(m_path[index].position, m_uncertainty);
            if (!m_mustLoopPath && index == 0) continue;
            Gizmos.DrawLine(m_path[index == 0 ? m_path.Length-1 : index -1].position, m_path[index].position);
        }
    }
}
