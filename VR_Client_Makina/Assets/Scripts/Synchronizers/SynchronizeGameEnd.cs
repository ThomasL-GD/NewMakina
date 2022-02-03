using CustomMessages;
using Network;
using TMPro;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeGameEnd : Synchronizer<SynchronizeGameEnd>
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
            MyNetworkManager.OnReceiveInitialData += Reset;
            
            foreach (GameObject go in m_GOToSetActive) {
                go.SetActive(false);
            }
            
        }

        /// <summary>Does the opposite of GameEnd to be sure nothing is displayed when a new game starts </summary>
        /// <param name="p_initialdata">The message sent by the server</param>
        private void Reset(InitialData p_initialdata) {
            m_text.gameObject.SetActive(false);
            foreach (GameObject go in m_GOToSetActive) { go.SetActive(false); }
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