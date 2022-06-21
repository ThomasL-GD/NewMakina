using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizerHearts : Synchronizer<SynchronizerHearts> {
        
        [SerializeField] [Tooltip("The parent of the hearts that belong in the arena")] private Transform m_realGameHeartParent;
        [SerializeField] [Tooltip("The parent of the hearts that belong in the lobbby")] private Transform m_lobbyHeartParent;
        [SerializeField] [Tooltip("The prefab of the hearts that will be spawned on server connection")] private GameObject m_heartPrefabs;
        [SerializeField] private Transform[] m_UiLives;
        private byte m_livesLeft = 0;
        private bool m_hpInUiAreWorking = false;
        private bool m_hasDestroyedEverything = false;
        
        [SerializeField] private float m_heartDestructionTime = 6f;
        [SerializeField] private float m_heartLaserFlashPhase = 0.5f;

        private InitialData m_initialDataBuffer;

        private class Heart {
            public GameObject heartObject;
        }
        
        private Heart[] m_hearts;
        
        void Awake() {
            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnReadyToGoIntoTheBowl += ReceiveReadyToGoIntoTheBowl;
            MyNetworkManager.OnReceiveHeartBreak += ReceiveHeartBreak;
            MyNetworkManager.OnReceiveGameEnd += Reset;
        }

        private void ReceiveReadyToGoIntoTheBowl(ReadyToGoIntoTheBowl p_p_ready) {
            if (m_hasDestroyedEverything) {
                CreateHearts();
                m_hasDestroyedEverything = false;
            }
            SetUpUI();
        }

        /// <summary>Destroy every heart to be ready to launch another game </summary>
        /// <param name="p_gameend">The message sent by the server</param>
        private void Reset(GameEnd p_gameend) {

            //Destroy them all （￣ｗ￣）Ψ
            foreach (Heart heart in m_hearts) {
                Destroy(heart.heartObject);
            }

            m_hasDestroyedEverything = true;
        }

        /// <summary/> The function called when the heart heartPositions are updated
        /// <param name="p_initialData"></param>
        private void ReceiveInitialData(InitialData p_initialData) {
            m_initialDataBuffer = p_initialData;
            Transition.a_transitionDone += CreateHearts;
        }

        private void CreateHearts() {
            
            // Making a list of hearts to be able to modify the hearts array
            List<Heart> hearts = new List<Heart>();
            
            // Adding the hearts to the list
            for (int i = 0; i < m_initialDataBuffer.heartPositions.Length; i++) {

                hearts.Add(new Heart(){heartObject = Instantiate(m_heartPrefabs, m_initialDataBuffer.heartPositions[i], m_heartPrefabs.transform.rotation, i < m_initialDataBuffer.firstLobbyHeartIndex ? m_realGameHeartParent : m_lobbyHeartParent)});
                
                
                if (hearts[i].heartObject.TryGetComponent( out HeartIdentifier hi)) {
                    hi.heartIndex = i;
                }
                else {
                    hi = hearts[i].heartObject.AddComponent<HeartIdentifier>();
                    hi.heartIndex = i;
                }
            }

            SetUpUI();

            // Saving the list
            m_hearts = hearts.ToArray();
        }

        private void SetUpUI() {
            m_hpInUiAreWorking = m_initialDataBuffer.healthVrPlayer <= m_UiLives.Length;

            if (!m_hpInUiAreWorking) return;
                m_livesLeft = (byte) m_initialDataBuffer.healthVrPlayer;

                for (var i = 0; i < m_UiLives.Length; i++) {
                    m_UiLives[i].gameObject.SetActive(i <= m_livesLeft); //Setting active as many UI hearts as the number of lives
                }
        }
        
        /// <summary/> Will destroy a heart and play the according sound
        /// <param name="p_heartbreak">The message sent from the server</param>
        private void ReceiveHeartBreak(HeartBreak p_heartbreak) {
            
            // Starting the Coroutine called to break a heart
            StartCoroutine(BreakMyHeart(p_heartbreak.index));

            // Synching the data with the feedback
            SynchronizeInitialData.instance.LoseVrHealth();
        }

        /// <summary/> The Coroutine called to break a heart 
        /// <param name="p_index"> the index of the heart to get destroyed </param>
        /// <returns> null </returns>
        IEnumerator BreakMyHeart(int p_index) {

            if (m_hearts[p_index].heartObject == null) yield break;
            
            // Fetching the heart game object
            GameObject heart = m_hearts[p_index].heartObject;

            m_hearts[p_index].heartObject.GetComponent<HeartIdentifier>().Detonate();
            
            // Starting the Coroutine called to make the heart beacon flash
            Coroutine flash = m_livesLeft > 0 ? StartCoroutine(FlashMyHeart(heart.GetComponent<LineRenderer>())) : null;
            
            // Waiting for the destruction time
            yield return new WaitForSeconds(m_heartDestructionTime);
            
            if(m_livesLeft > 0)StopCoroutine(flash);
            heart.GetComponent<LineRenderer>().enabled = false;
            if(m_hpInUiAreWorking && m_livesLeft > 0) {
                m_UiLives[m_livesLeft - 1].gameObject.SetActive(false);
                m_livesLeft--;
            }

            //Destroying the heart game object
            //Destroy(heart); 
        }

        /// <summary/> The Coroutine called to make the heart beacon flash
        /// <param name="p_heartLineRenderer"></param>
        /// <returns> null </returns>
        IEnumerator FlashMyHeart(LineRenderer p_heartLineRenderer) {
            
            while (p_heartLineRenderer != null)
            {
                p_heartLineRenderer.enabled = !p_heartLineRenderer.enabled;
                if(m_hpInUiAreWorking && m_UiLives[m_livesLeft-1].gameObject != null)m_UiLives[m_livesLeft-1].gameObject.SetActive(!m_UiLives[m_livesLeft-1].gameObject.activeSelf);
                yield return new WaitForSeconds(m_heartLaserFlashPhase);
            }
        }
    }
}