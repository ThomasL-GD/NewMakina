using CustomMessages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synchronizers
{
    public class SynchronizeInitialData : Synchronizer<SynchronizeInitialData>
    {

        [SerializeField][Tooltip("the GUI element that keeps track of the PC player's health")] private RawImage m_pcPlayerHealth;
        [SerializeField][Tooltip("the GUI element that keeps track of the VR player's health")] private RawImage m_vrPlayerHealth;

        [SerializeField] private Texture[] m_pcHealthTextures;
        [SerializeField] private Texture[] m_vrHealthTextures;

        [SerializeField] private GameObject m_waitingForPlayerFeddback;
        [SerializeField] private GameObject[] m_unloadOnInitialData;
        /// <summary/> The PC Health data saved localy
        private int m_pcHealth;
        
        /// <summary/> The VR Health data saved localy
        private int m_vrHealth;


        /// <summary/> Adding functions to the client delegate
        void Awake()
        {
            Reset();
            ClientManager.OnReceiveGameEnd += Reset;
            ClientManager.OnReceiveInitialData += ReceiveInitialData;
        }
        
        private void Reset(GameEnd p_gameEnd = new GameEnd())
        {
            foreach (var obj in m_unloadOnInitialData) obj.SetActive(true);
        }
        
        /// <summary/> Singleton type beat
        private void Start()
        {
            m_waitingForPlayerFeddback.SetActive(true);
        }

        /// <summary/> Function called when the client received the initial data
        /// <param name="p_initialData"> The message sent by the server</param>
        void ReceiveInitialData(InitialData p_initialData)
        {
            m_waitingForPlayerFeddback.SetActive(false);
            
            foreach (var obj in m_unloadOnInitialData) obj.SetActive(false);

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
        [ContextMenu("test")]
        private void UpdateHealthText()
        {
            m_pcPlayerHealth.texture = m_pcHealthTextures[m_pcHealth];
            m_vrPlayerHealth.texture = m_vrHealthTextures[m_vrHealth];
        }
    }
}