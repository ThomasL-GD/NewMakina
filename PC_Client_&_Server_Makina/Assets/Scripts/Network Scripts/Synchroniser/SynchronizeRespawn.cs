using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using Player_Scripts;
using UnityEngine;

namespace Synchronizers {
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

        private ushort m_numberOfSpawnPointsToSend;

        public delegate void PlayerDeathDelegator();

        public static PlayerDeathDelegator OnPlayerDeath;

        public static PlayerDeathDelegator OnPlayerRespawn;

        void Awake() {
            OnPlayerDeath += ReceiveLaser;
            ClientManager.OnReceiveReadyToGoIntoTheBowl += ReceiveReadyToGoIntoTheBowl;
            ClientManager.OnReceiveInitialData += SetInitialValues;
        }

        private void SetInitialValues(InitialData p_initialdata)
        {
            m_numberOfSpawnPointsToSend = p_initialdata.numberOfSpawnPointsToDisplay;
        }

        /// <summary>Teleports the player to a random spawn point on start.
        /// Also fetch any data needed in InitialData
        /// I should add that this documentation is very well made (ღゝ◡╹)ノ♡</summary>
        /// <param name="p_readyToGoIntoTheBowl">The message sent by the server, NO SHiT !</param>
        private void ReceiveReadyToGoIntoTheBowl(ReadyToGoIntoTheBowl p_readyToGoIntoTheBowl) {
            Vector3 spawnPos = m_spawnPoints[Random.Range(0, m_spawnPoints.Length)].position;
            FadeToBlack.FadeToBlackNow?.Invoke(m_player.transform, spawnPos);
        }

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
            if (ClientManager.singleton.m_isInGame) m_deathFeedback.SetActive(true);
            
            //Todo Make this no cursed for the love of baby jesus
            m_player.transform.position = Vector3.one * -1000f;
            
            Debug.Log(SynchronizeInitialData.Instance != false);
            
            //Updating the feedback
            SynchronizeInitialData.Instance.LosePcHealth();

            //Waiting for the respawn time
            yield return new WaitForSeconds(m_respawnTime);
            
            //Disabling the feedback and teleporting the player to his new position
            InputMovement3.instance.m_isDead = false;
            m_deathFeedback.SetActive(false);

            //TODO this might be causing isue, w are 10h before a jury and i am alone
            //If the game is ont running anymore, we don't make it respawn
            if (!ClientManager.singleton.m_isInGame) yield break;
            
            //Creating a pool to pull random indexes from
            List<ushort> availableSpawnPoints = new List<ushort>();
            for (ushort j = 0; j < m_spawnPoints.Length; j++) availableSpawnPoints.Add(j);

            //Create a list of spawnpoints with no duplicates
            List<Vector3> spawnPointsShownToVR = new List<Vector3>();
            for (int j = 0; j < m_numberOfSpawnPointsToSend; j++) {
                ushort rand = (ushort)Random.Range(0, availableSpawnPoints.Count);
                spawnPointsShownToVR.Add(m_spawnPoints[availableSpawnPoints[rand]].position);
                availableSpawnPoints.RemoveAt(rand);
            }
            
            m_player.transform.position = spawnPointsShownToVR[0]; //We spawn the player on the first
            NetworkClient.Send(new PotentialSpawnPoints(){position = spawnPointsShownToVR.ToArray()});

            OnPlayerRespawn?.Invoke();

            //Making the player invisible
            NetworkClient.Send(new PcInvisibility() {isInvisible = true});

            //Waiting for the end of the timer
            yield return new WaitForSeconds(m_invisibilityTime);

            //Making the player visible
            NetworkClient.Send(new PcInvisibility() {isInvisible = false});
        }

    }
}