using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    [SerializeField] private ElevatorBehavior m_elevator;
    [SerializeField] private KeyCode m_keyToActivate = KeyCode.Mouse0;
    [SerializeField] float m_minActivationDistance = 3f;
    [SerializeField] private Animator m_animator;
    
    private bool m_inRange;

    private static readonly int Switch = Animator.StringToHash("Switch");

    // Update is called once per frame
    void Update()
    {
        m_inRange = Vector3.Distance(CameraAndUISingleton.camera.transform.position, transform.position) <
                    m_minActivationDistance;
        if(m_inRange)
        {
            if (Input.GetKeyDown(m_keyToActivate))
            {
                m_elevator.ButtonActivateElevator();
                m_animator.SetTrigger(Switch);
            }
        }
        else if(CameraAndUISingleton.elevatorButtonFeedback.activeSelf) CameraAndUISingleton.elevatorButtonFeedback.SetActive(false);
    }

    private void LateUpdate()
    {
        if (m_inRange && !m_elevator.m_activated) CameraAndUISingleton.elevatorButtonFeedback.SetActive(true);
    }
}
