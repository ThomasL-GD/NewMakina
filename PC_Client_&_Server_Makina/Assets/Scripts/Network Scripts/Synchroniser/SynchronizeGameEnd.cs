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

    /// <summary> </summary>
    /// <param name="p_p_initialdata"></param>
    private void Prepare(ReadyToPlay p_readyToPlay) => m_text.gameObject.SetActive(false);
    
    void GameEnd(GameEnd p_gameEnd)
    {
        string winMessage = p_gameEnd.winningClient.ToString();
        winMessage = winMessage.Replace("Player", " Player");
        winMessage = winMessage.Replace("Vr", "Virtual Reality");
        winMessage += " Wins!";
        
        m_text.gameObject.SetActive(true);
        m_text.text = winMessage;

        SynchronizePlayerPosition.Instance.m_player.position = m_initialPlayerPosition;
    }
}
