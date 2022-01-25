using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [SerializeField] private Transform m_playerTransform;
    [SerializeField] private float m_yOffset = 100f;
    [SerializeField] private bool m_rotateAsWell;
    
    // Update is called once per frame
    void Update()
    {
        transform.position = m_playerTransform.position + Vector3.up * m_yOffset;
        if (m_rotateAsWell) transform.rotation = m_playerTransform.rotation;
    }
}
