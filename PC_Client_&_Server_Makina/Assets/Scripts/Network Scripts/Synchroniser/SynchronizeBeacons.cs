using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField] private GameObject m_prefabBeacon = null;

        [SerializeField] private Color m_undetectedColor = Color.red;
        [SerializeField] private Color m_detectedColor = Color.green;
        
        private List<GameObject> m_beacons = new List<GameObject>();

        private void Awake() {
            if(m_prefabBeacon == null) Debug.LogError("You forgot to serialize a beacon prefab here ! (╬ ಠ益ಠ)", this);

            ClientManager.OnReceiveBeaconsPositions += UpdatePositions;
            ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            ClientManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            
        }

        private void UpdatePositions(BeaconsPositions p_beaconsPositions) {

            for(int i = 0; i < m_beacons.Count; i++) {
                GameObject beacon = m_beacons[i];
                if(beacon.transform.position != p_beaconsPositions.positions[i]) beacon.transform.position = p_beaconsPositions.positions[i];
            }
            
            
            //We add new beacons if we find new ones in the data
            if (p_beaconsPositions.positions.Length > m_beacons.Count) {
                int prevCount = m_beacons.Count;
                for (int i = 0; i < p_beaconsPositions.positions.Length - prevCount; i++) {
                    GameObject go = Instantiate(m_prefabBeacon, p_beaconsPositions.positions[prevCount + i], new Quaternion(0, 0, 0, 0));
                    m_beacons.Add(go);
                }
            }
        }

        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate)
        {
            Material mat = m_beacons[p_beaconDetectionUpdate.index].GetComponent<MeshRenderer>().material;
            mat.SetColor("_Beacon_Color", p_beaconDetectionUpdate.playerDetected?m_detectedColor:m_undetectedColor); 
        }

        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {
            int index = p_destroyedBeacon.index;
            Destroy(m_beacons[index]);
            m_beacons.RemoveAt(index);
        }
    }

}
