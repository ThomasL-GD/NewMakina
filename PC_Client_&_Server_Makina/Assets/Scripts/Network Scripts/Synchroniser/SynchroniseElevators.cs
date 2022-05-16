using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using UnityEngine;

public class SynchroniseElevators : Synchronizer <SynchroniseElevators>
{
    [SerializeField, Tooltip("the elevators to be synchronized (in the same order as on the other side if not big L)")]
    private ElevatorBehavior[] m_elevators;

    /// <summary>An array of the positions of all elevators</summary>
    public Vector3[] elevatorPositions {
        get {
            List<Vector3> tempValue = new List<Vector3>();
            foreach (ElevatorBehavior elevatorScript in m_elevators) {
                tempValue.Add(elevatorScript.transform.position);
            }

            return tempValue.ToArray();
        }
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        
        ClientManager.OnReceiveInitialData += InitializeElevators;
        ClientManager.OnReceiveElevatorActivation += OnUpdateElevator;
    }

    private void InitializeElevators(InitialData p_initialData)
    {
        for (int i = 0; i < m_elevators.Length; i++)
        {
            m_elevators[i].SetInitialData(p_initialData.elevatorSpeed,p_initialData.elevatorWaitTime,i);
        }
    }
    
    
    private void OnUpdateElevator(ElevatorActivation p_elevatorActivation) => m_elevators[p_elevatorActivation.index].ActivateElevator();
}
