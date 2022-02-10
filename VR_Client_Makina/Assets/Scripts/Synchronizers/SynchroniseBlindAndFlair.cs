using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

public class SynchroniseBlindAndFlair : Synchronizer<SynchroniseBlindAndFlair>
{
    [SerializeField] private MoveUp m_flair;
    [SerializeField] private UIOpacityTransition m_blindObject;
    private Coroutine m_coco;

    void Awake() {
        MyNetworkManager.OnReceiveInitialData += FlairInitialData;
        MyNetworkManager.OnReceiveActivateFlair += ActivateFlair;
        MyNetworkManager.OnReceiveActivateBlind += ActivateBlindness;
        MyNetworkManager.OnReceiveDeActivateBlind += DeactivateBlindness;
    }

    private void ActivateFlair(ActivateFlair p_activateflair) =>
        m_flair.StartRaise(p_activateflair.startPosition);

    private void FlairInitialData(InitialData p_initialdata) {
        m_flair.Reset();
        m_flair.SetRaiseSpeedAndTime(p_initialdata.flairRaiseSpeed,p_initialdata.flairDetonationTime);
        DeactivateBlindness(new DeActivateBlind());
    }
    
    void ActivateBlindness(ActivateBlind p_activateBlind) 
    {
        m_blindObject.gameObject.SetActive(true);
        if(m_coco != null) StopCoroutine(m_coco);
        m_coco = StartCoroutine(m_blindObject.WaitFortransition(Mathf.Max(p_activateBlind.blindIntensity-1f,0)));
    }

    void DeactivateBlindness(DeActivateBlind p_deActivateBlind) => m_blindObject.gameObject.SetActive(false);
}
