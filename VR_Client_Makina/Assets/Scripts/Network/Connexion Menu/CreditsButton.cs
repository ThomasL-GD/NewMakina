using System;
using CustomMessages;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class CreditsButton : AttackSensitiveButton {

        [SerializeField] private GameObject[] m_creditsObjects;
        [SerializeField] private GameObject[] m_objectsToUnactiveOnCredits;
        [SerializeField] private bool m_isCredits;

        public override void OnBeingActivated() {
            base.OnBeingActivated();

            Transition.a_transitionDone += ShowCredits;
            Transition.Instance.StartTransition();

            if (m_isCredits) {
                MyNetworkManager.OnReadyToGoIntoTheBowl += Reset;
                MyNetworkManager.OnReceiveInitiateLobby += Reset;
            }
        }

        private void Reset(InitiateLobby p_p_ready) => Reset();

        private void Reset(ReadyToGoIntoTheBowl p_p_ready) => Reset();

        private void ShowCredits() {
            foreach (GameObject obj in m_creditsObjects) obj.SetActive(true);
            foreach (GameObject obj in m_objectsToUnactiveOnCredits) obj.SetActive(false);
        }

        private void Reset() {
            foreach (GameObject obj in m_objectsToUnactiveOnCredits) obj.SetActive(false);
        }
    }
}
