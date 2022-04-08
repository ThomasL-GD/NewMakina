using System.Collections;
using CustomMessages;
using Mirror;
using Player_Scripts;
using UnityEngine;

namespace Synchronizers
{
    public class SynchronizeRespawn : Synchronizer<SynchronizeRespawn>
    {
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

        public delegate void OnPlayerDeathDelegator();

        public static OnPlayerDeathDelegator OnPlayerDeath;

        public static OnPlayerDeathDelegator OnPlayerRespawn;

        void Awake() {
            OnPlayerDeath += ReceiveLaser;
            ClientManager.OnReceiveInitialData += InitialSpawn;
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
        IEnumerator DeathLoop()
        {
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
            
            //Disabling the feedback and teleporting the player to his new position
            InputMovement3.instance.m_isDead = false;
            m_deathFeedback.SetActive(false);

            //TODO this might be causing isue, w are 10h before a jury and i am alone
            //If the game is ont running anymore, we don't make it respawn
            if (!ClientManager.singleton.m_isInGame) yield break;
            
            m_player.transform.position = m_spawnPoints[respawnIndex].position;

            OnPlayerRespawn?.Invoke();

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