using System;
using System.Collections;
using CustomMessages;
using Mirror;
using Network;
using TMPro;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeReadyOrNot : Synchronizer<SynchronizeReadyOrNot> {

        private enum GameState {
            Menu,
            Lobby,
            Game,
            EndScreen
        }

        [SerializeField, Range(0f, 60f)] private float m_timeBeforeGoingInEndScene = 2f;
        
        [Space]

        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToDesactiveWhenOutOfMenu = null;
        
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnStartGame = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnLobby = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnLobbyDuringTimer = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnLobbyAfterTimer = null;
        [SerializeField] [Tooltip("self explicit plz")] private GameObject[] m_objectToActiveOnGameEnd = null;

        private GameState m_currentGameState = GameState.Menu;

        private InitiateLobby m_initiateLobby;
        public static GameEnd m_gameEnd { get; private set; } = new GameEnd();
        
        [SerializeField] [Tooltip("This will be set active once the player needs to confirm their readyness, so it better have a ReadyButton somewhere")] private GameObject[] m_objectToActiveOnReady = null;
        [SerializeField] [Tooltip("This will be set unactive once the player needs to confirm their readyness")] private GameObject[] m_objectToDesactiveOnReady = null;

        [Space] [SerializeField] private TextMeshPro m_lobbyTimerText = null;
        
        private void Start() {
            MyNetworkManager.OnReadyToFace += AppearReadyButton;
            MyNetworkManager.OnReadyToGoIntoTheBowl += GoInGame;
            MyNetworkManager.OnReceiveInitialData += DisappearReadyButton;
            MyNetworkManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
            MyNetworkManager.OnReceiveGameEnd += EndGame;
            
            if(m_objectToActiveOnReady == null) {
                Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
                return;
            }

            foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnGameEnd) go.SetActive(false);
            
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobbyAfterTimer) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobbyDuringTimer) go.SetActive(false);
            
            foreach (GameObject go in m_objectToDesactiveWhenOutOfMenu) go.SetActive(true);
        }

        private void AppearReadyButton(ReadyToFace p_ready) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(true);
            foreach (GameObject obj in m_objectToDesactiveOnReady) obj.SetActive(false);
        }

        private void DisappearReadyButton(InitialData p_initialData) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
        }

        private void GoInGame(ReadyToGoIntoTheBowl p_readyToGoIntoTheBowl) {
            Transition.a_transitionDone += SetGameScene;
            Transition.Instance.StartTransition();
        }

        public void GoFromEndScreenToMainMenu() {
            if (m_currentGameState != GameState.EndScreen) return;
                Transition.a_transitionDone += SetMenu;
                Transition.Instance.StartTransition();
        }

        IEnumerator UnlockButton(float p_time) {
            foreach (GameObject go in m_objectToActiveOnLobbyDuringTimer) go.SetActive(true);
            string textBeginning = m_lobbyTimerText != null ? m_lobbyTimerText.text : "";
            float elapsedTime = 0f;
            
            while (elapsedTime < p_time) {
                yield return null;
                elapsedTime += Time.deltaTime;
                if (m_lobbyTimerText != null) m_lobbyTimerText.text = textBeginning + Mathf.Round(elapsedTime) + "/" + Mathf.Round(p_time);
            }
            
            if(m_currentGameState != GameState.Lobby) yield break;
            foreach (GameObject go in m_objectToActiveOnLobbyDuringTimer) go.SetActive(false);
            foreach (GameObject go in m_objectToActiveOnLobbyAfterTimer) go.SetActive(true);
        }

        private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby) {
            m_initiateLobby = p_initiateLobby;
            Transition.a_transitionDone += SetLobbyScene;
            Transition.Instance.StartTransition();
        }

        IEnumerator WaitForEndingScreen() {
            yield return new WaitForSeconds(m_timeBeforeGoingInEndScene);
            if (m_currentGameState != GameState.Game) yield break;
            Transition.a_transitionDone += SetEndScene;
            Transition.Instance.StartTransition();
        }

        private void EndGame(GameEnd p_message) {
            m_gameEnd = p_message;
            StartCoroutine(WaitForEndingScreen());
        }

        private void SetLobbyScene() => SetNewScene(GameState.Lobby);
        private void SetGameScene() => SetNewScene(GameState.Game);
        private void SetEndScene() => SetNewScene(GameState.EndScreen);
        private void SetMenu() => SetNewScene(GameState.Menu);

        private void SetNewScene(GameState p_currentState) {
            switch (m_currentGameState) {
                case GameState.Menu: {
                    foreach (GameObject go in m_objectToDesactiveWhenOutOfMenu) go.SetActive(false);
                    break;
                }
                case GameState.Lobby: {
                    foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(false);
                    foreach (GameObject go in m_objectToActiveOnLobbyAfterTimer) go.SetActive(false);
                    foreach (GameObject go in m_objectToActiveOnLobbyDuringTimer) go.SetActive(false);
                    break;
                }
                case GameState.Game:
                    foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(false);
                    break;
                case GameState.EndScreen:
                    foreach (GameObject go in m_objectToActiveOnGameEnd) go.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (p_currentState) {
                case GameState.Menu: {
                    foreach (GameObject go in m_objectToDesactiveWhenOutOfMenu) go.SetActive(true);
                    if(MyNetworkManager.singleton.m_canSend) foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(true);
                    break;
                }
                case GameState.Lobby: {
                    foreach (GameObject go in m_objectToActiveOnLobby) go.SetActive(true);
                    if (m_initiateLobby.trial) StartCoroutine(UnlockButton(m_initiateLobby.trialTime));
                    else foreach (GameObject go in m_objectToActiveOnLobbyAfterTimer) go.SetActive(true);
                    break;
                }
                case GameState.Game:
                    foreach (GameObject go in m_objectToActiveOnStartGame) go.SetActive(true);
                    break;
                case GameState.EndScreen:
                    foreach (GameObject go in m_objectToActiveOnGameEnd) go.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            m_currentGameState = p_currentState;
        }
    }
}