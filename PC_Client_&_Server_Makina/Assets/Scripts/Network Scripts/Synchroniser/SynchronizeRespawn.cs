using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using Player_Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Synchronizers
{
    public class SynchronizeRespawn : Synchronizer<SynchronizeRespawn> {
        
        [SerializeField] [Tooltip("The PC player")]
        private GameObject m_player;

        [SerializeField] [Tooltip("The player's respawn points")]
        public Transform[] m_spawnPoints;

        [SerializeField] [Tooltip("The player's respawn Time in seconds")]
        private float m_respawnTime = 3f;

        [SerializeField] [Tooltip("The ui element to inform the players that he's dead")]
        private GameObject m_deathFeedback;

        [SerializeField] [Tooltip("The time for which the player will be invisible on respawn")]
        private float m_invisibilityTime = 5f;
        
        [SerializeField] private AudioSource m_deathSound;
        [SerializeField] [Tooltip("The sound played when the respawn invisibility ends")] private AudioSource m_invisibilityEndSound;

        [Header("Respawn Panel")]
        [SerializeField] [Tooltip("The GameObjects that will appear on death to chose a point where to spawn")] private GameObject[] m_gosToActivateOnRespawn = null;
        [SerializeField] [Tooltip("The prefab of a spawnPoint, works faster if it has the UISpawnPoint script on it")] private GameObject m_spawnPointPrefab = null;
        
        [SerializeField] [Range(0, 10)] [Tooltip("The number of spawnpoints to choose from when respawning")] private byte m_spawnpointsChoiceNumber = 3;

        public delegate void PlayerDeathDelegator();

        public static PlayerDeathDelegator OnPlayerDeath;

        public static PlayerDeathDelegator OnPlayerRespawn;

        void Awake() {
            OnPlayerDeath += ReceiveLaser;
            ClientManager.OnReceiveInitialData += InitialSpawn;
            
            foreach (GameObject go in m_gosToActivateOnRespawn) go.SetActive(false);
            
#if UNITY_EDITOR
            if(m_spawnpointsChoiceNumber > m_spawnPoints.Length)Debug.LogError("");
#endif
        }

        /// <summary>Teleports the player to a random spawn point on start
        /// I should add that this documentation is very well made (ღゝ◡╹)ノ♡</summary>
        /// <param name="p_initialdata">The message sent by the server, NO SHiT !</param>
        private void InitialSpawn(InitialData p_initialdata) => m_player.transform.position = m_spawnPoints[Random.Range(0, m_spawnPoints.Length)].position;


        /// <summary>
        /// The function called when the synchronizer receives a laser
        /// </summary>
        void ReceiveLaser() {
            if (InputMovement3.instance.m_isDead) return;
            StartCoroutine(DeathLoop());
        }

        /// <summary>
        /// The coroutine called that will handle the player's death
        /// </summary>
        /// <returns> null </returns>
        IEnumerator DeathLoop() {
            
            //Enabling the feedback and finding the next spawn point
            InputMovement3.instance.m_isDead = true;
            int respawnIndex = Random.Range(0, m_spawnPoints.Length);
            m_deathFeedback.SetActive(true);

            m_deathSound.Play();
            
            //Todo Make this no cursed for the love of baby jesus
            m_player.transform.position = Vector3.one * -1000f;
            
            Debug.Log(SynchronizeInitialData.Instance != false);
            
            //Updating the feedback
            SynchronizeInitialData.Instance.LosePcHealth();

            //Waiting for the respawn time
            yield return new WaitForSeconds(m_respawnTime);
            
            //Disabling the feedback
            m_deathFeedback.SetActive(false);

            //If the game is ont running anymore, we don't make it respawn
            //if (!ClientManager.singleton.m_isInGame) yield break;

            //A wild Spawnpoint choice appears
            foreach (GameObject go in m_gosToActivateOnRespawn) go.SetActive(true);

            //Creating a pool to pull random indexes from
            List<ushort> availableSpawnPoints = new List<ushort>();
            for (ushort j = 0; j < m_spawnPoints.Length; j++) availableSpawnPoints.Add(j);

            List<Transform> selectedSpawnPoints = new List<Transform>();
            for (byte i = 0; i < m_spawnpointsChoiceNumber; i++) {
                int rand = Random.Range(0, availableSpawnPoints.Count);
                selectedSpawnPoints.Add(m_spawnPoints[availableSpawnPoints[rand]]);
                availableSpawnPoints.RemoveAt(rand);
                
                //TODO spawn lil dots here
            }
        }

        public void ChooseSpawnPoint(byte p_index) {

            InputMovement3.instance.m_isDead = false;
            
            m_player.transform.position = m_spawnPoints[p_index].position;

            OnPlayerRespawn?.Invoke();
            
        }

        IEnumerator RespawnInvisibility() {

            //Making the player invisible
            NetworkClient.Send(new PcInvisibility() {isInvisible = true});

            //Waiting for the end of the timer
            yield return new WaitForSeconds(m_invisibilityTime);

            //Making the player visible
            NetworkClient.Send(new PcInvisibility() {isInvisible = false});
            m_invisibilityEndSound.Stop();
            m_invisibilityEndSound.Play();
        }

    }
}