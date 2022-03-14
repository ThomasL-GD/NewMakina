using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizerHearts : Synchronizer<SynchronizerHearts> {
        
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")] private GameObject m_heartPrefabs;
        [SerializeField,Tooltip("The prefab of the area around a heart \n WARNING : Will be rescaled leave at 1,1,1 in prefab")] private GameObject m_heartRangePrefab;

        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private AudioClip m_heartDestroyedSound = null;
        [SerializeField] private float m_heartDestructionTime = 6f;
        [SerializeField] private float m_heartLaserFlashPhase = 0.5f;

        private class Heart {
            public GameObject heartObject;
            public GameObject heartRange;
        }
        
        private Heart[] m_hearts;
        
        void Awake()
        {
            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnReceiveHeartBreak += ReceiveHeartBreak;
            MyNetworkManager.OnReceiveGameEnd += Reset;
        }

        /// <summary>Destroy every heart to be ready to launch another game </summary>
        /// <param name="p_gameend">The message sent by the server</param>
        private void Reset(GameEnd p_gameend) {

            //Destroy them all （￣ｗ￣）Ψ
            foreach (Heart heart in m_hearts) {
                Destroy(heart.heartObject);
                Destroy(heart.heartRange);
            }
        }

        /// <summary/> The function called when the heart heartPositions are updated
        /// <param name="p_initialData"></param>
        private void ReceiveInitialData(InitialData p_initialData) {
            
            // Making a list of hearts to be able to modify the hearts array
            List<Heart> hearts = new List<Heart>();
            
            // Adding the hearts to the list
            for (int i = 0; i < p_initialData.heartPositions.Length; i++) {
                
                //TODO : Change that once it has changed
                GameObject radius = Instantiate(m_heartRangePrefab, p_initialData.heartPositions[i],m_heartRangePrefab.transform.rotation);
                radius.transform.localScale = p_initialData.heartRange * 2f * Vector3.one;
                
                hearts.Add(new Heart(){heartObject = Instantiate(m_heartPrefabs, p_initialData.heartPositions[i], m_heartPrefabs.transform.rotation),heartRange = radius});
                
                radius.transform.parent = hearts[i].heartObject.transform;
                
                if (hearts[i].heartObject.TryGetComponent( out HeartIdentifier hi))
                    hi.heartIndex = i;
                else
                {
                    hi = hearts[i].heartObject.AddComponent<HeartIdentifier>();
                    hi.heartIndex = i;
                }
            }

            // Saving the list
            m_hearts = hearts.ToArray();
        }
        
        /// <summary/> Will destroy a heart and play the according sound
        /// <param name="p_heartbreak">The message sent from the server</param>
        private void ReceiveHeartBreak(HeartBreak p_heartbreak) {
            
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
        IEnumerator BreakMyHeart(int p_index) {
            
            // Fetching the heart game object
            GameObject heart = m_hearts[p_index].heartObject;

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
        IEnumerator FlashMyHeart(LineRenderer p_heartLineRenderer) {
            
            while (p_heartLineRenderer != null)
            {
                p_heartLineRenderer.enabled = !p_heartLineRenderer.enabled;
                yield return new WaitForSeconds(m_heartLaserFlashPhase);
            }
        }
    }
}