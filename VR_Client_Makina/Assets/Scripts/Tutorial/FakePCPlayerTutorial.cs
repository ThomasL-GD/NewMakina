using System;
using CustomMessages;
using Network;
using Network.Connexion_Menu;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorial {

    [RequireComponent(typeof(Emerge))]
    public class FakePCPlayer : AttackSensitiveButton {

        [SerializeField] private Transform[] m_path = null;
        [SerializeField] private Vector3[] m_pathIfDetected = null;
        private Vector3[] m_originalPathStamp = null;
        [SerializeField] private bool m_mustLoopPath = true;
        [SerializeField] private MeshRenderer[] m_renderers;
        [SerializeField, Range(0f, 50f)] private float m_speed = 15f;
        [SerializeField, Range(0f, 5f)] private float m_uncertainty = 15f;

        private int m_currentPathIndex = 0;
        private bool m_isOnAlternative = false;

        private bool m_isRunning = false;

        private void Start() {
            GetComponent<Emerge>().OnEmergeDone += StartRunning;
            foreach (MeshRenderer meshRenderer in m_renderers) {
                meshRenderer.enabled = false;
            }
        }

        /// <summary>Initialize everything in order to start running around and have the correct consequences</summary>
        private void StartRunning() {
            m_isRunning = true;
            foreach (MeshRenderer meshRenderer in m_renderers) {
                meshRenderer.enabled = true;
            }
            LocalBeaconFeedback.fakePcPlayerTarget = this;
        }

        // Update is called once per frame
        void Update() {
            if (!m_isRunning) return;
            Transform transform1 = transform;
            Vector3 position = transform1.position;
            position += ((m_path[m_currentPathIndex].position - position).normalized * (m_speed * Time.deltaTime));
            transform1.position = position;
            transform1.LookAt(m_path[m_currentPathIndex].position);
            
            if (!((transform.position - m_path[m_currentPathIndex].position).magnitude < m_uncertainty)) return; //If we've the position we were seeking
                m_currentPathIndex++;
                
                if (m_currentPathIndex < m_path.Length) return; // If we've reached the end of the array
                    switch (m_mustLoopPath) {
                            case true:
                                m_currentPathIndex = 0;
                                break;
                            case false:
                                m_isRunning = false;
                                break;
                    }
        }

        /// <summary>Guess I'll die</summary>
        private void GuessIllDie() {
            GetComponent<Emerge>().OnEmergeDone -= StartRunning;
            m_isRunning = false;
            LocalBeaconFeedback.fakePcPlayerTarget = null;
            TutorialManager.singleton.StartCoroutine(TutorialManager.singleton.NextStep());
        }

        private void OnDrawGizmosSelected() {
            if (m_path == null) return;
            Gizmos.color = Color.yellow;
            foreach (Transform tran in m_path) {
                Gizmos.DrawWireSphere(tran.position, m_uncertainty);
            }
            Gizmos.color = Color.grey;
            foreach (Vector3 pos in m_pathIfDetected) {
                Gizmos.DrawWireSphere(pos, m_uncertainty);
            }
        }

        public void ChangeToAlternativePath() {
            if(m_isOnAlternative) return;
            m_originalPathStamp = new Vector3[m_path.Length];
            for (int i = 0; i < m_originalPathStamp.Length; i++) {
                m_originalPathStamp[i] = m_path[i].position;
                m_path[i].position = m_pathIfDetected[i];
            }
            m_isOnAlternative = true;
        }

        public void GoBackToOriginalPath() {
            if(!m_isOnAlternative) return;
            for (int i = 0; i < m_originalPathStamp.Length; i++) {
                m_path[i].position = m_originalPathStamp[i];
            }
            m_isOnAlternative = false;
        }

        public void ResetPosition() {
            transform.position = m_path[0].position;
            m_currentPathIndex = 0;
            m_isRunning = true;
        }

        public override void OnBeingActivated() {
            base.OnBeingActivated();
            GuessIllDie();
        }
    }

}
