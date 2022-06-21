using CustomMessages;
using Synchronizers;
using UnityEngine;

public class DisplayWinner : MonoBehaviour {

    [SerializeField] private GameObject m_winObject;
    [SerializeField] private GameObject m_looseObject;

    private void OnEnable() {
        bool won = SynchronizeReadyOrNot.m_gameEnd.winningClient == ClientConnection.VrPlayer;
        m_winObject.SetActive(won);
        m_looseObject.SetActive(!won);
    }
}
