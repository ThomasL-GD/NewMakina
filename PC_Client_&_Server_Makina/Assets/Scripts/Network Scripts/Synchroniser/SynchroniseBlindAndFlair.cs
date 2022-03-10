using System;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Rendering;

public class SynchroniseBlindAndFlair : Synchronizer<SynchroniseBlindAndFlair>
{
    [SerializeField] private Volume m_postProcessEffect;
    [SerializeField] private float m_fadeOutSpeed = .3f;
    
    
    
    void Awake()
    {
        ClientManager.OnReceiveActivateBlind += ActivateBlind;
    }

    private void ActivateBlind(ActivateBlind p_activateflair) => m_postProcessEffect.weight = 1;
    

    private void Update()
    {
        if (m_postProcessEffect.weight <= 0) return;
        m_postProcessEffect.weight -= (1 / m_fadeOutSpeed) * Time.deltaTime;
        if (m_postProcessEffect.weight < 0) m_postProcessEffect.weight = 0;
    }
}
