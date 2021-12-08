using CustomMessages;
using Unity.Mathematics;
using UnityEngine;

namespace Synchronizers {

    public class SynchronizeBombs : Synchronizer {

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

            // GameObject go = Instantiate(m_prefabFxBoom, p_position, quaternion.Euler(0f, 0f, 0f));
            // go.GetComponent<ParticleSystem>().Play();
            
            Destroy(gameObject);
        }

    }

}
