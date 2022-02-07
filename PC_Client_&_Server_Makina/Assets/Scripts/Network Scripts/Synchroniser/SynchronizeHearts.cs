using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeHearts : Synchronizer<SynchronizeHearts>
    {
        
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")]
        private GameObject m_heartPrefabs;

        /// <summary/> the gameobjects that will represent the hearts on the pc side
        private GameObject[] m_hearts;
        
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

        private void Reset(GameEnd p_p_gameend) {

            //Destroy them all （￣ｗ￣）Ψ
            foreach (GameObject heart in m_hearts) {
                Destroy(heart);
            }

            m_hearts = new GameObject[]{ };
        }

        /// <summary>
        ///  Spawning in the hearts
        /// </summary>
        /// <param name="p_heartTransforms"> the network message </param>
        private void SynchronizeInitialDataLocal(InitialData p_initialData)
        {
            
            // Creating a list to be able to iterate on the hearts
            List<GameObject> hearts = new List<GameObject>();
            
            // Fetching all the hearts and adding them to the list
            for (int i = 0; i < p_initialData.heartPositions.Length; i++)
            {
                //Todo : set a rotation
                hearts.Add(Instantiate(m_heartPrefabs, p_initialData.heartPositions[i], m_heartPrefabs.transform.rotation));
                
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
        
        /// <summary>
        /// Destroying the objects localy when the server tells this client to do it
        /// </summary>
        /// <param name="p_heartbreak"> the network message </param>
        private void SynchronizeHeartBreak(HeartBreak p_heartbreak)
        {
            Destroy(m_hearts[p_heartbreak.index]);
            SynchronizeInitialData.Instance.LoseVrHealth();
        }
    }
}