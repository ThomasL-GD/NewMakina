using System;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeReadyOrNot : Synchronizer<SynchronizeReadyOrNot> {

        private enum GameState {
            Menu,
            Lobby,
            Game
        }

        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToDesactiveWhenOutOfMenu = null;
        
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnStartGame = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnLobby = null;

        private GameState m_currentGameState = GameState.Menu;

        
        [SerializeField] [Tooltip("This will be set active once the player needs to confirm their readyness, so it better have a ReadyButton somewhere")] private GameObject[] m_objectToActiveOnReady = null;
        [SerializeField] [Tooltip("This will be set unactive once the player needs to confirm their readyness")] private GameObject[] m_objectToDesactiveOnReady = null;
        
        private void Start() {
            MyNetworkManager.OnReadyToFace += AppearReadyButton;
            MyNetworkManager.OnReadyToGoIntoTheBowl += GoInGame;
            MyNetworkManager.OnReceiveInitialData += DisappearReadyButton;
            MyNetworkManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
            
            if(m_objectToActiveOnReady == null) {
                Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
                return;
            }

            foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(false);
            
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
        }

        private void AppearReadyButton(ReadyToFace p_ready) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(true);
            foreach (GameObject obj in m_objectToDesactiveOnReady) obj.SetActive(false);
        }

        private void DisappearReadyButton(InitialData p_initialData) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
        }

        private void GoInGame(ReadyToGoIntoTheBowl p_readyToGoIntoTheBowl) {
            Transition.a_transitionDone += SetPlayScene;
            Transition.Instance.StartTransition();
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
            Transition.a_transitionDone += SetLobbyScene;
            Transition.Instance.StartTransition();
        }
    }
}