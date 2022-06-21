using System;
using CustomMessages;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class ReadyButton : AttackSensitiveButton {

        private enum ReadyType {
            ReadyToFace,
            ReadyToGoInTheBowl,
            RestartGame
        }

        [SerializeField] private GameObject m_waitForOtherToBeReadyGameObject = null;
        [SerializeField] private GameObject[] m_gosToDepopOnShot = null;
        [SerializeField] private ReadyType m_messageToSend = ReadyType.ReadyToFace;


        private void Start() {
#if UNITY_EDITOR
            if (m_waitForOtherToBeReadyGameObject == null) Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
#endif
            m_waitForOtherToBeReadyGameObject.SetActive(false);
            foreach (GameObject goToDepop in m_gosToDepopOnShot) goToDepop.SetActive(true);
        }

        public override void OnBeingActivated() {
            m_waitForOtherToBeReadyGameObject.SetActive(true);
            foreach (GameObject goToDepop in m_gosToDepopOnShot) goToDepop.SetActive(false);
            MyNetworkManager.OnReceiveInitialData += EraseFeedback;
            switch (m_messageToSend) {
                case ReadyType.ReadyToFace:
                    MyNetworkManager.singleton.SendVrData(new ReadyToFace(){});
                    break;
                case ReadyType.ReadyToGoInTheBowl:
                    MyNetworkManager.singleton.SendVrData(new ReadyToGoIntoTheBowl(){});
                    break;
                case ReadyType.RestartGame:
                    MyNetworkManager.singleton.SendVrData(new RestartGame(){});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EraseFeedback(InitialData p_initialData) {
            m_waitForOtherToBeReadyGameObject.SetActive(false);
        }
    
#if UNITY_EDITOR
        [ContextMenu("SetReady")]
        private void SetReadyDebug() {
            switch (m_messageToSend) {
                case ReadyType.ReadyToFace:
                    MyNetworkManager.singleton.SendVrData(new ReadyToFace(){});
                    break;
                case ReadyType.ReadyToGoInTheBowl:
                    MyNetworkManager.singleton.SendVrData(new ReadyToGoIntoTheBowl(){});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
    }

}