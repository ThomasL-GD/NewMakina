using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchronizeGameEnd : Synchronizer<SynchronizeGameEnd>
{
    [SerializeField] private TextMeshProUGUI m_text;
    
    /// <summary>
    /// Awake is called before the Start
    /// </summary>
    void Awake()
    {
        m_text.gameObject.SetActive(false);
        ClientManager.OnReceiveGameEnd += GameEnd;
    }

    void GameEnd(GameEnd p_gameEnd)
    {
        string winMessage = p_gameEnd.winningClient.ToString();
        winMessage = winMessage.Replace("Player", " Player");
        winMessage = winMessage.Replace("Vr", "Virtual Reality");
        winMessage += " Wins!";
        
        m_text.gameObject.SetActive(true);
        m_text.text = winMessage;
    }
}
