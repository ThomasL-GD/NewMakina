using System;
using CustomMessages;
using Network;
using Network.Connexion_Menu;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

namespace Tutorial {

    [RequireComponent(typeof(Emerge))]
    public class FakePCPlayer : AttackSensitiveButton {

        [SerializeField] private Transform[] m_path = null;
        [SerializeField] private bool m_mustLoopPath = true;
        [SerializeField] private MeshRenderer[] m_renderers;
        [SerializeField, Range(0f, 50f)] private float m_speed = 15f;
        [SerializeField, Range(0f, 5f)] private float m_uncertainty = 15f;

        private int m_currentPathIndex = 0;

        private bool m_isRunning = true;

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
            LocalLaser.SetNewSensitiveTargetForAll?.Invoke(this);
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
            LocalLaser.SetNewSensitiveTargetForAll?.Invoke(null);
            TutorialManager.singleton.NextStep();
        }

        private void OnDrawGizmosSelected() {
            if (m_path == null) return;
            Gizmos.color = Color.yellow;
            foreach (Transform tran in m_path) {
                Gizmos.DrawWireSphere(tran.position, m_uncertainty);
            }
        }

        public override void OnBeingActivated() {
            base.OnBeingActivated();
            GuessIllDie();
        }
    }

}
