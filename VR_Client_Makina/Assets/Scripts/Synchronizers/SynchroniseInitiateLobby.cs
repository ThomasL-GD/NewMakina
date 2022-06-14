using System;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchroniseInitiateLobby : Synchronizer<SynchroniseInitiateLobby> {

        private enum GameState {
            Menu,
            Lobby,
            Game
        }

        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToDesactiveWhenOutOfMenu = null;
        
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnStartGame = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnLobby = null;

        private GameState m_currentGameState = GameState.Menu;
    
        // Start is called before the first frame update
        void Start() {
            MyNetworkManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
            
            foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(false);
        }

        public void SetPlayScene() {
            switch (m_currentGameState) {
                case GameState.Menu: {
                    foreach (GameObject go in m_objectToDesactiveWhenOutOfMenu) go.SetActive(false);
                    break;
                }
                case GameState.Lobby: {
                    foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(false);
                    break;
                }
                case GameState.Game:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(true);
            m_currentGameState = GameState.Game;
        }

        private void SetLobbyScene() {
            if(m_currentGameState == GameState.Menu)foreach (GameObject go in m_objectToDesactiveWhenOutOfMenu) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(true);
            m_currentGameState = GameState.Lobby;
        }

        private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby) {
            Debug.Log($"initiate lobby received : {p_initiateLobby.trial}");

            if (p_initiateLobby.trial) {
                Transition.a_transitionDone += SetLobbyScene;
                Transition.Instance.StartTransition();
            }
            else {
                MyNetworkManager.singleton.SendVrData(new ReadyToGoIntoTheBowl());
            }
        }
    }
}