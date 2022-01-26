using CustomMessages;
using Synchronizers;
using UnityEngine;

public class SynchroniseElevators : Synchronizer
{
    [SerializeField, Tooltip("the elevators to be synchronized (in the same order as on the other side if not big L)")]
    private ElevatorBehavior[] m_elevators;
    public delegate void ElevatorInitialDataDelegator(float p_speed,float p_waitTime);
    public static ElevatorInitialDataDelegator OnReceiveElevatorInitialData;
    
    // Start is called before the first frame update
    void Awake()
    {
        
        ClientManager.OnReceiveInitialData += InitializeElevators;
        ClientManager.OnReceiveElevatorActivation += OnUpdateElevator;
    }

    private void InitializeElevators(InitialData p_initialData)=>
        OnReceiveElevatorInitialData?.Invoke(p_initialData.elevatorSpeed, p_initialData.elevatorWaitTime);
    
    
    private void OnUpdateElevator(ElevatorActivation p_elevatorActivation) => m_elevators[p_elevatorActivation.index].ActivateElevator();
}
