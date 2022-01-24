using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    [SerializeField] private ElevatorBehavior m_elevator;
    [SerializeField] float m_minActivationDistance = 3f;

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(CameraSingleton.camera.transform.position , transform.position) < m_minActivationDistance)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("press F");
                m_elevator.ButtonActivateElevator();
            }
        }
    }
}
