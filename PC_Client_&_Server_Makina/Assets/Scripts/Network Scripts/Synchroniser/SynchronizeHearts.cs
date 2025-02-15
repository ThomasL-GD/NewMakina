using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeHearts : Synchronizer<SynchronizeHearts>
    {
        
        [SerializeField,Tooltip("The prefab of the hearts that will be spawned on server connection")] private GameObject m_heartPrefabs;

        [SerializeField] private Transform m_bowl;
        [SerializeField] private Transform m_lobby;
        
        /// <summary/> the gameobjects that will represent the hearts on the pc side
        public GameObject[] m_hearts;

        private InitialData m_initialData;

        /// <summary>
        /// Awake is called as soon as the object is set active
        /// </summary>
        void Awake()
        {
            // Setting up the synchronizers with the delegates
            
            ClientManager.OnReceiveInitialData += SynchronizeInitialDataLocal;
            ClientManager.OnReceiveGameEnd += Reset;
            ClientManager.OnReceiveHeartBreak += SynchronizeHeartBreak;
        }

        private void Reset(GameEnd p_gameend) {

            //Destroy them all （￣ｗ￣）Ψ
            foreach (GameObject heart in m_hearts) {
                Destroy(heart);
            }

            m_hearts = new GameObject[]{};
            SynchronizeInitialDataLocal(m_initialData);
        }

        /// <summary>
        ///  Spawning in the hearts
        /// </summary>
        /// <param name="p_heartTransforms"> the network message </param>
        private void SynchronizeInitialDataLocal(InitialData p_initialData)
        {
            m_initialData = p_initialData;
            // Creating a list to be able to iterate on the hearts
            List<GameObject> hearts = new List<GameObject>();
            
            // Fetching all the hearts and adding them to the list
            for (int i = 0; i < p_initialData.heartPositions.Length; i++)
            {
                //Todo : set a rotation
                GameObject heart = Instantiate(m_heartPrefabs, p_initialData.heartPositions[i], m_heartPrefabs.transform.rotation);
                hearts.Add(heart);
                
                Transform parent = i < p_initialData.firstLobbyHeartIndex ? m_bowl: m_lobby;
                
                heart.transform.SetParent(parent);
                
                // Giving them a heart identifier component
                if (hearts[i].TryGetComponent( out HeartIdentifier hi))
                    hi.heartIndex = i;
                else
                {
                    hi = hearts[i].AddComponent<HeartIdentifier>();
                    hi.heartIndex = i;
                }
            }

            // Converting the list to a stable array
            m_hearts = hearts.ToArray();
        }
        
        /// <summary/> Destroying the objects locally when the server tells this client to do it
        /// <param name="p_heartbreak"> the network message </param>
        private void SynchronizeHeartBreak(HeartBreak p_heartbreak)
        {
            int index = p_heartbreak.index;
            if(m_hearts.Length > index) m_hearts[index].GetComponent<HeartIdentifier>().Break();
            
            SynchronizeInitialData.Instance.LoseVrHealth();
        }
    }
}