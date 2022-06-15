using System.Collections.Generic;
using CustomMessages;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Synchronizers
{
    public class SynchronizeInitialData : Synchronizer<SynchronizeInitialData>
    {
        [SerializeField] private GameObject m_pcHealthPrefab;
        [SerializeField] private GameObject m_vrHealthPrefab;
        
        [SerializeField] private Texture m_pcHealthTextureEmpty;
        [SerializeField] private Texture m_vrHealthTextureEmpty;

        [SerializeField] private float m_spacingPcHealth;
        [SerializeField] private float m_spacingVrHealth;

        [SerializeField] private GameObject m_waitingForPlayerFeddback;
        [SerializeField] private GameObject[] m_unloadOnInitialData;

        public static bool vrConnected;
        
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
            ClientManager.OnReceiveReadyToFace += ReceiveReady;
        }

        private void ReceiveReady(ReadyToFace p_activateblind)
        {
            m_waitingForPlayerFeddback.SetActive(false);
            vrConnected = true;
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
            
            foreach (var obj in m_unloadOnInitialData) obj.SetActive(false);

            m_pcHealth = p_initialData.healthPcPlayer;
            m_vrHealth = p_initialData.healthVrPlayer;
        }
        
        /// <summary/> The function called when the PC player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LosePcHealth(int p_healthLost = 1)
        {
            m_pcHealth -= p_healthLost;
        }

        /// <summary/> The function called when the VR player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LoseVrHealth(int p_healthLost = 1)
        {
            m_vrHealth -= p_healthLost;
        }
    }
}