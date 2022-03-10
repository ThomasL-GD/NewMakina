using System;
using UnityEngine;

namespace Tutorial {

    [RequireComponent(typeof(InflateToSize))]
    public class LocalBeaconFeedback : MonoBehaviour {

        public static Transform fakePcPlayerTarget = null;
        private bool m_isDetecting = false;
        private float m_range;
        private static readonly int CodeBeaconColor = Shader.PropertyToID("_Beacon_Color");
        
        // Start is called before the first frame update
        private void Start() {
            InflateToSize script = GetComponent<InflateToSize>();
            m_range = TutorialManager.singleton.beaconRange;
            script.m_targetScale = m_range;
            script.StartInflating();
        }

        private void Update() { 
            if (fakePcPlayerTarget == null) {
                if (!m_isDetecting) return;
                    m_isDetecting = false;
                    ActualiseColor();
                    return;
            }

            bool isDetectingThisFrame = Vector3.Distance(fakePcPlayerTarget.position, transform.position) < m_range;

            if (m_isDetecting == isDetectingThisFrame) return;
                m_isDetecting = isDetectingThisFrame;
                ActualiseColor();
        }
        
        private void ActualiseColor() {

            Color newColor;
            
            switch (m_isDetecting) {
                case true:
                    newColor = Color.green;
                    break;
                case false:
                    newColor = Color.red;
                    break;
            }
            
            GetComponent<MeshRenderer>().material.SetColor(CodeBeaconColor, newColor);
            
        }
    }
}