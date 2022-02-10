using CustomMessages;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class ReadyButton : AttackSensitiveButton {

        [SerializeField] private GameObject m_waitForOtherToBeReadyGameObject = null;


        private void Start() {
#if UNITY_EDITOR
            if (m_waitForOtherToBeReadyGameObject == null) Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
#endif
            m_waitForOtherToBeReadyGameObject.SetActive(false);
        }

        public override void OnBeingActivated() {
            m_waitForOtherToBeReadyGameObject.SetActive(true);
            MyNetworkManager.OnReceiveInitialData += EraseFeedback;
            MyNetworkManager.singleton.SendVrData(new ReadyToPlay(){});
        }

        private void EraseFeedback(InitialData p_initialdata) {
            m_waitForOtherToBeReadyGameObject.SetActive(false);
        }
    }

}
