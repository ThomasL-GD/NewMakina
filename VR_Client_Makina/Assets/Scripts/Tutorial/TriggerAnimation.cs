using System.Collections;
using UnityEngine;

namespace Tutorial {

    public class TriggerAnimation : MonoBehaviour {

        private Animator m_animator;

        [SerializeField, Tooltip("Between 0 and 1 only plz")] private AnimationCurve m_triggerPosition;

        [SerializeField] private bool m_animateTrigger = true;
        [SerializeField] private bool m_animateGrip = false;

        private Coroutine m_coroutine;
        private bool m_mustRun = true;
        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private static readonly int Grip = Animator.StringToHash("Grip");

        // Start is called before the first frame update
        void OnEnable() {
            m_animator = GetComponent<Animator>();
            m_mustRun = true;
            m_coroutine = StartCoroutine(LoopAnim());
        }

        IEnumerator LoopAnim() {
            float elapsedTime = 0f;
            while (m_mustRun) {
                if (m_animateTrigger)m_animator.SetFloat(Trigger, m_triggerPosition.Evaluate(elapsedTime));
                if (m_animateGrip)m_animator.SetFloat(Grip, m_triggerPosition.Evaluate(elapsedTime));
                
                yield return new WaitForFixedUpdate();
                elapsedTime += Time.fixedDeltaTime;
            }
        }

        private void OnDestroy() {
            m_mustRun = false;
            if(m_coroutine != null)StopCoroutine(m_coroutine);
        }

        private void OnDisable() {
            m_mustRun = false;
            if(m_coroutine != null)StopCoroutine(m_coroutine);
        }
    }
}