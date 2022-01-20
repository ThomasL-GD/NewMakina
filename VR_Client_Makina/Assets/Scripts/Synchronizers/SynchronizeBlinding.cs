using UnityEngine;
using Synchronizers;
using Network;
using CustomMessages;
public class SynchronizeBlinding : Synchronizer
{
    [SerializeField] private GameObject m_blindObject;
    void Awake()
    {
        MyNetworkManager.OnReceiveActivateBlind += ActivateBlindness;
        MyNetworkManager.OnReceiveDeActivateBlind += DeactivateBlindness;
    }

    void ActivateBlindness(ActivateBlind p_activateBlind) => m_blindObject.SetActive(true);

    void DeactivateBlindness(DeActivateBlind p_deActivateBlind) => m_blindObject.SetActive(false);
}
