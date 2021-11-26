using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizerHearts : Synchronizer
    {
        
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")]
        private GameObject m_heartPrefabs;
        
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private AudioClip m_heartDestroyedSound = null;

        private GameObject[] m_hearts;
        
        void Awake()
        {
            MyNetworkManager.OnReceiveHeartTransforms += ReceiveHeartTransforms;
            MyNetworkManager.OnReceiveHeartBreak += ReceiveHeartBreak;
        }


        private void ReceiveHeartTransforms(HeartTransforms p_heartTransforms)
        {
            List<GameObject> hearts = new List<GameObject>();
            for (int i = 0; i < p_heartTransforms.positions.Length; i++)
            {
                hearts.Add(Instantiate(m_heartPrefabs, p_heartTransforms.positions[i], p_heartTransforms.rotations[i]));
                
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
        /// Will destroy a heart and play the according sound
        /// </summary>
        /// <param name="p_heartbreak">The message sent from the server</param>
        private void ReceiveHeartBreak(HeartBreak p_heartbreak) {
            
            Destroy(m_hearts[p_heartbreak.index]); 
            
            m_audioSource.Stop();
            m_audioSource.clip = m_heartDestroyedSound;
            m_audioSource.Play();
        } 
    }
}