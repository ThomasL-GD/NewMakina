using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeHearts : Synchronizer
    {
        
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")]
        private GameObject m_heartPrefabs;

        private GameObject[] m_hearts;
        
        /// <summary>
        /// Awake is called as soon as the object is set active
        /// </summary>
        void Awake()
        {
            // Setting up the synchronizers with the delegates
            
            ClientManager.OnReceiveHeartTransforms += SynchronizeHeartTransforms;
            ClientManager.OnReceiveHeartBreak += SynchronizeHeartBreak;
        }
        
        /// <summary>
        ///  Spawning in the hearts
        /// </summary>
        /// <param name="p_heartTransforms"> the network message </param>
        private void SynchronizeHeartTransforms(HeartTransforms p_heartTransforms)
        {
            List<GameObject> hearts = new List<GameObject>();
            for (int i = 0; i < p_heartTransforms.positions.Length; i++)
            {
                hearts.Add(Instantiate(m_heartPrefabs, p_heartTransforms.positions[i], p_heartTransforms.rotations[i]));
                
                // Giving them a heart identifier component
                if (hearts[i].TryGetComponent( out HeartIdentifier hi))
                    hi.heartIndex = i;
                else
                {
                    hi = hearts[i].AddComponent<HeartIdentifier>();
                    hi.heartIndex = i;
                }
            }

            m_hearts = hearts.ToArray();
        }
        
        /// <summary>
        /// Destroying the objects localy when the server tells this client to do it
        /// </summary>
        /// <param name="p_heartbreak"> the network message </param>
        private void SynchronizeHeartBreak(HeartBreak p_heartbreak)
        {
            Destroy(m_hearts[p_heartbreak.index]);
            SynchronizeInitialData.instance.LoseVrHealth();
        }
    }
}