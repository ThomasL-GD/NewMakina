using CustomMessages;
using TMPro;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeGameEnd : Synchronizer
    {
        [SerializeField] private TextMeshPro m_text;
        [SerializeField] private GameObject[] m_GOToSetActive;
        
        /// <summary>
        /// Awake is called before the Start
        /// </summary>
        void Awake()
        {
            m_text.gameObject.SetActive(false);
            MyNetworkManager.OnReceiveGameEnd += GameEnd;
            
            foreach (GameObject go in m_GOToSetActive) {
                go.SetActive(false);
            }
            
        }

        void GameEnd(GameEnd p_gameEnd)
        {
            string winMessage = p_gameEnd.winningClient.ToString();
            winMessage = winMessage.Replace("Player", " Player");
            winMessage = winMessage.Replace("Vr", "Virtual Reality");
            winMessage += " Wins!";
            
            m_text.gameObject.SetActive(true);
            m_text.text = winMessage;

            foreach (GameObject go in m_GOToSetActive) {
                go.SetActive(true);
            }
        }
    }

}