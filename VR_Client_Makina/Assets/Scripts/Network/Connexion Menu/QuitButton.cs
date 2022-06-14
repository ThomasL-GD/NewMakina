using System.Collections;
using TMPro;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class QuitButton : AttackSensitiveButton {

        [SerializeField] private TextMeshPro m_text = null;
        [SerializeField] private string m_areYouSureMessage;
        [SerializeField, Range(1f, 300f)] private float m_timeToReset = 10f;
        private string m_originalMessage;
        private bool m_hasAlreadyBeenShotOnce = false;
        private Coroutine m_resetCoroutine;

        private void OnEnable() {
            m_originalMessage = m_text.text;
            Initialize();
        }

        public override void OnBeingActivated() {
            base.OnBeingActivated();

            if (!m_hasAlreadyBeenShotOnce) {
                m_text.text = m_areYouSureMessage;
                m_hasAlreadyBeenShotOnce = true;
                m_resetCoroutine = StartCoroutine(ResetAlreadyShot());
            }
            else {
                Debug.Log("Quit motherfucker");
                Application.Quit();
            }
        }

        IEnumerator ResetAlreadyShot() {
            yield return new WaitForSeconds(m_timeToReset);
            Initialize();
        }

        private void Initialize() {
            m_hasAlreadyBeenShotOnce = false;
            m_text.text = m_originalMessage;
            if(m_resetCoroutine != null)StopCoroutine(m_resetCoroutine);
        }
    }
}