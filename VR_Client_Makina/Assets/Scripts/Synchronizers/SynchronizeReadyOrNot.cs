using System;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeReadyOrNot : Synchronizer<SynchronizeReadyOrNot> {

        [SerializeField] [Tooltip("This will be set active once the player needs to confirm their readyness, so it better have a ReadyButton somewhere")] private GameObject[] m_objectToActiveOnReady = null;
        private void Start() {
            MyNetworkManager.OnReadyToPlay += AppearReadyButton;
            MyNetworkManager.OnReceiveInitialData += DisappearReadyButton;
            
            if(m_objectToActiveOnReady == null) {
                Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
                return;
            }
            
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
        }

        private void AppearReadyButton(ReadyToPlay p_ready) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(true);
        }

        private void DisappearReadyButton(InitialData p_initialData) {
            foreach (GameObject obj in m_objectToActiveOnReady) obj.SetActive(false);
        }
    }

}
