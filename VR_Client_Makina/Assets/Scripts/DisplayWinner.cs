using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class DisplayWinner : MonoBehaviour {

    [SerializeField] private TextMeshPro m_text;
    [SerializeField] private string m_winText = "";
    [SerializeField] private string m_looseText = "";

    private void OnEnable() {
        if (m_winText == "") m_winText = m_text.text;
        if (m_looseText == "") m_winText = m_text.text;

        m_text.text = SynchronizeReadyOrNot.m_gameEnd.winningClient == ClientConnection.VrPlayer ? m_winText : m_looseText;
    }
}
