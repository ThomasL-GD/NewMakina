using System.Collections.Generic;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeBombs : SynchronizeLoadedObjectsAbstract {

        [SerializeField] private GameObject m_prefabBomb = null;

        [SerializeField] [Range(1f, 100f)] [Tooltip("This shall be replaced by server data sent to this")] private float m_explosionRange = 5f;

        [SerializeField] private GameObject m_prefabFxBoom = null;

        /// <summary> Will simply spawn a bomb lol </summary>
        [ContextMenu("SpawnMenu")]
        private void SpawnBomb() {
            
            GameObject go = Instantiate(m_prefabBomb);

            if (!go.TryGetComponent(out BombBehavior script)) {
                script = go.AddComponent<BombBehavior>();
            }

            script.m_synchronizer = this;
        }

        public void ExplodeLol(Vector3 p_position) {

            GameObject go = Instantiate(m_prefabFxBoom, p_position, Quaternion.Euler(0f, 0f, 0f));
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
        
    }

}
