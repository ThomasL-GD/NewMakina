using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    [SerializeField] private ElevatorBehavior m_elevator;
    [SerializeField] float m_minActivationDistance = 3f;

    private bool m_inRange;
    // Update is called once per frame
    void Update()
    {
        m_inRange = Vector3.Distance(CameraAndUISingleton.camera.transform.position, transform.position) <
                    m_minActivationDistance;
        if(m_inRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                m_elevator.ButtonActivateElevator();
            }
        }
        else if(CameraAndUISingleton.elevatorButtonFeedback.activeSelf) CameraAndUISingleton.elevatorButtonFeedback.SetActive(false);
    }

    private void LateUpdate()
    {
        if (m_inRange && !m_elevator.m_activated) CameraAndUISingleton.elevatorButtonFeedback.SetActive(true);
    }
}
