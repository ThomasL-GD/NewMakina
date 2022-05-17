using CustomMessages;
using Network;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Synchronizers {

    public class SynchronizeRespawn : MonoBehaviour {

        [SerializeField] [Tooltip("The prefab of what a potential spawnpoint of the pc player looks like.\nWill be instantiated at each potential spawnpoint when the pc player dies")] private GameObject m_prefabPotentialSpawnPoint;
    
        // Start is called before the first frame update
        void Start() {
            MyNetworkManager.OnReceivePotentialSpawnPoints += DisplaySpawnPoints;
        }

        private void DisplaySpawnPoints(PotentialSpawnPoints p_potentialSpawnPoints) {
            foreach (Vector3 pointPos in p_potentialSpawnPoints.position) {
                Instantiate(m_prefabPotentialSpawnPoint, pointPos, Quaternion.Euler(Vector3.zero));
            }
        }
    }

}
