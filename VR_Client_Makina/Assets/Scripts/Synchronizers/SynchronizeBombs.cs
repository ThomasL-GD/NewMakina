using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeBombs : SynchronizeLoadedObjectsAbstract {

        [SerializeField] private GameObject m_prefabBomb = null;

        [SerializeField] [Range(1f, 100f)] [Tooltip("This shall be replaced by server data sent to this")] private float m_explosionRange = 5f;

        [SerializeField] private GameObject m_prefabFxBoom = null;

        private class BombInfo {
            public Transform transform;
            public float serverID;

            public BombInfo(Transform p_transform, float p_ID) {
                transform = p_transform;
                serverID = p_ID;
            }
        }

        private List<BombInfo> m_bombs = new List<BombInfo>();

        protected override void Start() {
            base.Start();
            
            MyNetworkManager.OnReceiveSpawnBomb += SpawnBomb;
            MyNetworkManager.OnReceiveBombExplosion += ExplosionFeedback;
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
            
            m_bombs.Add(new BombInfo(go.transform, p_spawnBomb.bombID));
        }

        /// <summary> Call this to make a bomb explode, will send the necessary message to the server </summary>
        /// <param name="p_index">The index of the bomb</param>
        /// <param name="p_serverID">The server ID of the bomb</param>
        public void ExplodeLol(int p_index, float p_serverID) {

            int index = FindBeaconFromID(p_index, p_serverID)??0;
            MyNetworkManager.singleton.SendVrData(new BombExplosion(){position = m_bombs[index].transform.position, index = index, bombID = m_bombs[index].serverID, hit = false});
        }

        /// <summary> Will display the feedback of an explosion </summary>
        /// <param name="p_bombExplosion">The message received from the server</param>
        private void ExplosionFeedback(BombExplosion p_bombExplosion) {
            //TODO if(p_bombExplosion)
            
            GameObject go = Instantiate(m_prefabFxBoom, p_bombExplosion.position, Quaternion.Euler(0f, 0f, 0f));
            go.GetComponent<ParticleSystem>().Play();
        }

        /// <summary> Will load a bomb in a random available position </summary>
        /// <param name="p_script">The ObjectLoading script of the beacon you want to load</param>
        public void LoadBombRandomly(ObjectLoading p_script) {
            List<int> currentAvailablePositions = new List<int>();
            for(int i = 0; i < m_availblePositions.Length; i++) if(m_availblePositions[i]) currentAvailablePositions.Add(i);

            int random = Random.Range(0, currentAvailablePositions.Count);
            LoadObjectFromIndex(p_script, currentAvailablePositions[random]);
        }
        
        /// <summary/> A function to find the index of the bomb that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted bomb </param>
        /// <param name="p_beaconID"> the ID of the wanted bomb </param>
        /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
        private int? FindBeaconFromID(int p_index, float p_beaconID)
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
