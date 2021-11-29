using CustomMessages;
using Network;
using TMPro;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeInitialData : Synchronizer
    {

        [SerializeField][Tooltip("the TMP Text element that keeps track of the PC player's health")] private TextMeshPro m_pcPlayerHealthText;
        [SerializeField][Tooltip("the TMP Text element that keeps track of the VR player's health")] private TextMeshPro m_vrPlayerHealthText;

        /// <summary/> The PC Health data saved localy
        private int m_pcHealth;
        
        /// <summary/> The VR Health data saved localy
        private int m_vrHealth;
        
        /// <summary/> The singleton instance of SynchronizeInitialData
        public static SynchronizeInitialData instance;
        
        /// <summary/> Adding functions to the client delegate
        void Awake() => MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;

        /// <summary/> Singleton type beat
        private void Start()
        {
            if (instance == null) instance = this;
            else
            {
                gameObject.SetActive(false);
                Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
            }
        }

        /// <summary/> Function called when the client received the initial data
        /// <param name="p_initialData"> The message sent by the server</param>
        void ReceiveInitialData(InitialData p_initialData)
        {
            m_pcHealth = p_initialData.healthPcPlayer;
            m_vrHealth = p_initialData.healthVrPlayer;
            UpdateHealthText();
        }

        
        /// <summary/> The function called when the PC player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LosePcHealth(int p_healthLost = 1)
        {
            m_pcHealth -= p_healthLost;
            UpdateHealthText();
        }

        
        /// <summary/> The function called when the VR player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LoseVrHealth(int p_healthLost = 1)
        {
            m_vrHealth -= p_healthLost;
            UpdateHealthText();
        }

        /// <summary/> Updates both player's health on the PC GUI based on the local variables
        private void UpdateHealthText()
        {
            m_pcPlayerHealthText.text = $"Health : {m_vrHealth}";
            m_vrPlayerHealthText.text = $"Enemy Health : {m_pcHealth}";
        }
    }
}