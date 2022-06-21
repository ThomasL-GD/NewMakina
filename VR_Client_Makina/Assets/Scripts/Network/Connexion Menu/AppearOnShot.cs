using UnityEngine;

namespace Network.Connexion_Menu {

    public class AppearOnShot : AttackSensitiveButton {

        [SerializeField] private GameObject[] m_objectsToActivate = null;
    
        // Start is called before the first frame update
        void OnEnable() {
            if (m_objectsToActivate != null) {
                foreach (GameObject go in m_objectsToActivate) {
                    go.SetActive(false);
                }
            }
        }

        public override void OnBeingActivated() {
            base.OnBeingActivated();
            if (m_objectsToActivate != null) {
                foreach (GameObject go in m_objectsToActivate) {
                    go.SetActive(true);
                }
            }
        }
    }

}
