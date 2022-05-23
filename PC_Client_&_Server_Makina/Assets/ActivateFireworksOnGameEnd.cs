using CustomMessages;
using UnityEngine;

public class ActivateFireworksOnGameEnd : MonoBehaviour
{
    [SerializeField] private GameObject m_toActivate;
    
    void Awake()
    {
        ClientManager.OnReceiveGameEnd += ReceiveGameEnd;
        m_toActivate.SetActive(false);
    }

    // Update is called once per frame
    void ReceiveGameEnd(GameEnd p_mess)
    {
        if(p_mess.winningClient == ClientConnection.PcPlayer)
        {
            m_toActivate.SetActive(true);
            return;
        }
        m_toActivate.SetActive(false);
    }
}
