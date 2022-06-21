using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchronizeGameEnd : Synchronizer<SynchronizeGameEnd>
{
    [SerializeField] private GameObject m_winScreen;
    [SerializeField] private GameObject m_defeatScreen;
    [SerializeField] private GameObject m_bowl;
    [SerializeField] private GameObject m_lobby;
    
    private Vector3 m_initialPlayerPosition;
    /// <summary>
    /// Awake is called before the Start
    /// </summary>
    void Awake() {
        ClientManager.OnReceiveGameEnd += GameEnd;
        ClientManager.OnReceiveReadyToFace += Prepare;
        ClientManager.OnReceiveReadyToGoIntoTheBowl += Prepare;
    }

    private void Start()
    {
        m_initialPlayerPosition = SynchronizePlayerPosition.Instance.m_player.position;
    }
    
    private void Prepare(ReadyToFace pReadyToFace) => Prepare();
    private void Prepare(ReadyToGoIntoTheBowl pReadyToFace) => Prepare();
    
    private void Prepare()
    {
        m_winScreen.gameObject.SetActive(false); 
        m_defeatScreen.gameObject.SetActive(false);
    }
    
    void GameEnd(GameEnd p_gameEnd) {
        
        if (p_gameEnd.winningClient == ClientConnection.PcPlayer)
            m_winScreen.gameObject.SetActive(true);
        else if(p_gameEnd.winningClient == ClientConnection.VrPlayer) 
            m_defeatScreen.gameObject.SetActive(true);
        else
            Debug.LogError("GameEnd message reception problem ! Could not determine a winner", this); 
        

        SynchronizePlayerPosition.Instance.m_player.position = m_initialPlayerPosition;
        m_bowl.SetActive(false);
        m_lobby.SetActive(true);
    }
}
