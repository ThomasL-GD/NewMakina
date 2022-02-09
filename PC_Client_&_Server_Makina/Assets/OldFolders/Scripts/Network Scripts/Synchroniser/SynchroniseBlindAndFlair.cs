using CustomMessages;
using Synchronizers;
using UnityEngine;

public class SynchroniseBlindAndFlair : Synchronizer<SynchroniseBlindAndFlair>
{
    [SerializeField] private MoveUp m_flair;
    void Awake()
    {
        ClientManager.OnReceiveInitialData += FlairInitialData;
        ClientManager.OnReceiveActivateFlair += ActivateFlair;
    }

    private void ActivateFlair(ActivateFlair p_activateflair) =>
        m_flair.StartRaise(p_activateflair.startPosition);

    private void FlairInitialData(InitialData p_initialdata) =>
        m_flair.SetRaiseSpeedAndTime(p_initialdata.flairRaiseSpeed,p_initialdata.flairDetonationTime);
}
