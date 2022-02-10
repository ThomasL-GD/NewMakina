using System;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeReadyOrNot : Synchronizer<SynchronizeReadyOrNot> {

        [SerializeField] [Tooltip("This will be set active once the player needs to confirm their readyness, so it better have a ReadyButton somewhere")] private GameObject m_objectToActiveOnReady = null;
        private void Start() {
#if UNITY_EDITOR
            if(m_objectToActiveOnReady == null)Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
#endif
            m_objectToActiveOnReady.SetActive(false);
            MyNetworkManager.OnReadyToPlay += AppearReadyButton;
            MyNetworkManager.OnReceiveInitialData += DisappearReadyButton;
        }

        private void AppearReadyButton(ReadyToPlay p_ready) {
            m_objectToActiveOnReady.SetActive(true);
        }

        private void DisappearReadyButton(InitialData p_initialData) {
            m_objectToActiveOnReady.SetActive(false);
        }
    }

}
