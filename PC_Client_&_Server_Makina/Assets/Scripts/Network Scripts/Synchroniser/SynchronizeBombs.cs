using System;
using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBombs : MonoBehaviour {

        [SerializeField] private GameObject m_prefabBomb = null;
        

        [Serializable]
        private struct Bomb
        {
            public GameObject bombPrefabInstance;
            public float ID;

            public Bomb(GameObject p_bombPrefab, float p_id, Vector3? p_position = null)
            {
                p_bombPrefab.name += p_id.ToString();
                bombPrefabInstance = Instantiate(p_bombPrefab);
                bombPrefabInstance.transform.position = p_position?? Vector3.zero;
                    
                ID = p_id;
            }
        }

        private List<Bomb> m_bombs = new List<Bomb>();
        
        // Start is called before the first frame update
        void Start() {

            ClientManager.OnReceiveSpawnBomb += SpawnBomb;
            ClientManager.OnReceiveBombsPositions += UpdatePositions;
            ClientManager.OnReceiveBombExplosion += DestroyBomb;

        }

        /// <summary> YES ! You actually guessed, this function will spawn a freaking bomb ! </summary>
        /// <param name="p_spawnBomb">The message received by the ClientManager</param>
        private void SpawnBomb(SpawnBomb p_spawnBomb)
        {
            m_bombs.Add(new Bomb(m_prefabBomb, p_spawnBomb.bombID));
        }

        /// <summary>
        /// Updating the bombs positions
        /// This commentary was sponsored by Captain Obvious
        /// </summary>
        /// <param name="p_bombsPositions"></param>
        private void UpdatePositions(BombsPositions p_bombsPositions) {
            
            Debug.Log("update positions");
            for (int i = 0; i < m_bombs.Count; i++) {
                BombData data = p_bombsPositions.data[i];
                    
                int? index = FindBeaconFromID(i, data.bombID);

                if (index == null) {
                    Debug.LogWarning($"BOMB POSITION UPDATE ID ({data.bombID}) SEARCH FAILED");
                    return;
                }

                m_bombs[index ?? 0].bombPrefabInstance.transform.position = data.position;
            }
        }

        /// <summary>
        /// Destroying a bomb
        /// This commentary was sponsored by Captain Obvious
        /// </summary>
        /// <param name="p_bombExplosion"></param>
        private void DestroyBomb(BombExplosion p_bombExplosion)
        {

            int? index = FindBeaconFromID(p_bombExplosion.index, p_bombExplosion.bombID);
            
            if(index == null)
            {
                Debug.Log("Big L");
                return;
            }
            
            Destroy(m_bombs[index??0].bombPrefabInstance);
            m_bombs.RemoveAt(index??0);
        }

        /// <summary/> A function to find the index of the bomb that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted bomb </param>
        /// <param name="p_bombID"> the ID of the wanted bomb </param>
        /// <returns> returns the index of the bomb with the right ID if none are found, returns null </returns>
        private int? FindBeaconFromID(int p_index, float p_bombID)
        {
            int index = p_index;
            float ID = p_bombID;
            if ( index < m_bombs.Count && m_bombs[index].ID == ID) return index;

            for (int i = 0; i < m_bombs.Count; i++) if (m_bombs[i].ID == ID) return i;

            Debug.LogWarning($"I couldn't find the index matching this ID ({p_bombID}) brother",this);
            return null;
        }
    }

}