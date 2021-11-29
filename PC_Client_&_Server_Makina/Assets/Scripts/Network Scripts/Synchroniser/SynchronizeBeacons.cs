using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeacon = null;

        [SerializeField][Tooltip("The color of the beacon when the player is undetected")] private Color m_undetectedColor = Color.red;
        [SerializeField][Tooltip("The color of the beacon when the player is detected")] private Color m_detectedColor = Color.green;
        
        /// <summary/> The beacons of the player that are stored away
        private List<GameObject> m_beacons = new List<GameObject>();

        /// <summary/> Initiating the class by adding the right functions to the Client delegates
        private void Awake() {
            // Adding the right functions to the delegate
            ClientManager.OnReceiveBeaconsPositions += UpdatePositions;
            ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            ClientManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
        }

        /// <summary/> Updating the beacon positions
        /// <param name="p_beaconsPositions"></param>
        private void UpdatePositions(BeaconsPositions p_beaconsPositions) {

            // Updating the local beacon positions
            for(int i = 0; i < m_beacons.Count; i++) {
                GameObject beacon = m_beacons[i];
                if(beacon.transform.position != p_beaconsPositions.positions[i]) beacon.transform.position = p_beaconsPositions.positions[i];
            }
            
            
            //Adding a new beacon if we find new ones in the data
            if (p_beaconsPositions.positions.Length > m_beacons.Count) {
                int prevCount = m_beacons.Count;
                for (int i = 0; i < p_beaconsPositions.positions.Length - prevCount; i++) {
                    GameObject go = Instantiate(m_prefabBeacon, p_beaconsPositions.positions[prevCount + i], new Quaternion(0, 0, 0, 0));

                    m_beacons.Add(go);
                    go.name += Time.time;
                }
            }
        }

        /// <summary/> Updating the beacon's detections statuses
        /// <param name="p_beaconDetectionUpdate"></param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate)
        {
            Material mat = m_beacons[p_beaconDetectionUpdate.index].GetComponent<MeshRenderer>().material;
            mat.SetColor("_Beacon_Color", p_beaconDetectionUpdate.playerDetected?m_detectedColor:m_undetectedColor); 
        }

        /// <summary/> Destroying the beacon based in the server info
        /// <param name="p_destroyedBeacon"></param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {
            int index = p_destroyedBeacon.index;
            Destroy(m_beacons[index]);
            m_beacons.RemoveAt(index);
        }
    }
}
