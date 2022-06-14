using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeReadyOrNot : Synchronizer<SynchronizeReadyOrNot> {

        [SerializeField] [Tooltip("This will be set active once the player needs to confirm their readyness, so it better have a ReadyButton somewhere")] private GameObject[] m_objectToActiveOnReady = null;
        [SerializeField] [Tooltip("This will be set unactive once the player needs to confirm their readyness")] private GameObject[] m_objectToDesactiveOnReady = null;
        
        private void Start() {
            MyNetworkManager.OnReadyToFace += AppearReadyButton;
            MyNetworkManager.OnReadyToGoIntoTheBowl += GoInGame;
            MyNetworkManager.OnReceiveInitialData += DisappearReadyButton;
            
            if(m_objectToActiveOnReady == null) {
                Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
                return;
            }
            
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
            Transition.a_transitionDone += SynchroniseInitiateLobby.Instance.SetPlayScene;
            Transition.Instance.StartTransition();
        }
    }
}