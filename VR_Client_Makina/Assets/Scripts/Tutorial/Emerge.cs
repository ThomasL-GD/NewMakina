using System.Collections;
using UnityEngine;

namespace Tutorial {

    public class Emerge : MonoBehaviour {

        private Vector3 m_targetPos;
        private Vector3 m_originalPos;

        [SerializeField, Tooltip("Let 0 for automatic")] private float m_heightThatIsConsideredUnderground = 0f;

        private bool m_isEmerged;
#pragma warning disable CS0414
        private bool m_isMoving;
#pragma warning restore CS0414

        public delegate void EmergeStateDelegator();
        public EmergeStateDelegator OnEmergeDone;
        public EmergeStateDelegator OnSinkDone;
        
        private void Start() {
            Vector3 position = transform.position;
            m_targetPos = position;
            transform.position = new Vector3(position.x,  (m_heightThatIsConsideredUnderground == 0) ? TutorialManager.singleton.heightThatIsConsideredUnderground : m_heightThatIsConsideredUnderground, position.z);
            m_originalPos = transform.position;
        }

        public void StartEmerging() {
            StartCoroutine(Emerging(false));
        }

        public void StartDemerging() {
            StartCoroutine(Emerging(true));
        }

        IEnumerator Emerging(bool p_isActuallyDemerging) {
            m_isMoving = true;
            float elapsedTime = 0f;
            float emergingTime = TutorialManager.singleton.emergingTime;
            Vector3 yDifference = Vector3.up * (m_targetPos.y - m_originalPos.y);

            while (elapsedTime < emergingTime) {
                yield return new WaitForFixedUpdate();
                elapsedTime += Time.fixedDeltaTime;
                float ratio = p_isActuallyDemerging ? 1 - (elapsedTime / emergingTime) : elapsedTime / emergingTime;

                transform.position = m_originalPos + (yDifference * TutorialManager.singleton.speedToGoUp.Evaluate(ratio));
            }

            transform.position = p_isActuallyDemerging ? m_originalPos : m_targetPos;
            
            switch (p_isActuallyDemerging) {
                case true:
                    OnSinkDone?.Invoke();
                    break;
                case false:
                    OnEmergeDone?.Invoke();
                    break;
            }
            m_isEmerged = !p_isActuallyDemerging;

            m_isMoving = false;
        }
    }

}
