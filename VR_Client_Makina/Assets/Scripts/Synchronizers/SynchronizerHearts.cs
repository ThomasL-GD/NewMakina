using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizerHearts : Synchronizer<SynchronizerHearts>
    {
        
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")]
        private GameObject m_heartPrefabs;
        
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private AudioClip m_heartDestroyedSound = null;
        [SerializeField] private float m_heartDestructionTime = 6f;
        [SerializeField] private float m_heartLazerFlashPhase = 1f;
        
        
        private GameObject[] m_hearts;
        
        void Awake()
        {
            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnReceiveHeartBreak += ReceiveHeartBreak;
        }

        /// <summary/> The function called when the heart heartPositions are updated
        /// <param name="p_initialData"></param>
        private void ReceiveInitialData(InitialData p_initialData)
        {
            // Making a list of hearts to be able to modify the hearts array
            List<GameObject> hearts = new List<GameObject>();
            
            // Adding the hearts to the list
            for (int i = 0; i < p_initialData.heartPositions.Length; i++)
            {
                hearts.Add(Instantiate(m_heartPrefabs, p_initialData.heartPositions[i], p_initialData.heartRotations[i]));
                
                if (hearts[i].TryGetComponent( out HeartIdentifier hi))
                    hi.heartIndex = i;
                else
                {
                    hi = hearts[i].AddComponent<HeartIdentifier>();
                    hi.heartIndex = i;
                }
            }

            // Saving the list
            m_hearts = hearts.ToArray();
        }
        
        /// <summary/> Will destroy a heart and play the according sound
        /// <param name="p_heartbreak">The message sent from the server</param>
        private void ReceiveHeartBreak(HeartBreak p_heartbreak)
        {
            // Starting the Coroutine called to break a heart
            StartCoroutine(BreakMyHeart(p_heartbreak.index));

            // Synching the data with the feedback
            SynchronizeInitialData.instance.LoseVrHealth();
            
            // Audio Feedback
            m_audioSource.Stop();
            m_audioSource.clip = m_heartDestroyedSound;
            m_audioSource.Play();
        }

        /// <summary/> The Coroutine called to break a heart 
        /// <param name="p_index"> the index of the heart to get destroyed </param>
        /// <returns> null </returns>
        IEnumerator BreakMyHeart(int p_index)
        {
            // Fetching the heart game object
            GameObject heart = m_hearts[p_index];

            // Starting the Coroutine called to make the heart beacon flash
            StartCoroutine(FlashMyHeart(heart.GetComponent<LineRenderer>()));
            
            // Waiting for the destruction time
            yield return new WaitForSeconds(m_heartDestructionTime);
            
            //Destroying the heart game object
            Destroy(heart); 
        }

        /// <summary/> The Coroutine called to make the heart beacon flash
        /// <param name="p_heartLineRenderer"></param>
        /// <returns> null </returns>
        IEnumerator FlashMyHeart(LineRenderer p_heartLineRenderer)
        {
            while (p_heartLineRenderer != null)
            {
                p_heartLineRenderer.enabled = !p_heartLineRenderer.enabled;
                yield return new WaitForSeconds(m_heartLazerFlashPhase);
            }
        }
    }
}