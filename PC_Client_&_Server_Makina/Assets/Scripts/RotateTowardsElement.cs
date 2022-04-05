using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsElement : MonoBehaviour
{
    [SerializeField] private Transform m_target;
    [SerializeField] private bool m_rotateOnX = false;
    [SerializeField] private bool m_rotateOnY = true;
    [SerializeField] private bool m_rotateOnZ = false;

    private Vector3 m_ogRotation;

    private void Start() => m_ogRotation = transform.rotation.eulerAngles;
    

    // Update is called once per frame
    void Update()
    {
        Vector3 lookAtRotation = Quaternion.LookRotation(m_target.position - transform.position,Vector3.up).eulerAngles;

        Vector3 rotation = m_ogRotation;
        
        if (m_rotateOnX) rotation.x = lookAtRotation.x;
        if (m_rotateOnY) rotation.y = lookAtRotation.y;
        if (m_rotateOnZ) rotation.z = lookAtRotation.z;

        transform.rotation = Quaternion.Euler(rotation);
    }
}
