using System;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Rendering;

public class SynchroniseBlindAndFlair : Synchronizer<SynchroniseBlindAndFlair>
{
    [SerializeField] private MoveUp m_flair;
    [SerializeField] private Volume m_postProcessEffect;
    [SerializeField] private float m_fadeOutSpeed = .3f;
    
    
    
    void Awake()
    {
        ClientManager.OnReceiveInitialData += FlairInitialData;
        ClientManager.OnReceiveActivateFlair += ActivateFlair;
        ClientManager.OnReceiveActivateBlind += ActivateBlind;
    }

    private void ActivateFlair(ActivateFlair p_activateflair) =>
        m_flair.StartRaise(p_activateflair.startPosition);

    private void FlairInitialData(InitialData p_initialdata) =>
        m_flair.SetRaiseSpeedAndTime(p_initialdata.flairRaiseSpeed,p_initialdata.flairDetonationTime);

    private void ActivateBlind(ActivateBlind p_activateflair)
    {
        m_postProcessEffect.weight = 1;
        Debug.Log("Blind");
    }

    private void Update()
    {
        if (m_postProcessEffect.weight <= 0) return;
        m_postProcessEffect.weight -= (1 / m_fadeOutSpeed) * Time.deltaTime;
        if (m_postProcessEffect.weight < 0) m_postProcessEffect.weight = 0;
    }
}
