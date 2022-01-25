using CustomMessages;
using Network;
using UnityEngine;

public class SynchroniseBlindAndFlair : MonoBehaviour
{
    [SerializeField] private MoveUp m_flair;
    [SerializeField] private UIOpacityTransition m_blindObject;
    void Awake()
    {
        MyNetworkManager.OnReceiveInitialData += FlairInitialData;
        MyNetworkManager.OnReceiveActivateFlair += ActivateFlair;
        MyNetworkManager.OnReceiveActivateBlind += ActivateBlindness;
        MyNetworkManager.OnReceiveDeActivateBlind += DeactivateBlindness;
    }

    private void ActivateFlair(ActivateFlair p_activateflair) =>
        m_flair.StartRaise(p_activateflair.startPosition);

    private void FlairInitialData(InitialData p_initialdata) =>
        m_flair.SetRaiseSpeedAndTime(p_initialdata.flairRaiseSpeed,p_initialdata.flairDetonationTime);
    
    void ActivateBlindness(ActivateBlind p_activateBlind) 
    {
        m_blindObject.gameObject.SetActive(true);
        StartCoroutine(m_blindObject.WaitFortransition(Mathf.Max(p_activateBlind.blindIntensity)));
    }

    void DeactivateBlindness(DeActivateBlind p_deActivateBlind) => m_blindObject.gameObject.SetActive(false);
}
