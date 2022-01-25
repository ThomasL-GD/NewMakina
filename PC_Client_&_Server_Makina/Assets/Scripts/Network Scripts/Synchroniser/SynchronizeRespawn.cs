using System.Collections;
using CustomMessages;
using Mirror;
using Player_Scripts;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeRespawn : Synchronizer
    {
        [SerializeField] [Tooltip("The PC player")]
        private GameObject m_player;

        [SerializeField] [Tooltip("The player's respawn points")]
        private Transform[] m_spawnPoints;

        [SerializeField] [Tooltip("The player's respawn Time in seconds")]
        private float m_respawnTime = 3f;

        [SerializeField] [Tooltip("The ui element to inform the players that he's dead")]
        private GameObject m_deathFeedback;

        [SerializeField] [Tooltip("The time for which the player will be invisible on respawn")]
        private float m_invisibilityTime = 5f;

        public delegate void OnPlayerDeathDelegator();

        public static OnPlayerDeathDelegator OnPlayerDeath;
        public delegate void OnPlayerRespawnDelegator();

        public static OnPlayerDeathDelegator OnPlayerRespawn;

        void Awake() => OnPlayerDeath += ReceiveLaser;


        /// <summary>
        /// The function called when the synchroniser receives a laser
        /// </summary>
        /// <param name="p_laser"> the LaserShotInfo </param>
        void ReceiveLaser()
        {
            if (InputMovement3.instance.m_isDead) return;
            StartCoroutine(DeathLoop());
        }

        [ContextMenu("test")]
        void Test()
        {
            StartCoroutine(DeathLoop());
        }

        /// <summary>
        /// The coroutine called that will handle the player's death
        /// </summary>
        /// <returns> null </returns>
        IEnumerator DeathLoop()
        {
            //Enabling the feedback and finding the next spawn point
            InputMovement3.instance.m_isDead = true;
            int respawnIndex = Random.Range(0, m_spawnPoints.Length);
            m_deathFeedback.SetActive(true);

            //Todo Make this no cursed for the love of baby jesus
            m_player.transform.position = Vector3.one * -1000f;
            //Updating the feedback
            SynchronizeInitialData.instance.LosePcHealth();

            
            
            //Waiting for the respawn time
            yield return new WaitForSeconds(m_respawnTime);

            //Disabling the feedback and teleporting the player to his new position
            InputMovement3.instance.m_isDead = false;
            m_deathFeedback.SetActive(false);
            m_player.transform.position = m_spawnPoints[respawnIndex].position;

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