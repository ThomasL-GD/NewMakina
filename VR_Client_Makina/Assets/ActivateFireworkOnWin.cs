using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using Network;
using UnityEngine;

public class ActivateFireworkOnWin : MonoBehaviour
{
    [SerializeField] private GameObject m_firework; 

    // Start is called before the first frame update
    void Awake() {
        MyNetworkManager.OnReceiveGameEnd += OnReceiveGameEnd;
        MyNetworkManager.OnReadyToGoIntoTheBowl += Desactive;
    }

    private void Desactive(ReadyToGoIntoTheBowl p_p_ready) {
        m_firework.SetActive(false);
    }


    // Update is called once per frame
    void OnReceiveGameEnd(GameEnd p_mess)
    {
        if (p_mess.winningClient == ClientConnection.VrPlayer)
        {
            m_firework.SetActive(true);
            return;
        }
        m_firework.SetActive(false);
    }
}
