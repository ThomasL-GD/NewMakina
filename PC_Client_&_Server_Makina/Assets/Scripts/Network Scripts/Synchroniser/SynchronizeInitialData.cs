using CustomMessages;
using TMPro;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeInitialData : Synchronizer
    {

        [SerializeField] private TextMeshProUGUI m_pcPlayerHealthText;
        [SerializeField] private TextMeshProUGUI m_vrPlayerHealthText;

        private int m_pcHealth;
        private int m_vrHealth;
        
        public static SynchronizeInitialData instance;
        
        void Awake() => ClientManager.OnReceiveInitialData += ReceiveInitialData;

        private void Start()
        {
            if (instance == null) instance = this;
            else
            {
                gameObject.SetActive(false);
                Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
            }
        }

        // Update is called once per frame
        void ReceiveInitialData(InitialData p_initialData)
        {
            m_pcHealth = p_initialData.healthPcPlayer;
            m_vrHealth = p_initialData.healthVrPlayer;
            UpdateHealthText();
        }

        public void LosePcHealth()
        {
            m_pcHealth--;
            UpdateHealthText();
        }

        public void LoseVrHealth()
        {
            m_vrHealth--;
            UpdateHealthText();
        }

        private void UpdateHealthText()
        {
            m_pcPlayerHealthText.text = $"Health : {m_pcHealth}";
            m_vrPlayerHealthText.text = $"Enemy Health : {m_vrHealth}";
        }
    }
}