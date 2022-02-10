using CustomMessages;
using Mirror;
using Synchronizers;
using UnityEngine;

public class SynchroniseReady : Synchronizer<SynchroniseReady>
{
    [SerializeField] private GameObject m_readyStuff;


    private void Awake()
    {
        ClientManager.OnReceiveReadyToPlay += ReceiveReady;
    }


    private void ReceiveReady(ReadyToPlay p_message) => m_readyStuff.SetActive(true);
    

    // Update is called once per frame
    public void StartReady()
    {
        NetworkClient.Send(new ReadyToPlay());
        m_readyStuff.SetActive(false);
    }
}
