using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeBombs : SynchronizeLoadedObjectsAbstract {

        [SerializeField] private GameObject m_prefabBomb = null;

        [SerializeField] [Range(0f, 5f)] private float m_explosionTimeOnceTouchingTheGround = 0f;
        [SerializeField] private Color m_colorWhenAlmostExploding = Color.red;

        [SerializeField] private float m_bombScale = 1f;
        
        /*[SerializeField] [Range(1f, 100f)] [Tooltip("This shall be replaced by server data sent to this")] */private float m_explosionRange = 5f;

        [SerializeField] private GameObject m_prefabFxBoomHit = null;
        [SerializeField] private GameObject m_prefabFxBoomMiss = null;

        private class BombInfo {
            public Transform transform;
            public float serverID;

            public BombInfo(Transform p_transform, float p_ID) {
                transform = p_transform;
                serverID = p_ID;
            }
        }

        private List<BombInfo> m_bombs = new List<BombInfo>();
        

        [Header("Sounds")]
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] [Tooltip("If true, nice shot :)\nIf false, crippling emptiness...")] private bool m_niceShotQuestionMark = true;
        [SerializeField] private AudioClip m_niceShotSound = null;
        
        [Header("DropDown")]
        
        [SerializeField] [Tooltip("If true, nice shot :)\nIf false, crippling emptiness...")] private GameObject m_prefabDropDownFeedback;

        protected override void Start() {
            base.Start();
            
            MyNetworkManager.OnReceiveSpawnBomb += SpawnBomb;
            MyNetworkManager.OnReceiveBombExplosion += ExplosionFeedback;
            MyNetworkManager.OnReceiveInitialData += ReceiveIntialData;
            MyNetworkManager.OnReceiveGameEnd += Reset;
        }

        /// <summary>Is called by the OnReceiveGameEnd and destroys every bomb to be ready to launch a new game </summary>
        /// <param name="p_p_gameend">The message sent by the server</param>
        private void Reset(GameEnd p_p_gameend) {

            foreach (BombInfo info in m_bombs) {
                info.transform.GetComponent<BombBehavior>().DestroyMaSoul();
            }
            m_bombs.Clear();
        }

        /// <summary>Just sets maxSlotsForBombs according to the server's InitialData</summary>
        /// <param name="p_initialData">The message sent by the server</param>
        private void ReceiveIntialData(InitialData p_initialData) {
            m_maxSlotsLoading = p_initialData.maximumBombsCount;
        }

        private void Update() {
            if (m_bombs.Count < 1) return;
            BombData[] positionses = new BombData[m_bombs.Count];

            for (int i = 0; i < positionses.Length; i++) {
                positionses[i].position = m_bombs[i].transform.position;
                positionses[i].bombID = m_bombs[i].serverID;
            }
            
            MyNetworkManager.singleton.SendVrData(new BombsPositions(){data = positionses});
        }

        /// <summary> Will simply spawn a bomb lol </summary>
        [ContextMenu("SpawnMenu")]
        private void SpawnBomb(SpawnBomb p_spawnBomb) {
            
            GameObject go = Instantiate(m_prefabBomb);

            if (!go.TryGetComponent(out BombBehavior script)) {
                script = go.AddComponent<BombBehavior>();
            }

            script.m_synchronizer = this;
            script.m_index = m_bombs.Count;
            script.m_serverID = p_spawnBomb.bombID;
            script.m_explosionTimeOnceOnGround = m_explosionTimeOnceTouchingTheGround;
            script.m_mustDropDown = m_dropDown;
            script.m_prefabDropDownFeedback = m_prefabDropDownFeedback;
            
            m_bombs.Add(new BombInfo(go.transform, p_spawnBomb.bombID));
        }

        /// <summary> Call this to make a bomb explode, will send the necessary message to the server </summary>
        /// <param name="p_index">The index of the bomb</param>
        /// <param name="p_serverID">The server ID of the bomb</param>
        public void ExplodeLol(int p_index, float p_serverID) {

            int? index = FindBombFromID(p_index, p_serverID);
            Debug.LogWarning($"The index value is {index} but the initial one was {p_index}, the list contains {m_bombs.Count} elements");
            
            MyNetworkManager.singleton.SendVrData(new BombExplosion(){
                position = m_bombs[index??0].transform.position,
                index = index??0,
                bombID = m_bombs[index??0].serverID,
                hit = false});
            
            m_bombs.RemoveAt(index??0);
        }

        /// <summary> Will display the feedback of an explosion </summary>
        /// <param name="p_bombExplosion">The message received from the server</param>
        private void ExplosionFeedback(BombExplosion p_bombExplosion) {
            //TODO if(p_bombExplosion)
            
            GameObject go = Instantiate(p_bombExplosion.hit?m_prefabFxBoomHit:m_prefabFxBoomMiss, p_bombExplosion.position, Quaternion.Euler(0f, 0f, 0f));
            go.transform.localScale *= m_bombScale;
            go.GetComponent<ParticleSystem>().Play();

            if(p_bombExplosion.hit) SynchronizeInitialData.instance.LosePcHealth();
            
            if (m_niceShotQuestionMark && p_bombExplosion.hit) { // Audio Feedback
                m_audioSource.Stop();
                m_audioSource.clip = m_niceShotSound;
                m_audioSource.Play();
            }
        }

        /// <summary> Will load a bomb in a random available position </summary>
        /// <param name="p_script">The ObjectLoading script of the beacon you want to load</param>
        public void LoadBombRandomly(ObjectLoading p_script) {
            List<int> currentAvailablePositions = new List<int>();
            for(int i = 0; i < m_availblePositions.Length; i++) if(m_availblePositions[i]) currentAvailablePositions.Add(i);

            int random = Random.Range(0, currentAvailablePositions.Count);
            Debug.Log($"Index technically wrong : {random}   (btw, the list has {currentAvailablePositions.Count} elements)");
            LoadObjectFromIndex(p_script, currentAvailablePositions[random]);
        }

        public void ChangeMaterialOfABomb(int p_index, float p_serverID) {
            int index = FindBombFromID(p_index, p_serverID)??-1;
            m_bombs[index].transform.GetComponent<MeshRenderer>().material.color = m_colorWhenAlmostExploding;
        }
        
        /// <summary/> A function to find the index of the bomb that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted bomb </param>
        /// <param name="p_beaconID"> the ID of the wanted bomb </param>
        /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
        private int? FindBombFromID(int p_index, float p_beaconID)
        {
            int index = p_index;
            float ID = p_beaconID;
            List<BombInfo> data = m_bombs;
            if ( index < data.Count && data[index].serverID == ID) return index;

            for (int i = 0; i < data.Count; i++) if (data[i].serverID == ID) return i;

#if UNITY_EDITOR
            Debug.LogWarning("I couldn't find the index matching this ID brother",this);
#endif
            return null;
        }
        
    }

}
