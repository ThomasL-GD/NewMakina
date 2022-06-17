using System.Collections;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    
    public class SynchronizeInitialData : Synchronizer<SynchronizeInitialData> {

        /// <summary/> The PC Health data saved locally
        private int m_pcHealth;
        
        private int m_maxPcHealth;
        
        /// <summary/> The VR Health data saved locally
        private int m_vrHealth;

        private InitialData m_initialData;
        
        /// <summary/> The singleton instance of SynchronizeInitialData
        public static SynchronizeInitialData instance;

        private Coroutine m_healtheedbackCoroutine;
        [SerializeField]private float m_showHealthTime = 3f;
        
        [SerializeField, Tooltip("One of them will be activated every time the VR player gets a kill (in order)")] private Transform[] m_uiPoint;
        
        /// <summary/> Adding functions to the client delegate
        void Awake() {
            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnReadyToGoIntoTheBowl += Initialize;
            MyNetworkManager.OnReadyToFace += Initialize;
        }

        private void Initialize(ReadyToGoIntoTheBowl p_p_ready) => Initialize();
        private void Initialize(ReadyToFace p_p_ready) => Initialize();

        /// <summary/> Singleton type beat
        private void Start() {
            if (instance == null) instance = this;
            else {
                gameObject.SetActive(false);
                Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
            }

            foreach (Transform point in m_uiPoint) point.gameObject.SetActive(false);
        }

        /// <summary/> Function called when the client received the initial data
        /// <param name="p_initialData"> The message sent by the server</param>
        void ReceiveInitialData(InitialData p_initialData) {
            m_initialData = p_initialData;
            Debug.Log($"INITIAL DATA RECEPTION CONFIRMED");
        }

        private void Initialize() {
            m_pcHealth = m_initialData.healthPcPlayer;
            m_maxPcHealth = m_initialData.healthPcPlayer;
            m_vrHealth = m_initialData.healthVrPlayer;
            foreach (Transform uiPoint in m_uiPoint) { uiPoint.gameObject.SetActive(false); }
            UpdateHealthText();
        }

        
        /// <summary/> The function called when the PC player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LosePcHealth(int p_healthLost = 1) {
            if(m_pcHealth <= m_maxPcHealth)m_uiPoint[m_maxPcHealth - m_pcHealth].gameObject.SetActive(true);
            m_pcHealth -= p_healthLost;
            UpdateHealthText();
        }

        
        /// <summary/> The function called when the VR player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LoseVrHealth(int p_healthLost = 1) {
            m_vrHealth -= p_healthLost;
            UpdateHealthText();
        }

        /// <summary/> Updates both player's health on the PC GUI based on the local variables
        [ContextMenu("test")]
        private void UpdateHealthText() {
            if(m_healtheedbackCoroutine != null) StopCoroutine(m_healtheedbackCoroutine);
            m_healtheedbackCoroutine = StartCoroutine(ShowHealthText());
        }

        IEnumerator ShowHealthText() {
            yield return new WaitForSeconds(m_showHealthTime);
        }
    }
}