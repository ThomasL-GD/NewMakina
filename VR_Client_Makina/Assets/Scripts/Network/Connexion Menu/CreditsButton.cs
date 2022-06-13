using UnityEngine;

namespace Network.Connexion_Menu {

    public class CreditsButton : AttackSensitiveButton {

        [SerializeField] private GameObject[] m_creditsObjects;
        [SerializeField] private GameObject[] m_objectsToUnactiveOnCredits;

        public override void OnBeingActivated() {
            base.OnBeingActivated();

            Transition.a_transitionDone += ShowCredits;
            Transition.Instance.StartTransition();
        }

        private void ShowCredits() {
            foreach (GameObject obj in m_creditsObjects) obj.SetActive(true);
            foreach (GameObject obj in m_objectsToUnactiveOnCredits) obj.SetActive(false);
        }
    }
}
