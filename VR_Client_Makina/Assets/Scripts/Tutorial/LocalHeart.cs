using System;
using UnityEngine;

namespace Tutorial {

    public class LocalHeart : MonoBehaviour {
        
        // ReSharper disable once MemberCanBePrivate.Global
        public FakePCPlayer fakePcPlayerTarget = null;
        [SerializeField] private float m_range;
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private AudioClip m_explosionSound;

        private void Update() { 
            if (fakePcPlayerTarget == null) { return; }

            bool isDetectingThisFrame = Vector3.Distance(fakePcPlayerTarget.transform.position, transform.position) < m_range;

            if (isDetectingThisFrame) {
                m_audioSource.Stop();
                m_audioSource.clip = m_explosionSound;
                m_audioSource.Play();
                fakePcPlayerTarget.ResetPosition();
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_range);
        }
    }

}
