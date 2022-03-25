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
        
        [SerializeField] private RectTransform m_pcHealthTransform;
        [SerializeField] private RectTransform m_vrHealthTransform;

        [SerializeField] private GameObject m_waitingForPlayerFeddback;
        [SerializeField] private GameObject[] m_unloadOnInitialData;

        private List<GameObject> m_pcHealthObjects;
        private List<GameObject> m_vrHealthObjects;

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
            ClientManager.OnReceiveReadyToPlay += ReceiveReady;
        }

        private void ReceiveReady(ReadyToPlay p_activateblind)
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
            
            InitializeHealth();
        }
        
        /// <summary/> The function called when the PC player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LosePcHealth(int p_healthLost = 1)
        {
            m_pcHealth -= p_healthLost;
            UpdateHealthPC();
        }

        /// <summary/> The function called when the VR player looses Health
        /// <param name="p_healthLost"> the amount of health left (default : 1) </param>
        public void LoseVrHealth(int p_healthLost = 1)
        {
            m_vrHealth -= p_healthLost;
            UpdateHealthVR();
        }

        
        /// <summary/> Updates both player's health on the PC GUI based on the local variables
        [ContextMenu("test")]
        private void InitializeHealth()
        {
            m_pcHealthObjects = new List<GameObject>();
            m_vrHealthObjects = new List<GameObject>();
            
            for (int i = 0; i < m_pcHealth; i++)
            {
                m_pcHealthObjects.Add(Instantiate(m_pcHealthPrefab,m_pcHealthTransform));
                RectTransform rt = m_pcHealthObjects[i].GetComponent<RectTransform>();
                var pivot = m_pcHealthTransform.pivot;
                Debug.Log(pivot);
                rt.anchorMin = new Vector2(pivot.x + m_spacingPcHealth * (i + .5f) - m_spacingPcHealth * m_pcHealth / 2, pivot.y);
                rt.anchorMax = rt.anchorMin;
            }
            
            for (int i = 0; i < m_vrHealth; i++)
            {
                m_vrHealthObjects.Add(Instantiate(m_vrHealthPrefab,m_vrHealthTransform));
                RectTransform rt = m_vrHealthObjects[i].GetComponent<RectTransform>();
                var pivot = m_vrHealthTransform.pivot;
                Debug.Log(pivot);
                rt.anchorMin = new Vector2(pivot.x + m_spacingVrHealth * (i + .5f) - m_spacingVrHealth * m_vrHealth / 2, pivot.y);
                rt.anchorMax = rt.anchorMin;
            }
        }
        
        private void UpdateHealthPC()
        {
            if (m_pcHealth != -1 && m_pcHealth != m_pcHealthObjects.Count)
                m_pcHealthObjects[m_pcHealth].GetComponent<RawImage>().texture = m_pcHealthTextureEmpty;
        }
        private void UpdateHealthVR()
        {
            if (m_vrHealth != -1 && m_vrHealth != m_vrHealthObjects.Count)
                m_vrHealthObjects[m_vrHealth].GetComponent<RawImage>().texture = m_vrHealthTextureEmpty;
        }
    }
}