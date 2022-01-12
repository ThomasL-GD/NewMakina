using CustomMessages;
using Synchronizers;
using UnityEngine;

public class SynchroniseElevators : Synchronizer
{
    [SerializeField, Tooltip("the elevators to be synchronized (in the same order as on the other side if not big L)")]
    private ElevatorBehavior[] m_elevators;
    
    
    
    // Start is called before the first frame update
    void Awake()
    {
        ClientManager.OnReceiveInitialData += InitializeElevators;
        ClientManager.OnReceiveElevatorActivation += OnUpdateElevator;
    }

    private void InitializeElevators(InitialData p_initialData)
    {
        for (int i = 0; i< m_elevators.Length;i++)
        {
            m_elevators[i].SetInitialData(p_initialData.elevatorSpeed, p_initialData.elevatorWaitTime);
            m_elevators[i].m_index = i;
        }
    }
    
    private void OnUpdateElevator(ElevatorActivation p_elevatorActivation) => m_elevators[p_elevatorActivation.index].ActivateElevator();
}
