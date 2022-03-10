using Mirror;
using UnityEngine;

public class Hosting : MonoBehaviour
{
    [SerializeField] private KeyCode m_hostInput = KeyCode.P;
    [SerializeField] private GameObject m_notConnectedPrompt;
    
    void Update()
    {
        if(NetworkServer.active && NetworkClient.active)
        {
            m_notConnectedPrompt.SetActive(false);
            return;
        }
        
        m_notConnectedPrompt.SetActive(true);
        if(Input.GetKeyDown(m_hostInput)) MyNetworkManager.singleton.StartHost();
    }
}
