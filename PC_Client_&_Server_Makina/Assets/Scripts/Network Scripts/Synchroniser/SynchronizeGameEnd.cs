using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchronizeGameEnd : Synchronizer<SynchronizeGameEnd>
{
    [SerializeField] private TextMeshProUGUI m_text;
    [SerializeField] private string m_winMessage = "";
    [SerializeField] private string m_loseText = "";
    private Vector3 m_initialPlayerPosition;
    /// <summary>
    /// Awake is called before the Start
    /// </summary>
    void Awake() {
        ClientManager.OnReceiveGameEnd += GameEnd;
        ClientManager.OnReceiveReadyToPlay += Prepare;
    }

    private void Start()
    {
        m_initialPlayerPosition = SynchronizePlayerPosition.Instance.m_player.position;
    }

    private void Prepare(ReadyToPlay p_readyToPlay) => m_text.gameObject.SetActive(false);
    
    void GameEnd(GameEnd p_gameEnd) {

        if (p_gameEnd.winningClient == ClientConnection.PcPlayer) {
            m_text.text = m_winMessage;
        }else if(p_gameEnd.winningClient == ClientConnection.VrPlayer) {
            m_text.text = m_loseText;
        }else {
            Debug.LogError("GameEnd message reception problem ! Could not determine a winner", this); 
        }
        
        m_text.gameObject.SetActive(true);

        SynchronizePlayerPosition.Instance.m_player.position = m_initialPlayerPosition;
    }
}
